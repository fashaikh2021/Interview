using System.Collections.Generic;
using System.Threading.Tasks;
using CanWeFixItService;
using Microsoft.AspNetCore.Mvc;

namespace CanWeFixItApi.Controllers
{
    [ApiController]
    [Route("v1/valuations")]
    public class ValuationsController : ControllerBase
    {
		private readonly IDatabaseService _database;
		public ValuationsController(IDatabaseService database)
		{
			_database = database;
		}

	
		// GET
		public async Task<ActionResult<IEnumerable<MarketValuationDto>>> Get()
		{
			return Ok( _database.MarketValuation().Result);
		}
	}
}