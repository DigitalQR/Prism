using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements
{
	/// <summary>
	/// Internal exception type that will be thrown if any exceptions occur during behaviour passes
	/// </summary>
	public class ElementException : ReflectionException
	{
		public IReflectionElement m_Element;

		public ElementException(string message, IReflectionElement element = null)
			: base(message)
		{
			m_Element = element;
		}
	}
}
