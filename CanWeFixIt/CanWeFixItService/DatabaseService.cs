using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CanWeFixItService
{
    public class DatabaseService : IDatabaseService
    {
        // See SQLite In-Memory example:
        // https://github.com/dotnet/docs/blob/main/samples/snippets/standard/data/sqlite/InMemorySample/Program.cs
        
        // Using a name and a shared cache allows multiple connections to access the same
        // in-memory database
        const string connectionString = "Data Source=DatabaseService;Mode=Memory;Cache=Shared";
        private SqliteConnection _connection;
		DatabaseContext context = new DatabaseContext();

		private readonly ILogger<DatabaseService> _logger;
		public DatabaseService(ILogger<DatabaseService> logger)
        {
			_logger = logger;
			_logger.Log(LogLevel.Information, "logger is setup in DatabaseService()");
			_logger.Log(LogLevel.Information, "Called DatabaseService()");
			// The in-memory database only persists while a connection is open to it. To manage
			// its lifetime, keep one open connection around for as long as you need it.
			_connection = new SqliteConnection(connectionString);
            _connection.Open();
			_logger.Log(LogLevel.Information, "SqliteConnection is now open");
			context = new DatabaseContext();
			_logger.Log(LogLevel.Information, "context is setup");
		}

        public async Task<IEnumerable<Instrument>> Instruments()
		{
			_logger.Log(LogLevel.Information, "Called Instruments()");
			return context.Instrument.Where(o=>o.Active == true);
        }

        public async Task<IEnumerable<MarketData>> MarketData()
        {
			_logger.Log(LogLevel.Information, "Called MarketData()");
			var data = context.MarketData.Join(context.Instrument,
                md => md.Sedol,
                i => i.Sedol,
                (md, i) => new MarketData
                {
                    Id = md.Id,
                    DataValue = md.DataValue,
                    Sedol = md.Sedol,
                    InstrumentId = i.Id,
                    Active = md.Active
                }).Where(o=>o.Active == true).ToList();

            var md = context.MarketData.Where(o => o.Active == true);
			_logger.Log(LogLevel.Information, "Market Data accessed via context");
			_logger.Log(LogLevel.Information, "Market Data joined with instrument on sedol via context");
			_logger.Log(LogLevel.Information, "return Data prepared");
			return  data;
        }

		public async Task<IEnumerable<MarketValuation>> MarketValuation()
		{
			_logger.Log(LogLevel.Information, " Called MarketValuation()");
			var data = context.MarketData.Where(o => o.Active == true);
			_logger.Log(LogLevel.Information, "Market Data accessed via context for valuation");
			MarketValuation mv = new MarketValuation() { Name = "DataValueTotal", Total = data.Sum(o => o.DataValue) };
			_logger.Log(LogLevel.Information, "return MarketValuation object created");
			return new[] { mv };
		}

		/// <summary>
		/// This is complete and will correctly load the test data.
		/// It is called during app startup 
		/// </summary>
		public void SetupDatabase()
        {
            _logger.Log(LogLevel.Information, "Called SetupDatabase()");
			_logger.Log(LogLevel.Information, "Tables going to be created");

			const string createInstruments = @"
                CREATE TABLE instrument
                (
                    id     int,
                    sedol  text,
                    name   text,
                    active int
                );
                INSERT INTO instrument
                VALUES (1, 'Sedol1', 'Name1', 0),
                       (2, 'Sedol2', 'Name2', 1),
                       (3, 'Sedol3', 'Name3', 0),
                       (4, 'Sedol4', 'Name4', 1),
                       (5, 'Sedol5', 'Name5', 0),
                       (6, '', 'Name6', 1),
                       (7, 'Sedol7', 'Name7', 0),
                       (8, 'Sedol8', 'Name8', 1),
                       (9, 'Sedol9', 'Name9', 0)";

            _connection.Execute(createInstruments);
			_logger.Log(LogLevel.Information, "Instrument created");

			const string createMarketData = @"
                CREATE TABLE marketdata
                (
                    id        int,
                    datavalue int,
                    sedol     text,
                    active    int
                );
                INSERT INTO marketdata
                VALUES (1, 1111, 'Sedol1', 0),
                       (2, 2222, 'Sedol2', 1),
                       (3, 3333, 'Sedol3', 0),
                       (4, 4444, 'Sedol4', 1),
                       (5, 5555, 'Sedol5', 0),
                       (6, 6666, 'Sedol6', 1)";
			_connection.Execute(createMarketData);
			_logger.Log(LogLevel.Information, "Market Data created");
		}
    }
}