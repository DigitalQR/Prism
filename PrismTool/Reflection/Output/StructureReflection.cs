using Prism.CodeParsing;
using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	/// <summary>
	/// Reflection structure for normal structure i.e. struct/class
	/// </summary>
	public class StructureReflection : StructureReflectionBase
	{
		private class StructureParent
		{
			public string DeclerationName;
			public string Accessor;
		}
		
		/// <summary>
		/// The type of this structure
		/// </summary>
		private string m_StructureType;

		/// <summary>
		/// The parent stuctures of this new instance
		/// </summary>
		private List<StructureParent> m_StructureParents;

		/// <summary>
		/// All the variables in this structure
		/// </summary>
		private List<VariableReflection> m_Variables;

		/// <summary>
		/// All the functions in this structure
		/// </summary>
		private List<FunctionReflection> m_Functions;

		public StructureReflection(StructureSignature.ImplementationBeginData data, string[] tokenNamespace, ConditionState conditionState, int bodyLine, string tokenParams, string docString)
			: base(data.DeclareName, tokenNamespace, conditionState, bodyLine, tokenParams, docString)
		{
			m_StructureType = data.StructureType;

			m_Variables = new List<VariableReflection>();
			m_Functions = new List<FunctionReflection>();
			m_StructureParents = new List<StructureParent>();

			if (data.ParentCount != 0)
			{
				foreach (var inheritData in data.ParentStructures)
				{
					StructureParent parent = new StructureParent();
					parent.DeclerationName = inheritData.DeclareName;
					parent.Accessor = inheritData.Access;
					m_StructureParents.Add(parent);
				}
			}
		}

		public override string StructureType
		{
			get { return m_StructureType; }
		}

		public FunctionReflection[] Functions
		{
			get { return m_Functions.ToArray(); }
		}

		public VariableReflection[] Variables
		{
			get { return m_Variables.ToArray(); }
		}

		public override void AddInternalSignature(SignatureInfo sigInfo, string accessor, ConditionState conditionState, int tokenLine, string tokenParams, string docString)
		{
			if (sigInfo.SignatureType == SignatureInfo.SigType.FunctionDeclare)
			{
				var data = (FunctionInfo)sigInfo.AdditionalParam;
				FunctionReflection refl = new FunctionReflection(this, data, conditionState, tokenLine, tokenParams, docString);
				m_Functions.Add(refl);
			}
			else if (sigInfo.SignatureType == SignatureInfo.SigType.VariableDeclare)
			{
				var data = (VariableInfo)sigInfo.AdditionalParam;
				VariableReflection refl = new VariableReflection(this, data, conditionState, tokenLine, tokenParams, docString);
				m_Variables.Add(refl);
			}
		}

		public override string GenerateHeaderReflectionContent()
		{
			string content = "";

			// Setup Prism Class (Stored in body, so dupe name doesn't matter)
			content += @"private:
class ClassInfo : public Prism::Class
{
private:
	static ClassInfo g_AssemblyInstance;
	ClassInfo();
};
";
			// Add method header reflection
			foreach (var method in m_Functions)
			{
				content += method.GenerateHeaderReflectionContent();
			}

			// Add proprety header reflection
			foreach (var property in m_Variables)
			{
				content += property.GenerateHeaderReflectionContent();
			}

			// Reset the default access specifier
			if (m_StructureType == "struct")
				content += "public:\n";
			
			return content;
		}

		public override string GenerateIncludeReflectionContent()
		{
			string content = "";

			// Add method header reflection
			foreach (var method in m_Functions)
			{
				content += method.GenerateIncludeReflectionContent();
			}

			// Add proprety header reflection
			foreach (var property in m_Variables)
			{
				content += property.GenerateIncludeReflectionContent();
			}

			return content;
		}

		public override string GenerateSourceReflectionContent()
		{
			string content = "";

			string namespaceKey = TokenNamespaceFormatted == "" ? "" : TokenNamespaceFormatted + "::";

			// Enter into the existing namespace
			foreach (string space in TokenNamespace)
				content += "namespace " + space + " {\n";

			// Implement constructure
			content += @"
%CLASS_NAME%::ClassInfo::ClassInfo()
	: Prism::Class(
		Prism::TypeId::Get<%CLASS_NAME%>(), 
		PRISM_STR(""%NAMESPACE_STR%""), PRISM_STR(""%CLASS_NAME%""), PRISM_DEVSTR(R""(%DOC_STRING%)""),
		sizeof(%CLASS_NAME%),
		{ %METHOD_INSTANCES% },
		{ %PROPERTY_INSTANCES% }
)
{}

%CLASS_NAME%::ClassInfo %CLASS_NAME%::ClassInfo::g_AssemblyInstance;
";

			// Add method header reflection
			string methodInstances = "";
			foreach (var method in m_Functions)
			{
				content += "\n#if " + method.PreProcessorCondition + "\n";
				content += method.GenerateSourceReflectionContent();
				content += "\n#endif\n";

				methodInstances += "\n#if " + method.PreProcessorCondition + "\n";
				methodInstances += "new MethodInfo_" + method.ReflectionInfo.SafeFunctionName + "(), ";
				methodInstances += "\n#endif\n";
			}

			// Add proprety header reflection
			string propertyInstances = "";
			foreach (var property in m_Variables)
			{
				content += "\n#if " + property.PreProcessorCondition + "\n";
				content += property.GenerateSourceReflectionContent();
				content += "\n#endif\n";

				propertyInstances += "\n#if " + property.PreProcessorCondition + "\n";
				propertyInstances += "new VariableInfo_" + property.ReflectionInfo.VariableName + "(), ";
				propertyInstances += "\n#endif\n";
			}


			// Exit into the existing namespace
			foreach (string space in TokenNamespace)
				content += "}\n";
			
			return content
				.Replace("%CLASS_NAME%", DeclerationName)
				.Replace("%NAMESPACE_STR%", TokenNamespaceFormatted)
				.Replace("%METHOD_INSTANCES%", methodInstances)
				.Replace("%PROPERTY_INSTANCES%", propertyInstances)
				.Replace("%DOC_STRING%", SafeDocString);
		}
	}
}
