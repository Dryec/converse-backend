using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Utils
{
	public class ConsoleHelper
	{
		private static void Output(string prefix, string message, ConsoleColor color)
		{
			var oldFgColor = Console.ForegroundColor;
			Console.ForegroundColor = color;

			Console.WriteLine(prefix + ": " + message);

			Console.ForegroundColor = oldFgColor;
		}

		public static void Error(string message)
		{
			Output("Error", message, ConsoleColor.DarkRed);
		}

		public static void Info(string message)
		{
			Output("Info", message, ConsoleColor.DarkBlue);
		}

		public static string ReadPassword()
		{
			var password = "";

			while (true)
			{
				var key = Console.ReadKey(true);

				if (key.Key == ConsoleKey.Enter)
				{
					Console.WriteLine();
					break;
				}
				else if (key.Key != ConsoleKey.Backspace)
				{
					password += key.KeyChar;
					Console.Write("*");
				}
				else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
				{
					password = password.Substring(0, (password.Length - 1));
					Console.Write("\b \b");
				}
			}

			return password;
		}
	}
}
