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
    public class SubscribersController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public SubscribersController(DatabaseContext context)
        {
            _context = context;
        }

		// GET: api/Subscribers/{email}/{action}
		// Add/Removes an email from subscription. (Type -> 0 = Add, 1 = Remove)
		[HttpGet("{eMail}/{type}")]
        public async Task<IActionResult> GetSubscriber([FromRoute] string eMail, [FromRoute] int type)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

	        if (type != 0 && type != 1)
	        {
		        return BadRequest();
	        }

	        eMail = eMail.ToLower();

	        var subscriber = await _context.Subscriptions.SingleOrDefaultAsync(s => s.EMail == eMail);
			if (type == 0)
	        {
		        if (subscriber == null)
		        {
			        _context.Subscriptions.Add(new Subscriber()
			        {
						EMail =  eMail,
						CreatedAt = DateTime.UtcNow,
			        });
					await _context.SaveChangesAsync();
		        }
			}
	        else if (subscriber != null)
			{
				_context.Subscriptions.Remove(subscriber);
				await _context.SaveChangesAsync();
			}

			return Ok();
        }
    }
}