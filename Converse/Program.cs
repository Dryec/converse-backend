using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Client;
using Crypto;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math;

namespace Converse
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.Title = "[TronSociety] - Converse Server";

			try
			{
				var encryptedPrivateKey = File.ReadAllText("propertyAddress");
				if (encryptedPrivateKey != "")
				{
					Console.Write("Enter the Property-Address Password: ");
					var password = "123456789ABCDEFGHIJKLMNOPQRSTUVX";//Utils.ConsoleHelper.ReadPassword();
					var privateKey = NETCore.Encrypt.EncryptProvider.AESDecrypt(encryptedPrivateKey, password);

					Service.WalletClient.PropertyAddress = new Client.Wallet(ECKey.FromPrivateHexString(privateKey));

					CreateWebHostBuilder(args).Build().Run();
				}
				else
				{
					Utils.ConsoleHelper.Error("There is no Property-Address configured!");
				}
			}
			catch (Exception e)
			{
				Utils.ConsoleHelper.Error("Couldn't setup the Property-Address.");
				Utils.ConsoleHelper.Info("Make sure to create a 'propertyAddress' file with an AES-Encrypted private key.");
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			Console.ReadLine();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, builder) => { builder.AddJsonFile("blockchain.json"); })
				.UseStartup<Startup>();
	}
}
