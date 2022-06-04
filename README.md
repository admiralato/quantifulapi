# QuantifulAPI Job Application Exercises
Required NuGet Packages:
  - CsvHelper
  - Dapper
  - Microsoft.Data.Sqlite

Host: MS Azure

# Task 1
Part 1:
  - API: https://quantifulstocksapi.azurewebsites.net/api/stocks/SyncStockData?symbol=<STOCK_SYMBOL>
  - Method: GET
  - Parameters:
      - Symbol: Required. Pass the stock symbol that you need to sync weekly data

Part 2:
  - API: https://quantifulstocksapi.azurewebsites.net/api/Stocks/GetStocksAverage
  - Method: GET
  - Parameters: None
  - Description: When loading the URL, it will download the CSV file to your machine. Make sure to populate the table by running the API on Part 1.

Part 3:
  - This could be achieved by creating a service that will populate the stocks data based on a configurable day(s) of week and time.
  - The service will check either a config file or a database table where frequency will be configured.
  - The service can either be a Window Service, a Task that you will need to point the application that will run from the Windows Task Manager, or it could be a CRON job.
  - The data will be stored in the database, I think PART 1 API (SyncStockData) is already doing this, just need to set the other requirements
  - This submission/implementation doesn't include logging, but there should be logging of errors in a log file to troubleshoot errors.

# Task 2
  - API: https://quantifulstocksapi.azurewebsites.net/api/Stocks
  - Method: GET
  - Parameters: None
