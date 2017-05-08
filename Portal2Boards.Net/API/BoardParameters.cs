using System.Collections;
using System.Collections.Generic;

namespace Portal2Boards.Net.API
{
	public class BoardParameters : IEnumerable<KeyValuePair<IParameter, string>>
	{
		public Dictionary<IParameter, string> Parameters { get; set; }

		public BoardParameters()
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
	}
}