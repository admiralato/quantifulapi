using System;
using Microsoft.Data.Sqlite;

namespace QuantifulStocksAPI.Context
{
	public class DapperContext
	{
		private readonly IConfiguration _configuration;
		private readonly string _connectionString;

		public DapperContext(IConfiguration configuration)
		{
			_configuration = configuration;
			_connectionString = _configuration.GetConnectionString("DefaultConnection");
		}

		public SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);
	}
}

