using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using FirebaseNet.Messaging;
using Microsoft.Extensions.Configuration;

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

		public async Task<IFCMResponse> SendMessage(string receiver, INotification notification)
		{
			if (!_isInitialized)
			{
				return null;
			}

			var message = new Message()
			{
				To = receiver,
				Notification = notification
			};

			return await _client.SendMessageAsync(message);
		}
	}
}
