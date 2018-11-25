using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Converse.Models;

namespace Converse.Service
{
	public class DatabaseContext : DbContext
	{
		public DbSet<Models.Setting> Settings { get; set; }

		public DbSet<Models.User> Users { get; set; }
		public DbSet<Models.BlockedUser> BlockedUsers { get; set; }

		public DbSet<Models.Chat> Chats { get; set; }
		public DbSet<Models.ChatSetting> ChatSettings { get; set; }
		public DbSet<Models.ChatUser> ChatUsers { get; set; }
		public DbSet<Models.ChatMessage> ChatMessages { get; set; }

		public DatabaseContext(DbContextOptions contextOptions)
			: base(contextOptions)
		{
		}

		public Models.Chat GetChat(string firstAddress, string secondAddress)
		{
			// ToDo: Rewrite
			return null;
			//try
			//{
			//	return this.Chats
			//		.First(c => (c.FirstAddress == firstAddress && c.SecondAddress == secondAddress) ||
			//		            (c.FirstAddress == secondAddress && c.SecondAddress == firstAddress));
			//}
			//catch (InvalidOperationException)
			//{
			//	return null;
			//}
		}

		public Models.User CreateUserWhenNotExist(string address)
		{
			try
			{
				return this.Users.First(u => u.Address == address);
			}
			catch (InvalidOperationException)
			{
				Models.User user = new User
				{
					Address = address,
					CreatedAt = DateTime.Now
				};

				this.Users.Add(user);

				return user;
			}
		}
	}
}
