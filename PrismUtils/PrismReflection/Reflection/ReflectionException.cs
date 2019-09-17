using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	/// <summary>
	/// Internal exception type that all other Prism exception types will extend from
	/// </summary>
	public class ReflectionException : Exception
	{
		public ReflectionException(string message)
			: base(message)
		{
		}

		public ReflectionException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
