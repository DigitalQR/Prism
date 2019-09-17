using Prism.Reflection.Elements;
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
		/// <summary>
		/// Store of all behaviour types
		/// </summary>
		private Dictionary<string, IReflectionBehaviour> m_BehaviourLookup;

		/// <summary>
		/// The priority at which the code content generation will occur for structures
		/// </summary>
		public static int StructureGenerationPriority { get { return 10000; } }

		/// <summary>
		/// The priority at which the code content generation will occur for internal fields of structures
		/// </summary>
		public static int FieldGenerationPriority { get { return StructureGenerationPriority - 1; } }

		public BehaviourController()
		{
			CommandLineArguments.FillValues(this);

			m_BehaviourLookup = new Dictionary<string, IReflectionBehaviour>();
		}

		public void PopulateBehaviours(IEnumerable<Assembly> assemblies = null)
		{
			if(assemblies == null)
				assemblies = AppDomain.CurrentDomain.GetAssemblies();
			
			// Add any assemblies currently loaded
			foreach (Assembly assembly in assemblies)
				FindBehavioursInAssembly(assembly);
		}

		public void PopulateBehaviours(IEnumerable<string> assembliesPaths)
		{
			// Add additional assemblies requested
			if (assembliesPaths != null)
			{
				foreach (string path in assembliesPaths)
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
		
		private class BehaviourInstance : IComparable
		{
			public IReflectionBehaviour m_Behaviour;
			public AttributeData m_Data;
			public IReflectionElement m_TargetElement;

			public int CompareTo(object obj)
			{
				BehaviourInstance other = obj as BehaviourInstance;
				if (other != null)
				{
					// Queue based on queue priority
					int compare = m_Behaviour.QueuePriority.CompareTo(other.m_Behaviour.QueuePriority);

					// In same queue priority, just compare based on whether it's a struct or not
					if (compare == 0)
					{
						BehaviourTarget thisStruct = (m_TargetElement.SupportedTargets & BehaviourTarget.Structure);
						BehaviourTarget otherStruct = (other.m_TargetElement.SupportedTargets & BehaviourTarget.Structure);

						compare = thisStruct.CompareTo(otherStruct);
					}

					return compare;
				}

				return 0;
			}

			public void Run()
			{
				m_Behaviour.RunBehaviour(m_TargetElement, m_Data);
			}
		}

		/// <summary>
		/// Fetch all attributes for a given element
		/// </summary>
		public void ProcessElement(IReflectionElement element)
		{
			// Set the state of any attribute
			ResolveAttributes(element);

			List<BehaviourInstance> behaviourQueue = new List<BehaviourInstance>();
			QueueBehaviours(element, behaviourQueue);

			// Sort behaviours based on priority
			behaviourQueue.Sort();

			// Execute all behaviours
			foreach (BehaviourInstance task in behaviourQueue)
				task.Run();
		}

		/// <summary>
		/// Discover which attributes are behaviours and which are data
		/// </summary>
		private void ResolveAttributes(IReflectionElement element)
		{
			AttributeCollection collection = element as AttributeCollection;

			if (collection != null)
			{
				foreach (AttributeData attrib in collection.Attributes)
				{
					if (attrib.Status == AttributeStatus.Unknown)
					{
						if (m_BehaviourLookup.ContainsKey(attrib.Name))
						{
							IReflectionBehaviour behaviour = m_BehaviourLookup[attrib.Name];

							if (behaviour.ApplicationMode == BehaviourApplication.Implicit)
								throw new BehaviourException("'" + attrib.Name + "' is an attibute behaviour (It is a globally applied behaviour)", behaviour, element);

							if ((behaviour.SupportedTargets & element.SupportedTargets) == 0)
								throw new BehaviourException("Cannot apply attribute '" + attrib.Name + "' to this target (Unsupported Attribute target)", behaviour, element);

							attrib.Status = AttributeStatus.Behaviour;
						}
						else
						{
							// Assume is data, if not behaviour
							attrib.Status = AttributeStatus.Data;
						}
					}
				}
			}


			// Resolve any sub tokens too
			foreach (IReflectionElement child in element.ChildElements)
				ResolveAttributes(child);
		}

		private void QueueBehaviours(IReflectionElement element, List<BehaviourInstance> behaviourQueue)
		{
			// Queue any behaviour to apply

			// Queue any sub-elements first
			foreach (IReflectionElement child in element.ChildElements)
				QueueBehaviours(child, behaviourQueue);

			// Add global behaviour
			foreach (var pair in m_BehaviourLookup)
			{
				IReflectionBehaviour behaviour = pair.Value;

				if (behaviour.ApplicationMode == BehaviourApplication.Implicit && (behaviour.SupportedTargets & element.SupportedTargets) != 0)
					behaviourQueue.Add(new BehaviourInstance { m_Behaviour = behaviour, m_Data = null, m_TargetElement = element });
			}

			AttributeCollection collection = element as AttributeCollection;

			if (collection != null)
			{
				foreach (var attrib in collection.Attributes)
				{
					// State was validated earlier, so no need for additional checks
					if (attrib.Status == AttributeStatus.Behaviour)
						behaviourQueue.Add(new BehaviourInstance { m_Behaviour = m_BehaviourLookup[attrib.Name], m_Data = attrib, m_TargetElement = element });
				}
			}
		}
	}
}
