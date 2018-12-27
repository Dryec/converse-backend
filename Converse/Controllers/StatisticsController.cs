using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Converse.Database;
using Converse.Models;

namespace Converse.Controllers
{
	public struct Statistics
	{
		public int TransactionCount { get; set; }
		public int UserCount { get; set; }
		public int MessageCount { get; set; }
		public int ChatCount { get; set; }
		public int GroupCount { get; set; }
	}

	public class StatisticsController : Controller
    {
        private readonly DatabaseContext _context;

        public StatisticsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Statistics
		[Route("statistics")]
        public async Task<IActionResult> Index()
        {
	        var transactionCount = Convert.ToInt32(_context.GetConverseTransactionCounter()?.Value);
	        var userCount = await _context.Users.CountAsync();
	        var messageCount = await _context.ChatMessages.CountAsync();

	        var chatCount = new[] {0, 0};
	        foreach (var chat in _context.Chats.GroupBy(c => c.IsGroup)
		        .Select(c => new {isGroup = c.Key, count = c.Count()}))
	        {
		        chatCount[chat.isGroup ? 1 : 0] = chat.count;
	        }

	        ViewBag.Statistics = new Statistics
	        {
		        TransactionCount = transactionCount,
		        UserCount = userCount,
		        MessageCount = messageCount,
		        ChatCount = chatCount[0],
		        GroupCount = chatCount[1],
	        };

			return View();
        }
    }
}
