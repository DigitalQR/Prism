using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Tokens
{
	[Flags]
	public enum FunctionProperties
	{
		None = 0,
		Constructor = 1,
		Destructor = 2,
		Const = 4,
		Virtual = 8,
		Static = 16,
		Inline = 32,
		Abstract = 64,
		Explicit = 128
	}

	public class FunctionToken : ReflectableTokenBase
	{
		private AccessorMode m_Accessor;
		private string m_Name;
		private FunctionProperties m_Properties;
		private string m_PreProcessorCondition;
		private NamedTypeToken[] m_ParamTypes;
		private TypeToken m_ReturnType;
		
		private string m_Documentation;
		private ReflectionState m_DeclarationReflecitonState;
		private ReflectionState m_ImplementationReflectionState;
		private string m_CustomBody;

		/// <summary>
		/// The accessor for this variable
		/// </summary>
		public AccessorMode Accessor
		{
			get { return m_Accessor; }
		}

		/// <summary>
		/// Code name for this function
		/// </summary>
		public string Name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// Get the properties flag for this function
		/// </summary>
		public FunctionProperties Properties
		{
			get { return m_Properties; }
		}

		/// <summary>
		/// The preprocessor condition string for this structure
		/// </summary>
		public string PreProcessorCondition
		{
			get { return m_PreProcessorCondition; }
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
		/// All the params for this
		/// </summary>
		public NamedTypeToken[] ParamTypes
		{
			get { return m_ParamTypes; }
		}

		/// <summary>
		/// The type that will be returned by this function
		/// </summary>
		public TypeToken ReturnType
		{
			get { return m_ReturnType; }
		}

		/// <summary>
		/// The reflection state for this function's declaration
		/// </summary>
		public ReflectionState DeclarationState
		{
			get { return m_DeclarationReflecitonState; }
		}

		/// <summary>
		/// The reflection state for this function's implementation
		/// </summary>
		public ReflectionState ImplementationState
		{
			get { return m_ImplementationReflectionState; }
		}

		/// <summary>
		/// Replace any parts of the name that are reserved keywords
		/// </summary>
		public string SafeName
		{
			get
			{
				return Name.Replace("+", "Add")
				.Replace("-", "Sub")
				.Replace("*", "Mult")
				.Replace("/", "Div")
				.Replace("%", "Perc")
				.Replace("^", "Hat")
				.Replace("&", "Amp")
				.Replace("|", "Pipe")
				.Replace("~", "Inv")
				.Replace("!", "Not")
				.Replace(",", "Com")
				.Replace("=", "Equ")
				.Replace("<", "Less")
				.Replace(">", "Great")
				.Replace("(", "BrSrt")
				.Replace(")", "BrEnd")
				.Replace("[", "SqSrt")
				.Replace("]", "SqEnd");
			}
		}

		public bool IsConstructor
		{
			get { return (m_Properties & FunctionProperties.Constructor) != 0; }
		}

		public bool IsDestructor
		{
			get { return (m_Properties & FunctionProperties.Destructor) != 0; }
		}

		public bool IsConst
		{
			get { return (m_Properties & FunctionProperties.Const) != 0; }
		}

		public bool IsVirtual
		{
			get { return (m_Properties & FunctionProperties.Virtual) != 0; }
		}

		public bool IsStatic
		{
			get { return (m_Properties & FunctionProperties.Static) != 0; }
		}

		public bool IsInline
		{
			get { return (m_Properties & FunctionProperties.Inline) != 0; }
		}

		public bool IsAbstract
		{
			get { return (m_Properties & FunctionProperties.Abstract) != 0; }
		}

		public bool IsExplicit
		{
			get { return (m_Properties & FunctionProperties.Explicit) != 0; }
		}

		public FunctionToken(TokenOrigin origin, AccessorMode accessor, string name, string preProcessorCondition, NamedTypeToken[] paramTypes, TypeToken returnType, FunctionProperties properties, ReflectionState declarationState)
			: base(origin, BehaviourTarget.Function)
		{
			m_Accessor = accessor;
			m_Name = name;
			m_Properties = properties;
			m_PreProcessorCondition = preProcessorCondition;
			m_ParamTypes = paramTypes;
			m_ReturnType = returnType;
			m_DeclarationReflecitonState = declarationState;
			m_ImplementationReflectionState = ReflectionState.Unknown;
			m_Documentation = "";

			NamedTypeToken.Sanitize(ref m_ParamTypes);
			TypeToken.Sanitize(ref m_ReturnType);

			for (int i = 0; i < m_ParamTypes.Length; ++i)
			{
				if (string.IsNullOrWhiteSpace(m_ParamTypes[i].Name))
				{
					m_ParamTypes[i].Name = "param" + i;
				}
			}

			// Constructors must be static
			if (IsConstructor)
				m_Properties |= FunctionProperties.Static;
		}

		/// <summary>
		/// Forget about any custom body which has been provided
		/// </summary>
		public void ForgetCustomBody()
		{
			m_ImplementationReflectionState = ReflectionState.Unknown;
			m_CustomBody = null;
		}

		/// <summary>
		/// During reflection, a default function body will also be provided
		/// </summary>
		public void ProvideDefaultBody()
		{
			m_ImplementationReflectionState = ReflectionState.ProvideDefault;
			m_CustomBody = null;
		}

		/// <summary>
		/// During reflection, this custom function body will also be provided
		/// </summary>
		public void ProvideCustomBody(string body)
		{
			if (string.IsNullOrWhiteSpace(body))
			{
				ProvideDefaultBody();
			}
			else
			{
				m_ImplementationReflectionState = ReflectionState.ProvideCustom;
				m_CustomBody = body;
			}
		}

		/// <summary>
		/// Expand any macros relating to this type (Missing macros will be left)
		/// $(PreProcessorCondition)
		/// $(ReflectedName)
		/// $(ReflectHash)
		/// $(Accessor)
		/// $(AccessorPretty)
		/// $(IsPublic)
		/// $(IsPrivate)
		/// $(IsProtected)
		/// $(Name)
		/// $(SafeName)
		/// $(Documentation)
		/// $(ReturnType.*TypeTokenMacro*)
		/// $(ParamCount)
		/// $(Param[i].*TypeTokenMacro*)
		/// $(ParamTypeList)			e.g. int, float, std::string
		/// $(ParamNameList)			e.g. param0, param1, param2
		/// $(ParamDeclarationList)		e.g. int param0, float param1, std::string param2
		/// $(IsConstructor)
		/// $(IsDestructor)
		/// $(IsConst)
		/// $(IsVirtual)
		/// $(IsStatic)
		/// $(IsInline)
		/// $(IsAbstract)
		/// $(IsExplicit)
		/// Along with all macro expansions found in TypeToken
		/// </summary>
		/// <param name="input">The raw input string which should have it's macros replaced</param>
		/// <param name="prefix">The prefix to apply to every macro</param>
		/// <param name="suffix">The suffix to apply to each macro</param>
		/// <returns>The string with all relevent macros expanded</returns>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			builder.Replace("$(" + prefix + "PreProcessorCondition" + suffix + ")", string.IsNullOrWhiteSpace(m_PreProcessorCondition) ? "1" : m_PreProcessorCondition);
			builder.Replace("$(" + prefix + "ReflectedName" + suffix + ")", SafeName + (IsConstructor ? "Constructor" : (IsDestructor ? "Destructor" : "")) + "_" + GetReflectionHash());
			builder.Replace("$(" + prefix + "ReflectHash" + suffix + ")", GetReflectionHash());
			builder.Replace("$(" + prefix + "Documentation" + suffix + ")", m_Documentation);
			builder.Replace("$(" + prefix + "Accessor" + suffix + ")", m_Accessor.ToString().ToLower());
			builder.Replace("$(" + prefix + "AccessorPretty" + suffix + ")", m_Accessor.ToString());
			builder.Replace("$(" + prefix + "IsPublic" + suffix + ")", m_Accessor == AccessorMode.Public ? "1" : "0");
			builder.Replace("$(" + prefix + "IsPrivate" + suffix + ")", m_Accessor == AccessorMode.Private ? "1" : "0");
			builder.Replace("$(" + prefix + "IsProtected" + suffix + ")", m_Accessor == AccessorMode.Protected ? "1" : "0");
			builder.Replace("$(" + prefix + "Name" + suffix + ")", m_Name);
			builder.Replace("$(" + prefix + "SafeName" + suffix + ")", SafeName);

			m_ReturnType.ExpandMacros(builder, "ReturnType.");

			builder.Replace("$(ParamCount)", m_ParamTypes.Length.ToString());

			for (int i = 0; i < m_ParamTypes.Length; ++i)
				m_ParamTypes[i].ExpandMacros(builder, "Param[" + i + "].");


			string paramTypes = "";
			string paramNames = "";
			string paramDeclarations = "";

			for (int i = 0; i < m_ParamTypes.Length; ++i)
			{
				if (i != 0)
				{
					paramTypes += ", ";
					paramNames += ", ";
					paramDeclarations += ", ";
				}

				paramTypes += m_ParamTypes[i].InnerType.GetInnerTypeMinimal();
				paramNames += m_ParamTypes[i].Name;
				paramDeclarations += m_ParamTypes[i].ToString();
			}
			builder.Replace("$(" + prefix + "ParamTypeList" + suffix + ")", paramTypes);
			builder.Replace("$(" + prefix + "ParamNameList" + suffix + ")", paramNames);
			builder.Replace("$(" + prefix + "ParamDeclarationList" + suffix + ")", paramDeclarations);
			
			builder.Replace("$(" + prefix + "IsConstructor" + suffix + ")", IsConstructor ? "1" : "0");
			builder.Replace("$(" + prefix + "IsDestructor" + suffix + ")", IsDestructor ? "1" : "0");
			builder.Replace("$(" + prefix + "IsConst" + suffix + ")", IsConst ? "1" : "0");
			builder.Replace("$(" + prefix + "IsVirtual" + suffix + ")", IsVirtual ? "1" : "0");
			builder.Replace("$(" + prefix + "IsStatic" + suffix + ")", IsStatic ? "1" : "0");
			builder.Replace("$(" + prefix + "IsInline" + suffix + ")", IsInline ? "1" : "0");
			builder.Replace("$(" + prefix + "IsAbstract" + suffix + ")", IsAbstract ? "1" : "0");
			builder.Replace("$(" + prefix + "IsExplicit" + suffix + ")", IsExplicit ? "1" : "0");
						
			return builder;
		}

		public StringBuilder ExpandMacros(StringBuilder builder, IReflectableToken context, string prefix = "", string suffix = "")
		{
			ExpandMacros(builder, prefix, suffix);

			StructureToken parentStructure = context as StructureToken;
			if (parentStructure != null)
				parentStructure.ExpandMacros(builder, "Parent.");

			return builder;
		}

		public string GetReflectionHash()
		{
			int hash = 54;
			hash = hash * 31 + m_ReturnType.GetReflectionHash();
			foreach (NamedTypeToken token in m_ParamTypes)
				hash = hash * 31 + token.GetReflectionHash();
			
			return hash.ToString("x");
		}

		public override StringBuilder GenerateDeclarationContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateDeclarationContent(context);
			return ExpandMacros(builder, context);
		}

		public override StringBuilder GenerateImplementationContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateImplementationContent(context);
			return ExpandMacros(builder, context);
		}

		public override StringBuilder GenerateIncludeContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateIncludeContent(context);
			return ExpandMacros(builder, context);
		}
	}
}
