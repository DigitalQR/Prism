using Prism.Reflection.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Behaviour
{
	/// <summary>
	/// Possible targets for reflection behaviour
	/// </summary>
	[Flags]
	public enum BehaviourTarget
	{
		None = 0,
		Structure = 1,
		Function = 2,
		Variable = 4,
		Enumurator = 8,
		EnumuratorValue = 16,
		File = 32,
		Content = File,
	}

	public enum BehaviourApplication
	{
		/// <summary>
		/// Will run for any valid target
		/// </summary>
		Implicit,

		/// <summary>
		/// Expects a ReflectionAttribute to be applied
		/// </summary>
		Explicit,
	}

	/// <summary>
	/// Behaviour which can be executed during the reflection of a given token
	/// </summary>
	public interface IReflectionBehaviour
	{
		/// <summary>
		/// The unique name for this attribute
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Targets which this behviour should be applied to
		/// </summary>
		BehaviourTarget SupportedTargets { get; }

		/// <summary>
		/// The priority for this behaviour when they are ran (Normal data reflection has a priority of 0, so -ve means before reflection, +ve means after reflection)
		/// </summary>
		int QueuePriority { get; }

		/// <summary>
		/// How should this behaviour be applied (i.e. Always run or require attribute of name)
		/// </summary>
		BehaviourApplication ApplicationMode { get; }

		/// <summary>
		/// Run any behaviour associated with this attribute
		/// </summary>
		void RunBehaviour(IReflectionElement target, AttributeData data);
	}
}
