using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Tokens
{

	public class ScopedStructure
	{
		public AccessorMode Accessor;
		public string Name;

		public ScopedStructure(AccessorMode accessor, string name)
		{
			this.Accessor = accessor;
			this.Name = name;
		}
	}

	public class StructureToken : ReflectableTokenBase
	{
		private string m_Name;
		private string m_StructureType;
		private string m_PreProcessorCondition;
		private string[] m_Namespace;

		private List<ScopedStructure> m_Parents;
		private List<FunctionToken> m_Methods;
		private List<VariableToken> m_Properties;
		
		private string m_Documentation;
		private ReflectionState m_DeclarationReflecitonState;

		/// <summary>
		/// Code name for this structure
		/// </summary>
		public string Name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// Code keyword for this structure
		/// </summary>
		public string StructureType
		{
			get { return m_StructureType; }
		}

		/// <summary>
		/// The preprocessor condition string for this structure
		/// </summary>
		public string PreProcessorCondition
		{
			get { return m_PreProcessorCondition; }
		}

		/// <summary>
		/// The namespace that this structure is declared in
		/// </summary>
		public string[] Namespace
		{
			get { return m_Namespace; }
		}

		/// <summary>
		/// Any parents for this structure
		/// </summary>
		public ICollection<ScopedStructure> ParentStructures
		{
			get { return m_Parents; }
		}

		/// <summary>
		/// Any methods for this structure
		/// </summary>
		public ICollection<FunctionToken> Methods
		{
			get { return m_Methods; }
		}

		/// <summary>
		/// Any properties for this structure
		/// </summary>
		public ICollection<VariableToken> Properties
		{
			get { return m_Properties; }
		}
		
		/// <summary>
		/// The code-side documentation found for this variable
		/// </summary>
		public string Documentation
		{
			get { return m_Documentation; }
			set { m_Documentation = value; }
		}

		/// <summary>
		/// The reflection state for this structure's declaration
		/// </summary>
		public ReflectionState DeclarationState
		{
			get { return m_DeclarationReflecitonState; }
		}

		public StructureToken(TokenOrigin origin, string name, string structureType, string[] declaredNamespace, string preProcessorCondition, ReflectionState declarationState, ScopedStructure[] parents)
			: base(origin, BehaviourTarget.Structure)
		{
			m_Name = name;
			m_StructureType = structureType;
			m_Namespace = declaredNamespace;
			m_PreProcessorCondition = preProcessorCondition;
			m_Parents = new List<ScopedStructure>(parents);
			m_Methods = new List<FunctionToken>();
			m_Properties = new List<VariableToken>();
			m_DeclarationReflecitonState = declarationState;

			if (m_Namespace == null)
				m_Namespace = new string[0];
		}

		/// <summary>
		/// Get a enumerator of all the internal tokens this token may have
		/// (Will return null, if there are no tokens)
		/// </summary>
		public override IEnumerable<IReflectableToken> InternalTokens
		{
			get
			{
				List<IReflectableToken> tokens = new List<IReflectableToken>();
				tokens.AddRange(m_Properties);
				tokens.AddRange(m_Methods);
				return tokens;
			}
		}

		/// <summary>
		/// Add a method to this structure, to be reflected
		/// </summary>
		public void AddMethod(FunctionToken function)
		{
			m_Methods.Add(function);
		}

		/// <summary>
		/// Add a method to this structure, to be reflected
		/// </summary>
		public void AddProperty(VariableToken variable)
		{
			m_Properties.Add(variable);
		}

		/// <summary>
		/// Expand any macros relating to this function (Missing macros will be left)
		/// $(PreProcessorCondition)
		/// $(ReflectHash)
		/// $(TokenOriginFile)
		/// $(TokenOriginLine)
		/// $(Documentation)
		/// $(Name)
		/// $(StructureType)
		/// $(NamespaceList[,])			e.g. Namespace { A, B, C } = "A,B,C"
		/// $(NamespaceList[.])			e.g. Namespace { A, B, C } = "A.B.C"
		/// $(NamespaceList[::])		e.g. Namespace { A, B, C } = "A::B::C"
		/// $(Method[i].*TypeTokenMacro*)
		/// $(MethodCount)
		/// $(Property[i].*TypeTokenMacro*)
		/// $(PropertyCount)
		/// $(Parent[i].Accessor)
		/// $(Parent[i].AccessorPretty)
		/// $(Parent[i].Name)
		/// $(ParentCount)
		/// </summary>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			builder.Replace("$(" + prefix + "PreProcessorCondition" + suffix + ")", string.IsNullOrWhiteSpace(m_PreProcessorCondition) ? "1" : m_PreProcessorCondition);
			builder.Replace("$(" + prefix + "ReflectHash" + suffix + ")", GetReflectionHash());
			builder.Replace("$(" + prefix + "TokenOriginFile" + suffix + ")", Origin.FilePath.ToString());
			builder.Replace("$(" + prefix + "TokenOriginLine" + suffix + ")", Origin.LineNumber.ToString());
			builder.Replace("$(" + prefix + "Documentation" + suffix + ")", m_Documentation);
			builder.Replace("$(" + prefix + "Name" + suffix + ")", m_Name);
			builder.Replace("$(" + prefix + "StructureType" + suffix + ")", m_StructureType);
			
			builder.Replace("$(" + prefix + "NamespaceList[,]" + suffix + ")", string.Join(",", m_Namespace));
			builder.Replace("$(" + prefix + "NamespaceList[.]" + suffix + ")", string.Join(".", m_Namespace));
			builder.Replace("$(" + prefix + "NamespaceList[::]" + suffix + ")", string.Join("::", m_Namespace));

			builder.Replace("$(" + prefix + "NamespaceList[::]" + suffix + ")", string.Join("::", m_Namespace));
			builder.Replace("$(" + prefix + "NamespaceList[::]" + suffix + ")", string.Join("::", m_Namespace));

			builder.Replace("$(" + prefix + "MethodCount" + suffix + ")", m_Methods.Count.ToString());
			builder.Replace("$(" + prefix + "PropertyCount" + suffix + ")", m_Properties.Count.ToString());
			builder.Replace("$(" + prefix + "ParentCount" + suffix + ")", m_Parents.Count.ToString());

			for (int i = 0; i < m_Methods.Count; ++i)
				m_Methods[i].ExpandMacros(builder, prefix + "Method[" + i + "]" + suffix + ".");

			for (int i = 0; i < m_Properties.Count; ++i)
				m_Properties[i].ExpandMacros(builder, prefix + "Property[" + i + "]" + suffix + ".");

			for (int i = 0; i < m_Parents.Count; ++i)
			{
				builder.Replace("$(" + prefix + "Parent[" + i + "]" + suffix + ".Accessor", m_Parents[i].Accessor.ToString().ToLower());
				builder.Replace("$(" + prefix + "Parent[" + i + "]" + suffix + ".AccessorPretty", m_Parents[i].Accessor.ToString());
				builder.Replace("$(" + prefix + "Parent[" + i + "]" + suffix + ".Name", m_Parents[i].Name);
			}

			return builder;
		}

		public string GetReflectionHash()
		{
			int hash = 2;
			hash = hash * 31 + m_Name.GetHashCode();
			hash = hash * 31 + m_StructureType.GetHashCode();

			return hash.ToString("x");
		}

		public override StringBuilder GenerateIncludeContent(IReflectableToken context)
		{
			StringBuilder builder = new StringBuilder();

			// Add debug comment header
			builder.Append(@"///
/// $(TokenOriginFile)($(TokenOriginLine))
/// Structure: $(StructureType) $(Name) 
/// Properties: $(PropertyCount)
/// Methods: $(MethodCount)
/// 

");

			// Namespace open
			foreach (string ns in m_Namespace)
				builder.Append("namespace " + ns + " {\n");

			// Add variable/function include before structure include, as they're reflected as part of the structure
			foreach (VariableToken variable in m_Properties)
				builder.Append(variable.GenerateIncludeContent(this));

			foreach (FunctionToken function in m_Methods)
				builder.Append(function.GenerateIncludeContent(this));

			// Generate any base include content for this structure
			builder.Append(base.GenerateIncludeContent(context));

			// Namespace close
			foreach (string ns in m_Namespace)
				builder.Append("}\n");
			builder.Append("\n");

			// Generate a macro-able declaration define (Will be later replaced where the token originates)
			{
				StringBuilder declaration = new StringBuilder();
				declaration.Append(base.GenerateDeclarationContent(context) + "\n");

				foreach (VariableToken variable in m_Properties)
					declaration.Append(variable.GenerateDeclarationContent(this) + "\n");

				foreach (FunctionToken function in m_Methods)
					declaration.Append(function.GenerateDeclarationContent(this) + "\n");

				// Add declaration inside of define which will be placed inside the orignal structure
				builder.Append("\n#define PRISM_REFLECTION_BODY_$(TokenOriginLine) ");
				builder.Append(declaration.Replace("\r\n", "\n").Replace("\n", "\\\n\t") + "\\\n\tprivate:\n\n");
			}
			
			return ExpandMacros(builder);
		}

		public override StringBuilder GenerateDeclarationContent(IReflectableToken context)
		{
			throw new InvalidOperationException("Cannot call GenerateDeclarationContent for StructureToken (It IS the declaration)");
		}

		public override StringBuilder GenerateImplementationContent(IReflectableToken context)
		{
			StringBuilder builder = new StringBuilder();

			// Add debug comment header
			builder.Append(@"///
/// $(TokenOriginFile)($(TokenOriginLine))
/// Structure: $(StructureType) $(Name) 
/// Properties: $(PropertyCount)
/// Methods: $(MethodCount)
/// 

");

			// Namespace open
			foreach (string ns in m_Namespace)
				builder.Append("namespace " + ns + " {\n");

			builder.Append(base.GenerateImplementationContent(context));
			
			// Append any extra variable/function content
			foreach (VariableToken variable in m_Properties)
				builder.Append(variable.GenerateImplementationContent(this));

			foreach (FunctionToken function in m_Methods)
				builder.Append(function.GenerateImplementationContent(this));


			// Namespace close
			foreach (string ns in m_Namespace)
				builder.Append("}\n");
			
			return ExpandMacros(builder);
		}
	}
}
