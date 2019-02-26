using Prism.Reflection.Tokens;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Prism.Reflection.Behaviour
{
	public class BehaviourController
	{
		[CommandLineArgument(Arg = "custom-behaviour", Usage = "File path to any additional assemblies")]
		private string[] m_AdditionalAssemblies;

		private Dictionary<string, IReflectionBehaviour> m_BehaviourLookup;

		public BehaviourController()
		{
			CommandLineArguments.FillValues(this);

			m_BehaviourLookup = new Dictionary<string, IReflectionBehaviour>();
			PopulateBehaviours();
		}

		private void PopulateBehaviours()
		{
			// Add any assemblies currently loaded
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				FindBehavioursInAssembly(assembly);
			
			// Add additional assemblies requested
			if (m_AdditionalAssemblies != null)
			{
				foreach (string path in m_AdditionalAssemblies)
					FindBehavioursInExternal(path);
			}
		}

		/// <summary>
		/// Find all subclasses of IAttributeBehaviour present in a compiled assembly
		/// </summary>
		private void FindBehavioursInExternal(string assemblyPath)
		{
			if (!File.Exists(assemblyPath))
			{
				Console.Error.WriteLine("Error: Failed to find assembly at '" + assemblyPath + "' (Ignoring)");
			}

			Assembly assembly = null;
			try
			{
				assembly = Assembly.LoadFrom(assemblyPath);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Error: Found assembly but failed to load at '" + assemblyPath + "' (Ignoring)");
			}

			if(assembly != null)
				FindBehavioursInAssembly(assembly);
			else
				Console.Error.WriteLine("Error: Found assembly but failed to load at '" + assemblyPath + "' (Ignoring)");
		}

		/// <summary>
		/// Find all subclasses of IAttributeBehaviour
		/// </summary>
		private void FindBehavioursInAssembly(Assembly assembly)
		{
			// Attempt to add all types
			foreach (Type type in assembly.GetTypes())
				if (typeof(IReflectionBehaviour).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
				{
					IReflectionBehaviour behaviour = (IReflectionBehaviour)Activator.CreateInstance(type);
					if (behaviour != null)
					{
						if (m_BehaviourLookup.ContainsKey(behaviour.Name))
						{
							Console.Error.WriteLine("Found multiple AttributeBehaviours with name '" + behaviour.Name + "' (Ignoring later)");
							continue;
						}

						m_BehaviourLookup.Add(behaviour.Name, behaviour);
					}
				}
		}


		private struct BehaviourInstance
		{
			public IReflectionBehaviour Behaviour;
			public AttributeData Data;
		}

		/// <summary>
		/// Fetch all attributes for a given token
		/// </summary>
		public void ProcessToken(IReflectableToken target)
		{
			SortedList<int, BehaviourInstance> behaviourQueue = new SortedList<int, BehaviourInstance>();

			// Queue any behaviour to apply
			// Add global behaviour
			foreach(var pair in m_BehaviourLookup)
			{
				IReflectionBehaviour behaviour = pair.Value;

				if (behaviour.ApplicationMode == BehaviourApplication.Implicit && (behaviour.SupportedTargets & target.SupportedTargets) != 0)
					behaviourQueue.Add(behaviour.QueuePriority, new BehaviourInstance { Behaviour = behaviour, Data = null });
			}

			// TODO - Add Attribute Behaviours
			// ...

			// Execute behaviour
			foreach (BehaviourInstance task in behaviourQueue.Values)
			{
				task.Behaviour.RunBehaviour(target, task.Data);
			}
		}
	}
}
