using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CanWeFixItService
{
	public class DatabaseContext : DbContext
	{
		private readonly ILogger<DatabaseService> _logger;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(@"Data Source=DatabaseService;Mode=Memory;Cache=Shared", options =>
			 {
				 options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
			 });
			base.OnConfiguring(optionsBuilder);
		}



		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Instrument>().ToTable("Instrument", "Instrument");
			modelBuilder.Entity<MarketData>().ToTable("MarketData", "MarketData");
			base.OnModelCreating(modelBuilder);
		}

		public DbSet<Instrument> Instrument { get; set; }
		public DbSet<MarketData> MarketData { get; set; }
	}
}
