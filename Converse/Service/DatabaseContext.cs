using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Converse.Service
{
	public class DatabaseContext : DbContext
	{
		public DbSet<Models.Setting> Settings { get; set; }

		public DbSet<Models.User> Users { get; set; }

		public DbSet<Models.Chat> Chats { get; set; }
		public DbSet<Models.ChatUser> ChatUsers { get; set; }
		public DbSet<Models.ChatMessage> ChatMessages { get; set; }

		public DatabaseContext(DbContextOptions contextOptions)
			: base(contextOptions)
		{
		}
	}
}
