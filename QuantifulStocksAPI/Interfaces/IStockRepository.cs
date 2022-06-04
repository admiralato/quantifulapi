using System;
using QuantifulStocksAPI.Entities;

namespace QuantifulStocksAPI.Interfaces
{
	public interface IStockRepository
	{
		Task<IEnumerable<Stock>> GetStocks();
		Task BuildDatabase();
		Task SyncStockData(List<StockData> stocks, string symbol);
		Task<IEnumerable<StockAverage>> GetAverageVolumeByStock();
	}
}

