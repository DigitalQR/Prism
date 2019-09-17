using Prism.Reflection.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Behaviour
{
	/// <summary>
	/// Internal exception type that will be thrown if any exceptions occur during behaviour passes
	/// </summary>
	public class BehaviourException : ReflectionException
	{
		public IReflectionBehaviour m_Behaviour;
		public IReflectionElement m_Element;

		public BehaviourException(string message, IReflectionBehaviour behaviour = null, IReflectionElement element = null)
			: base(message)
		{
			m_Behaviour = behaviour;
			m_Element = element;
		}
	}
}
