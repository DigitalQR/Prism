using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Reflection.Tokens;

namespace Prism.Reflection.Behaviour
{
	/// <summary>
	/// Behaviour which will be applied to everything
	/// </summary>
	public abstract class GlobalReflectionBehaviour : IReflectionBehaviour
	{
		private string m_Name;
		private BehaviourTarget m_Target;
		private int m_QueuePriority;

		public GlobalReflectionBehaviour(BehaviourTarget supportedTarget, int queuePriority)
			: this(null, supportedTarget, queuePriority)
		{
		}

		public GlobalReflectionBehaviour(string name, BehaviourTarget supportedTarget, int queuePriority)
		{
			m_Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
			m_Target = supportedTarget;
			m_QueuePriority = queuePriority;
		}

		public string Name => m_Name;

		public BehaviourTarget SupportedTargets => m_Target;

		public int QueuePriority => m_QueuePriority;

		public BehaviourApplication ApplicationMode => BehaviourApplication.Implicit;

		public abstract void RunBehaviour(IReflectableToken target);

		public void RunBehaviour(IReflectableToken target, AttributeData data)
		{
			RunBehaviour(target);
		}
	}
}
