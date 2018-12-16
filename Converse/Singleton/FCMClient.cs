using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using FirebaseNet.Messaging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Converse.Singleton
{
	public class FCMClient
	{
		public static string ServerKey { get; set; }

		private FirebaseNet.Messaging.FCMClient _client;
		private bool _isInitialized;

		public void Initialize(IConfigurationSection configuration)
		{
			if (_isInitialized)
			{
				return;
			}

			var serverKey = configuration.GetValue<string>("ServerKey");
			_client = new FirebaseNet.Messaging.FCMClient(serverKey);

			_isInitialized = true;
		}

		public async Task<IFCMResponse> SendMessage<T>(string receiver, string id, string tag, T data, INotification notification, MessagePriority priority)
			where T : class
		{
			if (!_isInitialized)
			{
				return null;
			}

			var message = new Message()
			{
				To = receiver,
				Data = (data != null
					? new Dictionary<string, string>()
					{
						{
							"id", id
						},
						{
							"tag", tag
						},
						{
							"data", JsonConvert.SerializeObject(data)
						},
						{
							"silent", (notification == null).ToString().ToLower()
						},
						{
							"priority", priority.ToString()
						}
					}
					: null),
				Notification = notification,
				Priority = priority,
			};

			return await _client.SendMessageAsync(message);
		}
	}
}
