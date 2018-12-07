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
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public UsersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Users/5
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

	        var isNumeric = int.TryParse(userId, out var id);
            var user = await (isNumeric 
	            ? _context.Users.FindAsync(id) 
	            : _context.Users.SingleOrDefaultAsync(u => u.Address == userId)
		    );

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new Models.View.User(user));
        }
    }
}