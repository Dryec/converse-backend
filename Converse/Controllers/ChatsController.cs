using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse.Models;
using Converse.Service;
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

        // GET: api/Chats/all/tron_address or user_id
        [HttpGet("all/{userId}")]
        public async Task<IActionResult> GetChats([FromRoute] string userId)
        {
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var prepareUser = _context.Users
			        .Include(u => u.ChatUsers).ThenInclude(cu => cu.Chat).ThenInclude(c => c.Setting)
			        .Include(u => u.ChatUsers).ThenInclude(cu => cu.Chat).ThenInclude(c => c.Users).ThenInclude(cm => cm.User)
			        .Include(u => u.ChatUsers).ThenInclude(cu => cu.Chat).ThenInclude(c => c.Messages).ThenInclude(cm => cm.User);

			var isNumeric = int.TryParse(userId, out var id);
	        var user = await (isNumeric
			        ? prepareUser.SingleOrDefaultAsync(u => u.Id == id)
					: prepareUser.SingleOrDefaultAsync(u => u.Address == userId)
		        );
	        if (user == null)
	        {
		        return NotFound();
	        }

			var chats = new List<Models.View.Chat>();
			foreach (var chatUser in user.ChatUsers)
			{
				chats.Add(new Models.View.Chat(chatUser.Chat, user.Address));
			}

			return Content(JsonConvert.SerializeObject(chats, 
				new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore,
					DateTimeZoneHandling = DateTimeZoneHandling.Utc 
				})
			);
        }

		// GET: api/Chats/chat_id/tron_address or user_id
		[HttpGet("{chatId}/{userId?}")]
        public async Task<IActionResult> GetChat([FromRoute] string chatId, [FromRoute] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

	        var isChatIdNumeric = int.TryParse(chatId, out var chatIdAsInt);

            var preparedChat = _context.Chats
	            .Include(c => c.Setting)
	            .Include(c => c.Users).ThenInclude(u => u.User)
	            .Include(c => c.Messages).ThenInclude(u => u.User);

	        var chat = await (isChatIdNumeric
		        ? preparedChat.FirstOrDefaultAsync(c => c.Id == chatIdAsInt)
		        : preparedChat.FirstOrDefaultAsync(c => c.IsGroup && c.Setting.Address == chatId));
			if (chat == null)
            {
                return NotFound("ChatNotFound");
            }

	        if (chat.GetType() == Chat.Type.Normal)
	        {
		        if (userId == null)
		        {
			        return BadRequest("InvalidUserId");
		        }

		        var isNumeric = int.TryParse(userId, out var userIdAsInt);
		        if (isNumeric)
		        {
			        userId = (await _context.Users.FindAsync(userIdAsInt))?.Address;
			        if (userId == null)
			        {
				        return NotFound("UserNotFound");
			        }
		        }
			}

			return Content(JsonConvert.SerializeObject(new Models.View.Chat(chat, userId),
				new JsonSerializerSettings()
		        {
					NullValueHandling = NullValueHandling.Ignore,
			        DateTimeZoneHandling = DateTimeZoneHandling.Utc
		        }));
        }

	    // GET: api/Chats/Group/group_address or chat_id
	    [HttpGet("group/{chatId}")]
	    public async Task<IActionResult> GetGroup([FromRoute] string chatId)
	    {
		    if (!ModelState.IsValid)
		    {
			    return BadRequest(ModelState);
		    }

		    var isNumeric = int.TryParse(chatId, out var id);
		    Models.Chat chat;
		    if (isNumeric)
		    {
			    chat = await _context.Chats
				    .Include(c => c.Setting)
				    .Include(c => c.Users)
				    .SingleOrDefaultAsync(u => u.Id == id);
		    }
		    else
		    {
			    chat = (await _context.ChatSettings
				    .Include(cs => cs.Chat).ThenInclude(c => c.Setting)
				    .Include(cs => cs.Chat).ThenInclude(c => c.Users)
				    .SingleOrDefaultAsync(u => u.Address == chatId))?.Chat;
		    }

		    if (chat == null || chat.GetType() != Constants.Chat.Type.Group || chat.Setting == null)
		    {
			    return NotFound();
		    }

			return Content(JsonConvert.SerializeObject(new Models.View.ChatSetting(chat.Setting, chat.Users), 
				new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore,
					DateTimeZoneHandling = DateTimeZoneHandling.Utc
				}));
	    }


	    // GET: api/Chats/chat_id/messages/start_id/end_id
	    [HttpGet("{chatId}/messages/{startMessageId}/{endMessageId}")]
	    public async Task<IActionResult> GetGroup([FromRoute] int chatId, int startMessageId, int endMessageId)
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