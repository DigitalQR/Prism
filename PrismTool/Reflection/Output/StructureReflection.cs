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

		public StructureReflection(StructureSignature.ImplementationBeginData data, string[] tokenNamespace, ConditionState conditionState, string bodyFile, int bodyLine, string tokenParams, string docString)
			: base(data.DeclareName, tokenNamespace, conditionState, bodyFile, bodyLine, tokenParams, docString)
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

		public override void AddInternalSignature(SignatureInfo sigInfo, string accessor, ConditionState conditionState, string tokenFile, int tokenLine, string tokenParams, string docString)
		{
			if (sigInfo.SignatureType == SignatureInfo.SigType.FunctionDeclare)
			{
				var data = (FunctionInfo)sigInfo.AdditionalParam;
				FunctionReflection refl = new FunctionReflection(this, accessor, data, conditionState, tokenFile, tokenLine, tokenParams, docString);
				m_Functions.Add(refl);
			}
			else if (sigInfo.SignatureType == SignatureInfo.SigType.VariableDeclare)
			{
				var data = (VariableInfo)sigInfo.AdditionalParam;
				VariableReflection refl = new VariableReflection(this, accessor, data, conditionState, tokenFile, tokenLine, tokenParams, docString);
				m_Variables.Add(refl);
			}
			else if (sigInfo.SignatureType == SignatureInfo.SigType.StructureConstructor)
			{
				var data = (ConstructorInfo)sigInfo.AdditionalParam;
				FunctionReflection refl = new FunctionReflection(this, accessor, data, conditionState, tokenFile, tokenLine, tokenParams, docString);
				m_Functions.Add(refl);
			}
		}

		public override string GenerateHeaderReflectionContent()
		{
			string content = "";

			// Setup Prism Class (Stored in body, so dupe name doesn't matter)
			content += @"
public:
	virtual Prism::TypeInfo GetTypeInfo() const;

private:
class ClassInfo : public Prism::Class
{
private:
	static ClassInfo s_AssemblyInstance;
	ClassInfo();
	virtual Prism::ClassInfo GetParentClass(int index) const override;
	virtual size_t GetParentCount() const override;
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
		{ %ATTRIBUTES_VALUES% },
		std::is_abstract<%CLASS_NAME%>::value,
		{ %CONSTRUCTOR_INSTANCES% },
		{ %METHOD_INSTANCES% },
		{ %PROPERTY_INSTANCES% }
)
{}

Prism::TypeInfo %CLASS_NAME%::GetTypeInfo() const { return Prism::Assembly::Get().FindTypeOf<%CLASS_NAME%>(); }

Prism::ClassInfo %CLASS_NAME%::ClassInfo::GetParentClass(int searchIndex) const
{
	size_t index = 0;
%GET_PARENT_BODY%
	return nullptr;
}

size_t %CLASS_NAME%::ClassInfo::GetParentCount() const
{
	size_t count = 0;
%PARENT_COUNT_BODY%
	return count;
}

%CLASS_NAME%::ClassInfo %CLASS_NAME%::ClassInfo::s_AssemblyInstance;
";

			// Add method header reflection
			string constructorInstances = "";
			string methodInstances = "";
			foreach (var method in m_Functions)
			{
				content += "\n#if " + method.PreProcessorCondition + "\n";
				content += method.GenerateSourceReflectionContent();
				content += "\n#endif\n";

				if (method.IsConstructor)
				{
					constructorInstances += "\n#if " + method.PreProcessorCondition + "\n";
					constructorInstances += "new MethodInfo_" + method.ReflectionInfo.SafeFunctionName + "(), ";
					constructorInstances += "\n#endif\n";
				}
				else
				{
					methodInstances += "\n#if " + method.PreProcessorCondition + "\n";
					methodInstances += "new MethodInfo_" + method.ReflectionInfo.SafeFunctionName + "(), ";
					methodInstances += "\n#endif\n";
				}
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

			// Parent function bodies
			string getParentBody = "";
			string parentCountBody = "";

			foreach (var parent in m_StructureParents)
			{
				getParentBody += "__if_exists(" + parent.DeclerationName + "::ClassInfo) {";
				getParentBody += "if (index++ == searchIndex) { return Prism::Assembly::Get().FindTypeOf<" + parent.DeclerationName + ">(); }";
				getParentBody += "}\n";

				parentCountBody += "__if_exists(" + parent.DeclerationName + "::ClassInfo){ ++count; }\n";
			}

			// Exit into the existing namespace
			foreach (string space in TokenNamespace)
				content += "}\n";
			
			return content
				.Replace("%CLASS_NAME%", DeclerationName)
				.Replace("%NAMESPACE_STR%", TokenNamespaceFormatted.Replace("::", "."))
				.Replace("%ATTRIBUTES_VALUES%", GenerateAttributeInstancesString(m_StructureType == "class" ? "Classes" : "Structs"))
				.Replace("%CONSTRUCTOR_INSTANCES%", constructorInstances)
				.Replace("%METHOD_INSTANCES%", methodInstances)
				.Replace("%PROPERTY_INSTANCES%", propertyInstances)
				.Replace("%GET_PARENT_BODY%", getParentBody)
				.Replace("%PARENT_COUNT_BODY%", parentCountBody)
				.Replace("%DOC_STRING%", SafeDocString);
		}
	}
}
