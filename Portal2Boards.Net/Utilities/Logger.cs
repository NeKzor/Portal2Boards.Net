using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.Utilities
{
	internal static class Logger
	{
		public static Task LogModelException<T>(Exception e)
			where T : class, IModel
		{
			Debug.WriteLine($"[Portal2Boards.Net] Failed to fetch {typeof(T)} object.\n --- Stack Trace ---\n{e}\n -------------------");
			return Task.CompletedTask;
		}

		public static Task LogEntityException<T>(Exception e)
			where T : class, IEntity
		{
			Debug.WriteLine($"[Portal2Boards.Net] Failed to create {typeof(T)} object.\n --- Stack Trace ---\n{e}\n -------------------");
			return Task.CompletedTask;
		}
	}
}
