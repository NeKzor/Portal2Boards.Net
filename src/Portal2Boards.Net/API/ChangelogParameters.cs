using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Portal2Boards.API
{
	public class ChangelogParameters : IEnumerable<KeyValuePair<IParameter, string>>
	{
		public Dictionary<IParameter, string> Parameters { get; set; }

		public ChangelogParameters()
		{
			Parameters = new Dictionary<IParameter, string>();
			foreach (Parameter parameter in new Parameters())
				Parameters.Add(parameter, default(string));
		}

		public object this[IParameter p]
		{
			get => Parameters[p];
			set => Parameters[p] = value.ToString();
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> Parameters.GetEnumerator();
		public IEnumerator<KeyValuePair<IParameter, string>> GetEnumerator()
			=> ((IEnumerable<KeyValuePair<IParameter, string>>)Parameters).GetEnumerator();

		public Task<string> ToQuery()
		{
			var query = "?";
			foreach (var parameter in Parameters)
			{
				if (string.IsNullOrEmpty(parameter.Value))
					continue;
				query += $"{parameter.Key.Value}={Uri.EscapeDataString(parameter.Value)}&";
			}
			return Task.FromResult(query.Remove(query.Length - 1));
		}
	}
}