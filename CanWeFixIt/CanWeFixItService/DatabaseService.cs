﻿using System;
using System.Collections.Generic;
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
		const string connectionString = "Data Source=DatabaseService;Mode=Memory;Cache=Shared";
		private SqliteConnection _connection;
		DatabaseContext context = new DatabaseContext();

		private readonly ILogger<DatabaseService> _logger;
		public DatabaseService(ILogger<DatabaseService> logger)
		{
			try
			{
				_logger = logger;
				_logger.Log(LogLevel.Information, "logger is setup in DatabaseService class.");
				_logger.Log(LogLevel.Information, "Called DatabaseService()");
				_connection = new SqliteConnection(connectionString);
				_connection.Open();
				_logger.Log(LogLevel.Information, "SqliteConnection is now open");
				context = new DatabaseContext();
				_logger.Log(LogLevel.Information, "context is setup");
			}
			catch (Exception e)
			{
				_logger.Log(LogLevel.Error, $"DatabaseService:{e.Message}");
			}
		}

		public async Task<IEnumerable<Instrument>> Instruments()
		{
			try
			{
				_logger.Log(LogLevel.Information,connectionString, "All active instruments are called." );
				return await context.Instrument.Where(o => o.Active == true).ToListAsync();
			}
			catch (Exception e)
			{
				_logger.Log(LogLevel.Information, "Exception occured during calling all active instruments.");
				_logger.Log(LogLevel.Error, $"Instruments():{e.Message}");
				return null;
			}
		}

		public async Task<IEnumerable<MarketData>> MarketData()
		{
			try
			{
				_logger.Log(LogLevel.Information, "Called MarketData()");
				var data = await context.MarketData.Join(context.Instrument,
					md => md.Sedol,
					i => i.Sedol,
					(md, i) => new MarketData
					{
						Id = md.Id,
						DataValue = md.DataValue,
						Sedol = md.Sedol,
						InstrumentId = i.Id,
						Active = md.Active
					}).Where(o => o.Active == true).ToListAsync();

				_logger.Log(LogLevel.Information, "Market Data accessed via context");
				_logger.Log(LogLevel.Information, "Market Data joined with instrument on sedol via context");
				_logger.Log(LogLevel.Information, "return Data prepared");
				return data;
			}
			catch (Exception e)
			{
				_logger.Log(LogLevel.Error, $"MarketData():{e.Message}");
				return null;
			}
		}

		public async Task<IEnumerable<MarketValuation>> MarketValuation()
		{
			try
			{
				_logger.Log(LogLevel.Information, " Called MarketValuation()");
				var data = await context.MarketData.Where(o => o.Active == true).ToListAsync();
				_logger.Log(LogLevel.Information, "Market Data accessed via context for valuation");
				MarketValuation mv = new MarketValuation() { Name = "DataValueTotal", Total = data.Sum(o => o.DataValue) };
				_logger.Log(LogLevel.Information, "return MarketValuation object created");
				return new[] { mv };
			}
			catch (Exception e)
			{
				_logger.Log(LogLevel.Error, $"MarketValuation():{e.Message}");
				return null;
			}
		}

		/// <summary>
		/// This is complete and will correctly load the test data.
		/// It is called during app startup 
		/// </summary>
		public void SetupDatabase()
		{
			try
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
			catch (Exception e)
			{
				_logger.Log(LogLevel.Error, $"SetupDatabase():{e.Message}");
			}
		}
	}
}