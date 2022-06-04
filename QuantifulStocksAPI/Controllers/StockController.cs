using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuantifulStocksAPI.Entities;
using QuantifulStocksAPI.Helpers;
using QuantifulStocksAPI.Interfaces;

namespace QuantifulStocksAPI.Controllers
{
	public class StockController : BaseController
	{
		private readonly GlobalSettings _settings;
		private readonly IStockRepository _stockRepo;

		static readonly HttpClient client = new HttpClient();

		public StockController(IOptions<GlobalSettings> settings, IStockRepository stockRepo)
		{
			_settings = settings.Value;
			_stockRepo = stockRepo;
		}

        [HttpGet("/api/Stocks")]
		public async Task<ActionResult> GetStocks()
        {
			try
            {
				// Make sure the database & tables are created
				await _stockRepo.BuildDatabase();

				var stocks = await _stockRepo.GetStocks();
				return Ok(stocks);
            }
			catch(Exception ex)
            {
				return StatusCode(500, ex.StackTrace);
            }
        }

        [HttpGet("SyncStockData")]
		public async Task<ActionResult> SyncStockData([FromQuery]QueryParameters parameters,
			CancellationToken cts = default)
        {
			JsonElement errorData, metaData, weeklyData;

			try
            {
				if (parameters == null || string.IsNullOrEmpty(parameters.Symbol))
					return BadRequest();

				// Make sure the database & tables are created
				await _stockRepo.BuildDatabase();

				List<StockData> stocks = new List<StockData>();

				// Call the Vantage API
				var vantageAPI = _settings.VantageWeekStockAPI + "?function={0}&symbol={1}&apikey={2}";
				Uri queryUri = new Uri(string.Format(vantageAPI, "TIME_SERIES_WEEKLY",
					parameters.Symbol, _settings.VantageAPIKey));

				HttpResponseMessage response = await client.GetAsync(queryUri);
				response.EnsureSuccessStatusCode();

				//var jsonData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: cts);
				var jsonData = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cts));

				bool hasError = jsonData.RootElement.TryGetProperty("Error Message", out errorData);
				bool hasMetaData = jsonData.RootElement.TryGetProperty("Meta Data", out metaData);
				bool hasWeeklyData = jsonData.RootElement.TryGetProperty("Weekly Time Series", out weeklyData);

				if (jsonData != null)
                {
                    // Check if API returned an error message
                    if (hasError)
                    {
                        return StatusCode(400, "Invalid API call or symbol not found.");
                    }

                    if (hasMetaData && hasWeeklyData)
                    {
						var dataDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(weeklyData);

						foreach(var(key, value) in dataDict)
                        {
							// Build the Stock object
							StockData stock = new StockData();
							stock.Name = metaData.GetProperty("2. Symbol").ToString().ToUpper();
							stock.Date = Convert.ToDateTime(key).Date;

							foreach(var(k, v) in value)
                            {
								double open = 0;
								double high = 0;
								double low = 0;
								double close = 0;
								double volume = 0;

								if (k.ToString().ToLower().Equals("1. open"))
                                {
									Double.TryParse(v.ToString(), out open);
									stock.Open = open;
								}

								if(k.ToString().ToLower().Equals("2. high"))
                                {
									Double.TryParse(v.ToString(), out high);
									stock.High = high;
                                }

								if(k.ToString().ToLower().Equals("3. low"))
                                {
									Double.TryParse(v.ToString(), out low);
									stock.Low = low;
                                }

								if(k.ToString().ToLower().Equals("4. close"))
                                {
									Double.TryParse(v.ToString(), out close);
									stock.Close = close;
                                }

								if(k.ToString().ToLower().Equals("5. volume"))
                                {
									Double.TryParse(v.ToString(), out volume);
									stock.Volume = volume;
                                }
                            }

							// Add it to the list
							stocks.Add(stock);
                        }

						if(stocks.Count > 0)
							await _stockRepo.SyncStockData(stocks, parameters.Symbol);
					}
                }
				else
                {
					return StatusCode(400, "Invalid stock symbol.");
                }
                return Ok(stocks);
            }
			catch(Exception ex)
            {
				return StatusCode(500, ex.StackTrace);
            }
        }

        [HttpGet("GetStocksAverage")]
		public async Task<ActionResult> GetStocksAverage()
        {
			try
            {
				// Make sure the database & tables are created
				await _stockRepo.BuildDatabase();

				var stocksAverage = await _stockRepo.GetAverageVolumeByStock();

				if(stocksAverage.Count() > 0)
                {
					using (var writer = new StreamWriter(_settings.AssetsDirectoryPath + "/stocks-avg-" + DateTime.Now.ToFileTimeUtc() + ".csv"))
					using(var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
						csv.WriteRecords(stocksAverage);
                    }
                }

				return Ok(stocksAverage);
            }
			catch(Exception ex)
            {
				return StatusCode(500, ex);
            }
        }
	}
}

