using System;
using QuantifulStocksAPI.Entities;

namespace QuantifulStocksAPI.Helpers
{
	public class GlobalSettings
	{
		public const string Settings = "GlobalSettings";
		public string VantageAPIKey { get; set; }
		public string VantageWeekStockAPI { get; set; }
		public string AssetsDirectoryPath { get; set; }
	}
}

