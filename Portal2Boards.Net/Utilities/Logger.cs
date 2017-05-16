using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Portal2Boards.Net.API.Models;

namespace Portal2Boards.Net.Utilities
{
	internal static class Logger
	{
		public static Task LogException<T>(Exception e)
			where T : class, IModel
		{
			Debug.WriteLine($"[Portal2Boards.Net] Failed to {typeof(T)} object.\n --- Stack Trace ---\n{e}\n -------------------");
			return Task.CompletedTask;
		}
	}
}
