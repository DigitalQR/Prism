using Prism.Reflection;
using Prism.Reflection.Elements;
using Prism.Reflection.Elements.Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prism.Utils
{
	public static class GenerationUtils
	{
		public static string GetInternalMarker(IReflectionElement elem)
		{
			var attribs = elem as AttributeCollection;
			int dataAttribs = attribs != null ? attribs.DataAttributes.Count() : 0;

			return $@"
///
/// {elem.Name} ({elem.GetType().Name}:{elem.Origin.LineNumber})
/// Attibutes: {dataAttribs}
/// Children: {elem.ChildElements.Count()}
///
";
		}

		public static string GetDataAttributeInstancesString(AttributeCollection collection)
		{
			List<string> parts = new List<string>();
			foreach (AttributeData attrib in collection.DataAttributes)
			{
				parts.Add($"new {attrib.Name}Attribute({string.Join(",", attrib.Params)})");
			}
			return string.Join(",", parts);
		}

		public static string GetNamespaceOpen(IReflectionElement elem)
		{
			return string.Join("", elem.Namespace.Select((v) => "namespace " + v + " {"));
		}

		public static string GetNamespaceClose(IReflectionElement elem)
		{
			return string.Join("", elem.Namespace.Select((v) => '}'));
		}

		public static string GetInlineBlockName(IReflectionElement elem)
		{
			return MakeStringSyntaxSafe("PRISM_INLINE_DEF_" + (elem.ParentElement != null ? elem.ParentElement.UniqueName + "_" : "") + elem.UniqueName);
		}

		public static string MakeStringSyntaxSafe(string source)
		{
			Regex pattern = new Regex("s/[<>,]");
			return pattern.Replace(source, "_");
		}

		public static string GetInlineBlock(IReflectionElement elem, string content)
		{
			string blockName = GetInlineBlockName(elem);
			return $@"
#ifdef {blockName}
#undef {blockName}
#endif
#if {elem.PreProcessorCondition}
#define {blockName} {content.Replace("\r\n", "\n").Replace("\n", "\\\r\n\t\t")}

#else
#define {blockName}
#endif
";
		}
		
		public static void AppendChildIncludeContent(ReflectionElementBase target)
		{
			foreach (IReflectionElement elem in target.ChildElements)
			{
				string includeContent = elem.GenerateIncludeContent();
				string inlineContent = elem.GenerateInlineContent();

				target.AppendIncludeContent(GetInternalMarker(elem));
				target.AppendIncludeContent(includeContent);
				target.AppendIncludeContent(GetInlineBlock(elem, inlineContent));
			}
		}

		public static void AppendChildInlineContent(ReflectionElementBase target)
		{
			foreach (IReflectionElement elem in target.ChildElements)
			{
				target.AppendInlineContent(GenerationUtils.GetInlineBlockName(elem) + " ");
			}
		}

		public static void AppendChildSourceContent(ReflectionElementBase target)
		{
			foreach (IReflectionElement elem in target.ChildElements)
			{
				string content = elem.GenerateSourceContent();

				if (!string.IsNullOrWhiteSpace(content))
				{
					target.AppendSourceContent(GetInternalMarker(elem));
					target.AppendSourceContent(content);
				}
			}
		}

		public static string GetTemplateInfo(StructureElement target)
		{
			if (target.IsTemplate)
			{
				// TODO
				return "nullptr";
			}

			return "nullptr";
		}
	}
}
