using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Tokens
{
	public class EnumToken : ReflectableTokenBase
	{
		private string m_Name;
		private string m_StructureType;
		private string m_PreProcessorCondition;
		private string[] m_Namespace;

		private string m_InternalType;
		private List<EnumValueToken> m_Values;

		private string m_Documentation;
		private ReflectionState m_DeclarationReflectionState;

		/// <summary>
		/// Code name for this enum
		/// </summary>
		public string Name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// Code keyword for this enum structure (class or struct)
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
		/// Any values stored in this structure
		/// </summary>
		public IReadOnlyList<EnumValueToken> Values
		{
			get { return m_Values; }
		}

		/// <summary>
		/// The internal type of this enum (If one is provided)
		/// </summary>
		public string InternalType
		{
			get { return m_InternalType; }
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
			get { return m_DeclarationReflectionState; }
		}

		public EnumToken(TokenOrigin origin, string name, string structureType, string[] declaredNamespace, string preProcessorCondition, ReflectionState declarationState, string internalType, string attribParams)
			: base(origin, BehaviourTarget.Enumurator, attribParams)
		{
			m_Name = name;
			m_StructureType = structureType;
			m_Namespace = declaredNamespace;
			m_PreProcessorCondition = preProcessorCondition;
			m_InternalType = internalType;
			m_DeclarationReflectionState = declarationState;

			m_Values = new List<EnumValueToken>();

			if (m_Namespace == null)
				m_Namespace = new string[0];
		}

		/// <summary>
		/// Get a enumerator of all the internal value tokens for this enum
		/// </summary>
		public override IReadOnlyList<IReflectableToken> InternalTokens
		{
			get	{ return m_Values; }
		}

		/// <summary>
		/// Add a value to this enum, to be reflected
		/// </summary>
		public void AddValue(EnumValueToken value)
		{
			m_Values.Add(value);
		}

		/// <summary>
		/// Expand any macros relating to this function (Missing macros will be left)
		/// $(PreProcessorCondition)
		/// $(ReflectHash)
		/// $(ReflectedName)
		/// $(TokenOriginFile)
		/// $(TokenOriginLine)
		/// $(Documentation)
		/// $(Name)
		/// $(StructureType)
		/// $(InternalType)
		/// $(HasInternalType)
		/// $(NamespaceList[,])			e.g. Namespace { A, B, C } = "A,B,C"
		/// $(NamespaceList[.])			e.g. Namespace { A, B, C } = "A.B.C"
		/// $(NamespaceList[::])		e.g. Namespace { A, B, C } = "A::B::C"
		/// $(ValueCount)
		/// $(Value[i].Name)
		/// </summary>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			builder.Replace("$(" + prefix + "PreProcessorCondition" + suffix + ")", string.IsNullOrWhiteSpace(m_PreProcessorCondition) ? "1" : m_PreProcessorCondition);
			builder.Replace("$(" + prefix + "ReflectHash" + suffix + ")", GetReflectionHash());
			builder.Replace("$(" + prefix + "ReflectedName" + suffix + ")", m_Name + "_" + GetReflectionHash());
			builder.Replace("$(" + prefix + "TokenOriginFile" + suffix + ")", Origin.FilePath.ToString());
			builder.Replace("$(" + prefix + "TokenOriginLine" + suffix + ")", Origin.LineNumber.ToString());
			builder.Replace("$(" + prefix + "Documentation" + suffix + ")", m_Documentation);
			builder.Replace("$(" + prefix + "Name" + suffix + ")", m_Name);
			builder.Replace("$(" + prefix + "StructureType" + suffix + ")", m_StructureType);
			builder.Replace("$(" + prefix + "InternalType" + suffix + ")", m_InternalType);
			builder.Replace("$(" + prefix + "HasInternalType" + suffix + ")", string.IsNullOrWhiteSpace(m_InternalType) ? "0" : "1");

			builder.Replace("$(" + prefix + "NamespaceList[,]" + suffix + ")", string.Join(",", m_Namespace));
			builder.Replace("$(" + prefix + "NamespaceList[.]" + suffix + ")", string.Join(".", m_Namespace));
			builder.Replace("$(" + prefix + "NamespaceList[::]" + suffix + ")", string.Join("::", m_Namespace));

			builder.Replace("$(" + prefix + "NamespaceList[::]" + suffix + ")", string.Join("::", m_Namespace));
			builder.Replace("$(" + prefix + "NamespaceList[::]" + suffix + ")", string.Join("::", m_Namespace));

			builder.Replace("$(" + prefix + "ValueCount" + suffix + ")", m_Values.Count.ToString());

			for (int i = 0; i < m_Values.Count; ++i)
				m_Values[i].ExpandMacros(builder, prefix + "Value[" + i + "]" + suffix + ".");

			ExpandAttributeMacros(builder, prefix, suffix);
			return builder;
		}

		public string GetReflectionHash()
		{
			int hash = 2;
			hash = hash * 31 + m_Name.GetHashCode();
			hash = hash * 31 + m_StructureType.GetHashCode();
			hash = hash * 31 + m_InternalType.GetHashCode();

			return hash.ToString("x");
		}

		public override StringBuilder GenerateIncludeContent(IReflectableToken context)
		{
			StringBuilder builder = new StringBuilder();

			// Add debug comment header
			builder.Append(@"///
/// $(TokenOriginFile)($(TokenOriginLine))
/// Enum: $(StructureType) $(Name) 
/// Values: $(ValueCount)
/// 

");

			// Namespace open
			foreach (string ns in m_Namespace)
				builder.Append("namespace " + ns + " {\n");

			// Add value include before structure include, as they're reflected as part of the structure
			foreach (EnumValueToken value in m_Values)
				builder.Append(value.GenerateIncludeContent(this));
			
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

				foreach (EnumValueToken value in m_Values)
					declaration.Append(value.GenerateDeclarationContent(this) + "\n");
				
				// Add declaration inside of define which will be placed inside the orignal structure
				builder.Append("\n#define PRISM_REFLECTION_BODY_$(TokenOriginLine) ");
				builder.Append(declaration.Replace("\r\n", "\n").Replace("\n", "\\\n\t") + "\\\n");
			}

			return ExpandMacros(builder);
		}

		public override StringBuilder GenerateDeclarationContent(IReflectableToken context)
		{
			throw new InvalidOperationException("Cannot call GenerateDeclarationContent for EnumToken (It IS the declaration)");
		}

		public override StringBuilder GenerateImplementationContent(IReflectableToken context)
		{
			StringBuilder builder = new StringBuilder();

			// Add debug comment header
			builder.Append(@"///
/// $(TokenOriginFile)($(TokenOriginLine))
/// Enum: $(StructureType) $(Name) 
/// Values: $(ValueCount)
/// 

");

			// Namespace open
			foreach (string ns in m_Namespace)
				builder.Append("namespace " + ns + " {\n");

			builder.Append(base.GenerateImplementationContent(context));

			// Append any extra variable/function content
			foreach (EnumValueToken value in m_Values)
				builder.Append(value.GenerateImplementationContent(this));
			
			// Namespace close
			foreach (string ns in m_Namespace)
				builder.Append("}\n");

			return ExpandMacros(builder);
		}
	}
}
