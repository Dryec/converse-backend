using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse.Database;
using Converse.Models;

namespace Converse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public StatisticsController(DatabaseContext context)
        {
            _context = context;
        }

		// GET: api/Statistics/
		// Return some statistics about converse
		[HttpGet("")]
		public async Task<IActionResult> GetStatistics()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

	        var transactionCount = Convert.ToInt32(_context.GetConverseTransactionCounter()?.Value);
	        var userCount = await _context.Users.CountAsync();
	        var messageCount = await _context.ChatMessages.CountAsync();

	        var chatCount = new[] { 0, 0 };
	        foreach (var chat in _context.Chats.GroupBy(c => c.IsGroup).Select(c => new { isGroup = c.Key, count = c.Count()}))
	        {
		        chatCount[chat.isGroup ? 1 : 0] = chat.count;
	        }

			return Ok(new
			{
				transactionCount,
				userCount,
				messageCount,
				chatCount = chatCount[0],
				groupCount = chatCount[1],
			});
        }
    }
}