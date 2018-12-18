using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Converse.Models;

namespace Converse.Service
{
	public class DatabaseContext : DbContext
	{
		private struct ChatUserSetting
		{
			public User User { get; set; }
			public bool IsAdmin { get; set; }
		}

		public DbSet<Models.Setting> Settings { get; set; }

		public DbSet<Models.User> Users { get; set; }
		public DbSet<Models.UserReceivedToken> UserReceivedTokens { get; set; }
		public DbSet<Models.UserDeviceId> UserDeviceIds { get; set; }
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


		private (Chat, List<ChatUser>) CreateChat(IEnumerable<ChatUserSetting> chatUserSettings, bool isGroup, long joinedAt)
		{
			var time = DateTimeOffset.FromUnixTimeMilliseconds(joinedAt).DateTime;

			var chat = new Chat()
			{
				IsGroup = isGroup,
				CreatedAt = time
			};
			Chats.Add(chat);

			var chatUsers = chatUserSettings.Select(chatUserSetting => new ChatUser()
				{
					Chat = chat,
					User = chatUserSetting.User,
					Address = chatUserSetting.User.Address,
					IsAdmin = chatUserSetting.IsAdmin,
					JoinedAt = time,
					CreatedAt = DateTime.UtcNow,
				})
				.ToList();

			ChatUsers.AddRange(chatUsers);

			return (chat, chatUsers);
		}

		public (Chat, List<ChatUser>) CreateChat(User sender, User receiver, long joinedAt)
		{
			return CreateChat(new List<ChatUserSetting>()
			{
				new ChatUserSetting()
				{
					User = sender,
					IsAdmin = false,
				},
				new ChatUserSetting()
				{
					User = receiver,
					IsAdmin = false,
				}
			}, false, joinedAt);
		}

		public Models.User GetUser(string address, Func<IQueryable<User>, IQueryable<User>> eagerLoading = null)
		{
			return (eagerLoading == null ? Users : eagerLoading(Users)).SingleOrDefault(user => user.Address == address);
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
					CreatedAt = DateTime.UtcNow
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
