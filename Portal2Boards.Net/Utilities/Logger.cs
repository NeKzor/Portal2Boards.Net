using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Net.Entities;

namespace Portal2Boards.Net.Utilities
{
	internal static class Logger
	{
		public static Task LogException<T>(Exception e)
		{
			Debug.WriteLine($"[Portal2Boards.Net] Failed to create {typeof(T)} object.\n --- Stack Trace ---\n{e}\n -------------------");
			return Task.FromResult(0);
		}

		public static Task<ResponseType> LogModelException<T>(Exception e)
			where T : class, IModel
		{
			Debug.WriteLine($"[Portal2Boards.Net] Failed to fetch {typeof(T)} object.\n --- Stack Trace ---\n{e}\n -------------------");
			if (e is JsonException)
				return Task.FromResult(ResponseType.DeserializationError);
			if (e is HttpRequestException)
				return Task.FromResult(ResponseType.HttpRequestError);
			return Task.FromResult(ResponseType.Unknown);
		}

		public static Task LogEntityException<T>(Exception e)
			where T : class, IEntity
		{
			Debug.WriteLine($"[Portal2Boards.Net] Failed to create {typeof(T)} object.\n --- Stack Trace ---\n{e}\n -------------------");
			return Task.FromResult(0);
		}
	}
}
