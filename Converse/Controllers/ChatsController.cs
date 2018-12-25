using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converse.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse.Models;
using Newtonsoft.Json;
using Chat = Converse.Constants.Chat;

namespace Converse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public ChatsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Chats/all/{userAddress}
		// Return all chats from passed user address
        [HttpGet("all/{userAddress}")]
        public async Task<IActionResult> GetAllUserChats([FromRoute] string userAddress)
        {
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var prepareUser = _context.Users
			        .Include(u => u.ChatUsers).ThenInclude(cu => cu.Chat).ThenInclude(c => c.Setting)
			        .Include(u => u.ChatUsers).ThenInclude(cu => cu.Chat).ThenInclude(c => c.Users).ThenInclude(cm => cm.User)
			        .Include(u => u.ChatUsers).ThenInclude(cu => cu.Chat).ThenInclude(c => c.Messages).ThenInclude(cm => cm.User);

	        var user = await prepareUser.SingleOrDefaultAsync(u => u.Address == userAddress);
	        if (user == null)
	        {
		        return NotFound();
	        }

			var chats = new List<Models.View.Chat>();
			foreach (var chatUser in user.ChatUsers)
			{
				chats.Add(new Models.View.Chat(chatUser.Chat, user.Address, false));
			}

			return Content(JsonConvert.SerializeObject(chats, 
				new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore,
					DateTimeZoneHandling = DateTimeZoneHandling.Utc 
				})
			);
        }

		// GET: api/Chats/{chat_id}/{userAddress}
		// Return more detailed information about a chat (When normal chat type, chat partner included, for group users use another call)
		[HttpGet("{chatId}/{userAddress}")]
        public async Task<IActionResult> GetUserChat([FromRoute] string chatId, [FromRoute] string userAddress)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

	        var chat = await _context.GetChatAsync(chatId, chats => chats
		        .Include(c => c.Setting)
		        .Include(c => c.Users).ThenInclude(u => u.User)
		        .Include(c => c.Messages).ThenInclude(m => m.User));
			if (chat == null)
            {
                return NotFound("ChatNotFound");
            }
			
			return Content(JsonConvert.SerializeObject(new Models.View.Chat(chat, userAddress),
				new JsonSerializerSettings()
		        {
					NullValueHandling = NullValueHandling.Ignore,
			        DateTimeZoneHandling = DateTimeZoneHandling.Utc
		        }));
        }

		// GET: api/Chats/Group/{groupAddress or chat_id}/{userAddress}
		// Return more detailed information about a group chat included users, name, descriptions, image
		[HttpGet("group/{chatId}/{userAddress}")]
	    public async Task<IActionResult> GetGroup([FromRoute] string chatId, [FromRoute] string userAddress)
	    {
		    if (!ModelState.IsValid)
		    {
			    return BadRequest(ModelState);
		    }

		    var chat = await _context.GetChatAsync(chatId, chats => chats
			    .Include(c => c.Setting)
			    .Include(c => c.Users).ThenInclude(cu => cu.User));
		    if (chat == null || chat.GetType() != Constants.Chat.Type.Group || chat.Setting == null)
		    {
			    return NotFound();
		    }

			return Content(JsonConvert.SerializeObject(new Models.View.ChatSetting(chat.Setting, userAddress, chat.Users), 
				new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore,
					DateTimeZoneHandling = DateTimeZoneHandling.Utc
				}));
	    }

	    // GET: api/Chats/{chatId}/messages/{startId}/{endId}
		// Return messages in a chat from passed range
	    [HttpGet("{chatId}/messages/{startMessageId}/{endMessageId}")]
	    public async Task<IActionResult> GetGroupMessages([FromRoute] int chatId, int startMessageId, int endMessageId)
	    {
		    if (startMessageId > endMessageId || startMessageId < 0 || endMessageId < 0)
		    {
			    return BadRequest("InvalidRange");
		    } else if (endMessageId - startMessageId > 50)
		    {
			    return BadRequest("MaximumExceeded");
			}

			if (!ModelState.IsValid)
		    {
			    return BadRequest(ModelState);
		    }

		    var chat = await _context.Chats
				    .Include(c => c.Messages).ThenInclude(cm => cm.User)
				    .SingleOrDefaultAsync(u => u.Id == chatId);

		    if (chat == null)
		    {
			    return NotFound();
		    }

			var chatMessages = chat.Messages.Where(cm => cm.InternalId >= startMessageId && cm.InternalId <= endMessageId).OrderBy(cm => cm.InternalId).ToList();
		    if (!chatMessages.Any())
		    {
			    startMessageId = endMessageId = 0;
		    }
		    else
		    {
			    startMessageId = chatMessages.First().InternalId;
			    endMessageId = chatMessages.Last().InternalId;
		    }

			return Content(JsonConvert.SerializeObject(
				new Models.View.ChatMessagesRange(chatId, startMessageId, endMessageId, chatMessages),
			    new JsonSerializerSettings()
			    {
					NullValueHandling = NullValueHandling.Ignore,
				    DateTimeZoneHandling = DateTimeZoneHandling.Utc
			    }));
	    }
	}
}