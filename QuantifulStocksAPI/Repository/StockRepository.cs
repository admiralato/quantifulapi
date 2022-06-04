using Dapper;
using QuantifulStocksAPI.Context;
using QuantifulStocksAPI.Entities;
using QuantifulStocksAPI.Helpers;
using QuantifulStocksAPI.Interfaces;

namespace QuantifulStocksAPI.Repository
{
	public class StockRepository : IStockRepository
	{
        private readonly DapperContext _context;

		public StockRepository(DapperContext context)
		{
            _context = context;
		}

        public async Task BuildDatabase()
        {
            try
            {
                using var connection = _context.CreateConnection();
                var table = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'StockData'");
                var tableName = table.FirstOrDefault();
                if (!string.IsNullOrEmpty(tableName) && tableName == "StockData")
                    return;

                await connection.ExecuteAsync("CREATE TABLE StockData (" +
                    "Name VARCHAR(100) NOT NULL," +
                    "Date TEXT NOT NULL, " +
                    "Open REAL NOT NULL," +
                    "High REAL NOT NULL," +
                    "Low REAL NOT NULL," +
                    "Close REAL NOT NULL," +
                    "Volume REAL NOT NULL);");

                // Check if Stocks table is already created
                var stockable = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name = 'Stocks'");
                var stockTableName = stockable.FirstOrDefault();
                if (!string.IsNullOrEmpty(stockTableName) && stockTableName == "Stocks")
                    return;

                await connection.ExecuteAsync("CREATE TABLE Stocks (" +
                    "Symbol VARCHAR(100) NOT NULL," +
                    "Name VARCHAR(100) NOT NULL);");

                // Pre-create Stock table data
                await connection.ExecuteAsync("INSERT INTO Stocks (Symbol, Name) VALUES('TWTR', 'Twitter');");
                await connection.ExecuteAsync("INSERT INTO Stocks (Symbol, Name) VALUES('TSLA', 'Tesla');");
                await connection.ExecuteAsync("INSERT INTO Stocks (Symbol, Name) VALUES('AMZN', 'Amazon');");
            }
            catch(Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public Task<IEnumerable<StockAverage>> GetAverageVolumeByStock()
        {
            try
            {
                using var connection = _context.CreateConnection();

                var query = "SELECT Name, strftime('%Y', Date) Year, AVG(Volume) Average FROM StockData group by Name,  Year ORDER BY Year DESC";
                return connection.QueryAsync<StockAverage>(query);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public Task<IEnumerable<Stock>> GetStocks()
        {
            try
            {
                using var connection = _context.CreateConnection();
                return connection.QueryAsync<Stock>("SELECT * FROM Stocks");

            }
            catch(Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        public async Task SyncStockData(List<StockData> stocks, string symbol)
        {
            try
            {
                List<StockData> forUpdateList = new List<StockData>();
                List<StockData> forInsertList = new List<StockData>();

                using var connection = _context.CreateConnection();

                // Get data for this stock symbol
                var forUpdate = await connection.QueryAsync<StockData>(string.Format("SELECT * FROM StockData WHERE Name LIKE '{0}'", symbol));
                if(forUpdate != null && forUpdate.Count() > 0)
                {
                    // Check if there are similar records from the database, just update them
                    forUpdateList = forUpdate.ToList().Intersect(stocks, new StockComparer()).ToList();
                    // Insert those that aren't yet in the database
                    forInsertList = stocks.Except(forUpdate, new StockComparer()).ToList();

                }
                else
                {
                    // Insert all stocks to the table
                    forInsertList = stocks;
                }

                foreach(var stock in forUpdateList)
                {
                    var query = string.Format("UPDATE StockData SET Open=@Open, High=@High, Low=@Low, Close=@Close, Volume=@Volume WHERE Name LIKE @Name AND Date=@Date");
                    await connection.ExecuteAsync(query, stock);
                }

                foreach(var stock in forInsertList)
                {
                    var query = "INSERT INTO StockData (Name, Date, Open, High, Low, Close, Volume) VALUES (@Name, @Date, @Open, @High, @Low, @Close, @Volume)";
                    await connection.ExecuteAsync(query, stock);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }
    }
}

