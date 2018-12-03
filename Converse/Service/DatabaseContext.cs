using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Converse.Models;
using Converse.Utils;

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
			try
			{
				return Settings.First(s => s.Key == "LastSyncedBlockId");
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}


		public Models.Chat GetChat(string firstAddress, string secondAddress)
		{
			// ToDo: Rewrite
			return null;
		}


		public Models.User GetUser(string address)
		{
			try
			{
				return Users.FirstPredicate(user => user.Address == address);
			}
			catch (InvalidOperationException)
			{
				return null;
			}
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
			try
			{
				return BlockedUsers.FirstPredicate(bu => bu.Address == address && bu.BlockedAddress == blockedAddress);
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}
	}
}
