using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection
{
	public enum ReflectionState
	{
		/// <summary>
		/// Unaware of the state (May be implemented just not in the header, so don't reflect)
		/// </summary>
		Unknown,

		/// <summary>
		/// Found during parsing (Doesn't need to be implemented)
		/// </summary>
		Discovered,

		/// <summary>
		/// Default implementation should be provided for this (Wasn't present during parsing)
		/// </summary>
		ProvideDefault,

		/// <summary>
		/// Custom implementation should be provided for this
		/// </summary>
		ProvideCustom
	}
}
