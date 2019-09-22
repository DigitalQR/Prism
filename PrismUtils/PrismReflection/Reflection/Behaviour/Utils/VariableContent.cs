using Prism.Reflection.Elements.Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Behaviour.Utils
{
	public static class VariableContent
	{
		public static StructureElement GetParent(VariableElement variable)
		{
			// Check here?
			return variable.ParentElement as StructureElement;
		}
		
		private static void GenerateStaticDeclaration(VariableElement variable)
		{
			StructureElement parent = GetParent(variable);

			if (!parent.IsTemplate)
			{
				List<string> parts = new List<string>();
				
				parts.Add(variable.Details.m_TypeInfo.ToString());
				parts.Add(StructureContent.GetOwnerToken(parent) + "::" + variable.Details.m_Name);

				if (variable.Details.m_DefaultValue != null)
				{
					parts.Add("=");
					parts.Add(variable.Details.m_DefaultValue);
				}
				
				parent.AppendSourceContent(string.Join(" ", parts) + ";\n");
			}
		}
		
		public static void GenerateFull(VariableElement variable)
		{
			StructureElement parent = GetParent(variable);
			if (!parent.IsTemplate)
			{
				if (variable.Details.m_IsStatic && !variable.Details.m_IsInlined)
				{
					parent.AppendInlineContent(variable.Accessor + ":" + variable.Details.ToString() + ";\n");
					GenerateStaticDeclaration(variable);
				}
				else
				{
					parent.AppendInlineContent(variable.Accessor + ":" + variable.Details.ToStringWithDefaultValue() + ";\n");
				}
			}
			else
			{
				parent.AppendInlineContent(variable.Accessor + ":" + variable.Details.ToStringWithDefaultValue() + ";\n");
			}
		}
	}
}
