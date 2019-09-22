using Prism.Reflection.Elements;
using Prism.Reflection.Elements.Cpp;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Behaviour.Utils
{
	public static class StructureContent
	{
		public static StructureElement GetParent(StructureElement elem)
		{
			// Check here?
			return elem.ParentElement as StructureElement;
		}

		public static string GetOwnerToken(StructureElement parent)
		{
			if (parent.ParentElement is StructureElement)
			{
				var p = parent.ParentElement as StructureElement;
				return GetOwnerToken(p) + "::" + parent.Name;
			}
			else
				return parent.Name;
		}
		
		/// <summary>
		/// Generates the implementation for this structure and every element store within
		/// *Content will be appended to parent
		/// </summary>
		public static void GenerateInlineFull(ReflectionElementBase store, StructureElement elem)
		{
			//parent.AppendIncludeContent(elem.GenerateIncludeContent());
			store.AppendInlineContent($@"
class {elem.GetUntemplatedName()} {GetInheritanceString(elem)}
{{
{elem.GenerateInlineContent()}
}};
");

			store.AppendSourceContent($@"
#if {elem.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(elem)}
{elem.GenerateSourceContent()}
{GenerationUtils.GetNamespaceClose(elem)}
#endif
");
		}

		/// <summary>
		/// Generates the implementation for this structure and every element store within
		/// *Content will be appended to parent
		/// </summary>
		public static void GenerateIncludeFull(ReflectionElementBase store, StructureElement elem)
		{
			//parent.AppendIncludeContent(elem.GenerateIncludeContent());
			store.AppendIncludeContent($@"
#if {elem.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(elem)}
class {elem.GetUntemplatedName()} {GetInheritanceString(elem)}
{{
{elem.GenerateInlineContent()}
}};
{GenerationUtils.GetNamespaceClose(elem)}
#endif
");

			store.AppendSourceContent($@"
#if {elem.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(elem)}
{elem.GenerateSourceContent()}
{GenerationUtils.GetNamespaceClose(elem)}
#endif
");
		}

		private static string GetInheritanceString(StructureElement elem)
		{
			if (elem.Details.m_Parents.Any())
			{
				var parts = elem.Details.m_Parents.Select((d) => 
				{
					if (d.m_Accessor != null)
						return d.m_Accessor + " " + d.m_Name;
					else
						return d.m_Name;
				});

				return ": " + string.Join(", ", parts);
			}

			return "";
		}
	}
}
