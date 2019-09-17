using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Reflection.Elements;

namespace Prism.Reflection.Behaviour
{
	public abstract class ReflectionBehaviourBase : IReflectionBehaviour
	{
		private string m_Name;
		private BehaviourTarget m_Target;
		private int m_QueuePriority;
		private BehaviourApplication m_ApplicationMode;

		public ReflectionBehaviourBase(string name, BehaviourTarget supportedTarget, int queuePriority, BehaviourApplication application)
		{
			m_Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
			m_Target = supportedTarget;
			m_QueuePriority = queuePriority;
			m_ApplicationMode = application;
		}

		public string Name { get => m_Name; }
		public BehaviourTarget SupportedTargets { get => m_Target; }
		public int QueuePriority { get => m_QueuePriority; }
		public BehaviourApplication ApplicationMode { get => m_ApplicationMode; }

		public abstract void RunBehaviour(IReflectionElement target, AttributeData data);
	}
}
