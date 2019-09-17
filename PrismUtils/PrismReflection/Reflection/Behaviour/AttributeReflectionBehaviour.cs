using Prism.Reflection.Elements;
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
	public abstract class AttributeReflectionBehaviour<ElementType> : ReflectionBehaviourBase where ElementType : class, IReflectionElement
	{
		public AttributeReflectionBehaviour(BehaviourTarget supportedTarget, int queuePriority)
			: base(null, supportedTarget, queuePriority, BehaviourApplication.Explicit)
		{
		}

		public AttributeReflectionBehaviour(string name, BehaviourTarget supportedTarget, int queuePriority)
			: base(name, supportedTarget, queuePriority, BehaviourApplication.Explicit)
		{
		}
		
		public override void RunBehaviour(IReflectionElement target, AttributeData data)
		{
			ElementType castTarget = target as ElementType;
			if (castTarget == null)
				throw new BehaviourException("Attribute behaviour '" + Name + "' has encounted unsupported element type");
			else
				RunBehaviour(castTarget, data);
		}

		public abstract void RunBehaviour(ElementType target, AttributeData data);
	}
}
