using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements
{
	public class ElementOrigin
	{
		/// <summary>
		/// The line number this token was discovered on
		/// </summary>
		public int LineNumber { get; private set; }

		public ElementOrigin(int lineNumber)
		{
			LineNumber = lineNumber;
		}
	}

	public interface IReflectionElement
	{
		/// <summary>
		/// Raw name to associate with this element
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Unqiue identifier to use with this element internally
		/// </summary>
		string UniqueName { get; }

		/// <summary>
		/// What type of behaviour should this element be affected by
		/// </summary>
		BehaviourTarget SupportedTargets { get; }
		
		/// <summary>
		/// Where this element originated from
		/// </summary>
		ElementOrigin Origin { get; }
		
		/// <summary>
		/// Any documentation string that was found with this element
		/// </summary>
		string Documentation { get; }

		/// <summary>
		/// Any pre-pro condition that this element is confined by
		/// </summary>
		string PreProcessorCondition { get; }

		/// <summary>
		/// The namespace this should be placed in
		/// </summary>
		string[] Namespace { get; }

		/// <summary>
		/// How this element was discovered/what additional behaviour should be performed on it
		/// </summary>
		ReflectionState DiscoveryState { get; }

		/// <summary>
		/// The element which owns this one (If any)
		/// </summary>
		IReflectionElement ParentElement { get; set; }

		/// <summary>
		/// Any elements which are owned under this
		/// </summary>
		IEnumerable<IReflectionElement> ChildElements { get; }

		/// <summary>
		/// Any content that should be placed inside the generated include source
		/// </summary>
		string GenerateIncludeContent();

		/// <summary>
		/// Any content that should be placed at the structure macro
		/// </summary>
		string GenerateInlineContent();

		/// <summary>
		/// Any content that should be placed inside the generated compile source
		/// </summary>
		string GenerateSourceContent();
	}
}
