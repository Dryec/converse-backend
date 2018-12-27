using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Converse.Models;
using System.Threading.Tasks;

namespace Converse.Database
{
	public class DatabaseContext : DbContext
	{
		public struct ChatUserSetting
		{
			public User User { get; set; }
			public ChatUserRank Rank { get; set; }
			public string PrivateKey { get; set; }
		}

		public struct ChatGroupInfo
		{
			public string Address { get; set; }
			public string Name { get; set; }
			public string Image { get; set; }
			public string Description { get; set; }
			public string PublicKey { get; set; }
			public string PrivateKey { get; set; }
			public bool IsPublic { get; set; }
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

		public DbSet<Models.Subscriber> Subscriptions { get; set; }

		public DatabaseContext(DbContextOptions contextOptions)
			: base(contextOptions)
		{
		}

		public Models.Setting GetLastSyncedBlock()
		{
			return Settings.FirstOrDefault(s => s.Key == "LastSyncedBlockId");
		}

		public Models.Setting GetConverseTransactionCounter()
		{
			return Settings.FirstOrDefault(s => s.Key == "ConverseTransactionCounter");
		}


		public async Task<Models.Chat> GetChatAsync(string firstAddress, string secondAddress, Func<IQueryable<Models.Chat>, IQueryable<Models.Chat>> eagerLoading = null)
		{
			return (await ChatUsers
				.Include(cu => cu.Chat)
				.Where(cu => cu.Address == firstAddress &&
				             ChatUsers.Any(cuSecond => cu.ChatId == cuSecond.ChatId && cuSecond.Address == secondAddress)
				)
				.FirstOrDefaultAsync(cu => !cu.Chat.IsGroup))?.Chat;
		}

		public async Task<Models.Chat> GetChatAsync(string chatId, Func<IQueryable<Models.Chat>, IQueryable<Models.Chat>> eagerLoading = null)
		{
			var preparedChat = (eagerLoading == null ? Chats : eagerLoading(Chats));
			var isChatIdNumeric = int.TryParse(chatId, out var chatIdAsInt);

			return await (isChatIdNumeric
						? preparedChat.FirstOrDefaultAsync(c => c.Id == chatIdAsInt)
						: preparedChat.FirstOrDefaultAsync(c => c.IsGroup && c.Setting.Address == chatId)
					);
		}


		private Chat CreateChat(IEnumerable<ChatUserSetting> chatUserSettings, bool isGroup, long userJoinedAt, ChatGroupInfo? chatGroupInfo)
		{
			if (isGroup && !chatGroupInfo.HasValue)
			{
				throw new ArgumentNullException(nameof(chatGroupInfo));
			}

			var time = DateTimeOffset.FromUnixTimeMilliseconds(userJoinedAt).DateTime;

			var chat = new Chat()
			{
				IsGroup = isGroup,
				CreatedAt = time
			};
			Chats.Add(chat);

			Models.ChatSetting chatSetting = null;

			if (isGroup)
			{
				chatSetting = new Models.ChatSetting()
				{
					Chat = chat,
					Address = chatGroupInfo.Value.Address,
					Name = chatGroupInfo.Value.Name,
					Description = chatGroupInfo.Value.Description,
					PictureUrl = chatGroupInfo.Value.Image,
					PublicKey = chatGroupInfo.Value.PublicKey,
					PrivateKey = chatGroupInfo.Value.PrivateKey,
					IsPublic = chatGroupInfo.Value.IsPublic,
					CreatedAt = time,
				};

				ChatSettings.Add(chatSetting);
			}

			foreach (var userSetting in chatUserSettings)
			{
				CreateChatUser(chat, userSetting, userJoinedAt);
			}

			chat.Setting = chatSetting;

			return chat;
		}

		public ChatUser CreateChatUser(Models.Chat chat, ChatUserSetting chatUserSetting, long joinedAt)
		{
			var chatUser = new ChatUser()
			{
				Chat = chat,
				User = chatUserSetting.User,
				Address = chatUserSetting.User.Address,
				Rank = chatUserSetting.Rank,
				PrivateKey = chatUserSetting.PrivateKey,
				JoinedAt = DateTimeOffset.FromUnixTimeMilliseconds(joinedAt).DateTime,
				CreatedAt = DateTime.UtcNow,
			};

			ChatUsers.Add(chatUser);

			return chatUser;
		}

		public Chat CreateChat(User sender, User receiver, long userJoinedAt)
		{
			return CreateChat(new List<ChatUserSetting>()
			{
				new ChatUserSetting()
				{
					User = sender,
					Rank = ChatUserRank.User,
					PrivateKey = null,
				},
				new ChatUserSetting()
				{
					User = receiver,
					Rank = ChatUserRank.User,
					PrivateKey = null,
				}
			}, false, userJoinedAt, null);
		}

		public Chat CreateGroupChat(User owner, string privateKey, ChatGroupInfo chatGroupInfo, long userJoinedAt)
		{
			return CreateChat(new List<ChatUserSetting>()
			{
				new ChatUserSetting()
				{
					User = owner,
					Rank = ChatUserRank.Owner,
					PrivateKey = privateKey
				},
			}, true, userJoinedAt, chatGroupInfo);
		}


		public async Task<Models.User> GetUserAsync(string address, Func<IQueryable<User>, IQueryable<User>> eagerLoading = null)
		{
			return await (eagerLoading == null ? Users : eagerLoading(Users)).SingleOrDefaultAsync(user => user.Address == address);
		}


		public Models.User CreateUserWhenNotExist(string address, Func<IQueryable<User>, IQueryable<User>> eagerLoading = null)
		{
			var user = GetUserAsync(address, eagerLoading).GetAwaiter().GetResult();

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
			return addresses.Select(a => CreateUserWhenNotExist(a)).ToList();
		}


		public Models.BlockedUser GetBlockedUser(string address, string blockedAddress)
		{
			return BlockedUsers.FirstOrDefault(bu => bu.Address == address && bu.BlockedAddress == blockedAddress);
		}
	}
}
