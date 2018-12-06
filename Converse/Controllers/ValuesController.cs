using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Converse.Utils;

namespace Converse.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private readonly Service.DatabaseContext _databaseContext;

		public ValuesController(Service.DatabaseContext databaseContext)
		{
			_databaseContext = databaseContext;

			var chat = _databaseContext.GetChat("Fourth", "Second");
			if (chat != null)
			{
				Console.WriteLine("GroupId: " + chat.Id);
				Console.WriteLine("GroupId: " + chat.Id);
				Console.WriteLine("GroupId: " + chat.Id);
				Console.WriteLine("GroupId: " + chat.Id);
				Console.WriteLine("GroupId: " + chat.Id);
				Console.WriteLine("GroupId: " + chat.Id);
				Console.WriteLine("GroupId: " + chat.Id);
			}
		}

		// GET api/values
		[HttpGet]
		public ActionResult<string> Get()
		{
			return "yep";
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
