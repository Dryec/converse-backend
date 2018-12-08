using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Converse.Models;

namespace Converse.Service
{
	public class DatabaseContext : DbContext
	{
		public DbSet<Models.Setting> Settings { get; set; }

		public DbSet<Models.User> Users { get; set; }
		public DbSet<Models.UserReceivedToken> UserReceivedTokens { get; set; }
		public DbSet<Models.BlockedUser> BlockedUsers { get; set; }

		public DbSet<Models.Chat> Chats { get; set; }
		public DbSet<Models.ChatSetting> ChatSettings { get; set; }
		public DbSet<Models.ChatUser> ChatUsers { get; set; }
		public DbSet<Models.ChatMessage> ChatMessages { get; set; }


		public DatabaseContext(DbContextOptions contextOptions)
			: base(contextOptions)
		{
		}

		public Models.Setting GetLastSyncedBlock()
		{
			return Settings.FirstOrDefault(s => s.Key == "LastSyncedBlockId");
		}

		public Models.Chat GetChat(string firstAddress, string secondAddress)
		{
			return ChatUsers
				.Include(cu => cu.Chat)
				.Where(cu => cu.Address == firstAddress &&
				             ChatUsers.Any(cuSecond => cu.ChatId == cuSecond.ChatId && cuSecond.Address == secondAddress)
				)
				.FirstOrDefault(cu => !cu.Chat.IsGroup)?.Chat;
		}

		public Models.User GetUser(string address)
		{
			return Users.SingleOrDefault(user => user.Address == address);
		}

		public Models.User CreateUserWhenNotExist(string address)
		{
			var user = GetUser(address);

			if (user == null)
			{
				user = new User
				{
					Address = address,
					Nickname = null,
					Status = null,
					ProfilePictureUrl = null,
					CreatedAt = DateTime.Now
				};

				Users.Add(user);
			}

			return user;
		}

		public List<Models.User> CreateUsersWhenNotExist(IEnumerable<string> addresses)
		{
			return addresses.Select(CreateUserWhenNotExist).ToList();
		}

		public Models.BlockedUser GetBlockedUser(string address, string blockedAddress)
		{
			return BlockedUsers.FirstOrDefault(bu => bu.Address == address && bu.BlockedAddress == blockedAddress);
		}
	}
}
