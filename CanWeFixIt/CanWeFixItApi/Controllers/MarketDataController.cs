using System.Collections.Generic;
using System.Threading.Tasks;
using CanWeFixItService;
using Microsoft.AspNetCore.Mvc;

namespace CanWeFixItApi.Controllers
{
    [ApiController]
    [Route("v1/marketdata")]
    public class MarketDataController : ControllerBase
    {
		private readonly IDatabaseService _database;
		public MarketDataController(IDatabaseService database)
		{
			_database = database;
		}

		// GET
		public async Task<ActionResult<IEnumerable<MarketDataDto>>> Get()
        {
			return Ok(_database.MarketData().Result);
        }

	
	}
}