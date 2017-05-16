﻿using System.Diagnostics;

namespace Portal2Boards.Net.API
{
	[DebuggerDisplay("{Value,nq}")]
	public class Parameter : IParameter
	{
		public string Value { get; set; }

		public Parameter()
		{
		}
		public Parameter(string parameter)
			=> Value = parameter;
	}
}