using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Converse.Models;
using Converse.Service;

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

			return Ok(chats);
        }

        // GET: api/Chats/tron_address or user_id/chat_id
        [HttpGet("{userId}/{chatId}")]
        public async Task<IActionResult> GetChat([FromRoute] string userId, [FromRoute] int chatId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var chat = await _context.Chats
	            .Include(c => c.Setting)
	            .Include(c => c.Users).ThenInclude(u => u.User)
	            .Include(c => c.Messages).ThenInclude(u => u.User)
	            .FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null)
            {
                return NotFound("ChatNotFound");
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

			return Ok(new Models.View.Chat(chat, userId));
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
		    Models.Chat chat = null;
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

			return Ok(new Models.View.ChatSetting(chat.Setting, chat.Users));
	    }


	    // GET: api/Chats/chat_id/messages/start_id/end_id
	    [HttpGet("{chatId}/messages/{startMessageId}/{endMessageId}")]
	    public async Task<IActionResult> GetGroup([FromRoute] int chatId, int startMessageId, int endMessageId)
	    {
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

			var chatMessages = chat.Messages.Where(cm => cm.InternalId >= startMessageId && cm.InternalId <= endMessageId).ToList();
		    if (!chatMessages.Any())
		    {
			    startMessageId = endMessageId = 0;
		    }
		    else
		    {
			    startMessageId = chatMessages.First().InternalId;
			    endMessageId = chatMessages.Last().InternalId;
		    }

			return Ok(new Models.View.ChatMessagesRange(chatId, startMessageId, endMessageId, chatMessages));
	    }
	}
}