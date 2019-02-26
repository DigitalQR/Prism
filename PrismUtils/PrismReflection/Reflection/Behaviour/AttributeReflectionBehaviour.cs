using Prism.Reflection.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Behaviour
{
	/// <summary>
	/// Behaviour which will be applied to anything which has an attribute of matching name applied 
	/// (Will result in data attribute not being used, if one of matching name exists)
	/// </summary>
	public abstract class AttributeReflectionBehaviour : IReflectionBehaviour
	{
		private string m_Name;
		private BehaviourTarget m_Target;
		private int m_QueuePriority;

		public AttributeReflectionBehaviour(BehaviourTarget supportedTarget, int queuePriority)
			: this(null, supportedTarget, queuePriority)
		{
		}

		public AttributeReflectionBehaviour(string name, BehaviourTarget supportedTarget, int queuePriority)
		{
			m_Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
			m_Target = supportedTarget;
			m_QueuePriority = queuePriority;
		}

		public string Name => m_Name;

		public BehaviourTarget SupportedTargets => m_Target;

		public int QueuePriority => m_QueuePriority;

		public BehaviourApplication ApplicationMode => BehaviourApplication.Explicit;
		
		public abstract void RunBehaviour(IReflectableToken target, AttributeData data);
	}
}
