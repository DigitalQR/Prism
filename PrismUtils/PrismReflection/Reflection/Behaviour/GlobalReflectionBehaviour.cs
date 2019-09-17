using Prism.Reflection.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Behaviour
{
	/// <summary>
	/// Behaviour which will be applied to everything
	/// </summary>
	public abstract class GlobalReflectionBehaviour<ElementType> : ReflectionBehaviourBase where ElementType : class, IReflectionElement
	{		
		public GlobalReflectionBehaviour(BehaviourTarget supportedTarget, int queuePriority)
			: base(null, supportedTarget, queuePriority, BehaviourApplication.Implicit)
		{
		}

		public GlobalReflectionBehaviour(string name, BehaviourTarget supportedTarget, int queuePriority)
			: base(name, supportedTarget, queuePriority, BehaviourApplication.Implicit)
		{
		}
				
		public override void RunBehaviour(IReflectionElement target, AttributeData data)
		{
			ElementType castTarget = target as ElementType;
			if (castTarget == null)
				throw new BehaviourException("Global behaviour '" + Name + "' has encounted unsupported element type");
			else
				RunBehaviour(castTarget);
		}

		public abstract void RunBehaviour(ElementType target);
	}
}
