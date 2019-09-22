using Prism.Reflection.Elements.Cpp;
using Prism.Reflection.Elements.Cpp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Behaviour.Utils
{
	public static class FunctionContent
	{
		public static StructureElement GetParent(FunctionElement func)
		{
			// Check here?
			return func.ParentElement as StructureElement;
		}

		public static void GenerateJustDeclaration(FunctionElement func)
		{
			StructureElement parent = GetParent(func);
			parent.AppendInlineContent(func.Accessor + ":" + func.Details.ToString() + ";\n");
		}

		public static void GenerateJustBody(FunctionElement func, string body)
		{
			StructureElement parent = GetParent(func);

			if (!parent.IsTemplate)
			{
				// Format string for use in src
				List<string> parts = new List<string>();

				if (!func.Details.m_IsConstructor && !func.Details.m_IsDestructor)
					parts.Add(func.Details.m_ReturnInfo.ToString());

				List<string> paramParts = new List<string>();
				foreach (VariableInfo varInfo in func.Details.m_Params)
				{
					paramParts.Add(varInfo.ToString());
				}

				parts.Add(StructureContent.GetOwnerToken(parent) + "::" + func.Details.m_Name + "(" + string.Join(", ", paramParts) + ")");

				if (func.Details.m_IsConst)
					parts.Add("const");

				parts.Add(body);

				parent.AppendSourceContent(string.Join(" ", parts) + "\n");
			}
			else
				throw new BehaviourException("Cannot use FunctionUtils.GenerateJustBody with template structure");
		}

		public static void GenerateFull(FunctionElement func, string body)
		{
			StructureElement parent = GetParent(func);
			if (!parent.IsTemplate)
			{
				parent.AppendInlineContent(func.Accessor + ":" + func.ToString() + ";\n");
				GenerateJustBody(func, body);
			}
			else
			{
				parent.AppendInlineContent(func.Accessor + ":" + func.ToString() + body + "\n");
			}
		}
	}
}
