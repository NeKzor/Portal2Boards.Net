using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace Portal2Boards
{
	public class LogMessage
	{
		public Exception Error { get; }
		public string Message { get; }

		public LogMessage(Type type, Exception error)
		{
			Error = error;
			if (Error is JsonException)
				Message = $"[Portal2Boards.Net] Failed to parse {type} object.";
			else if (Error is HttpRequestException)
				Message = $"[Portal2Boards.Net] Failed to fetch {type} object.";
			else
				Message = $"[Portal2Boards.Net] Failed to create {type} object.";
		}

		public override string ToString()
		{
			return Message +
				"\n --- Stack Trace ---" +
				$"\n{Error}" +
				"\n -------------------";
		}
	}
}