using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Tokens
{
	public class VariableToken : ReflectableTokenBase
	{
		private AccessorMode m_Accessor;
		private NamedTypeToken m_TypeToken;
		private string m_PreProcessorCondition;
		
		private string m_Documentation;
		private ReflectionState m_DeclarationReflecitonState;
		
		/// <summary>
		/// The accessor for this variable
		/// </summary>
		public AccessorMode Accessor
		{
			get { return m_Accessor; }
		}

		/// <summary>
		/// Code name for this variable
		/// </summary>
		public string Name
		{
			get { return m_TypeToken.Name; }
		}
		
		/// <summary>
		/// The type of this variable
		/// </summary>
		public TypeToken InnerType
		{
			get { return m_TypeToken.InnerType; }
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
		/// The reflection state for this variable's declaration
		/// </summary>
		public ReflectionState DeclarationState
		{
			get { return m_DeclarationReflecitonState; }
		}

		public VariableToken(AccessorMode accessor, string name, TypeToken typeToken, string preProcessorCondition, ReflectionState declarationState)
			: base(BehaviourTarget.Variable)
		{
			m_Accessor = accessor;
			m_TypeToken = new NamedTypeToken(name, typeToken);
			m_PreProcessorCondition = preProcessorCondition;
			m_DeclarationReflecitonState = declarationState;
			m_Documentation = "";
			
			NamedTypeToken.Sanitize(ref m_TypeToken);
		}

		/// <summary>
		/// Expand any macros relating to this type (Missing macros will be left)
		/// $(PreProcessorCondition)
		/// $(ReflectedName)
		/// $(ReflectHash)
		/// $(Documentation)
		/// $(Accessor)
		/// $(AccessorPretty)
		/// $(IsPublic)
		/// $(IsPrivate)
		/// $(IsProtected)
		/// Along with all macro expansions found in TypeToken
		/// </summary>
		/// <param name="input">The raw input string which should have it's macros replaced</param>
		/// <param name="prefix">The prefix to apply to every macro</param>
		/// <param name="suffix">The suffix to apply to each macro</param>
		/// <returns>The string with all relevent macros expanded</returns>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			m_TypeToken.ExpandMacros(builder, prefix, suffix);

			builder.Replace("$(" + prefix + "PreProcessorCondition" + suffix + ")", string.IsNullOrWhiteSpace(m_PreProcessorCondition) ? "1" : m_PreProcessorCondition);
			builder.Replace("$(" + prefix + "ReflectedName" + suffix + ")", m_TypeToken.Name + "_" + GetReflectionHash());
			builder.Replace("$(" + prefix + "ReflectHash" + suffix + ")", GetReflectionHash());
			builder.Replace("$(" + prefix + "Documentation" + suffix + ")", m_Documentation);
			builder.Replace("$(" + prefix + "Accessor" + suffix + ")", m_Accessor.ToString().ToLower());
			builder.Replace("$(" + prefix + "AccessorPretty" + suffix + ")", m_Accessor.ToString());
			builder.Replace("$(" + prefix + "IsPublic" + suffix + ")", m_Accessor == AccessorMode.Public ? "1" : "0");
			builder.Replace("$(" + prefix + "IsPrivate" + suffix + ")", m_Accessor == AccessorMode.Private ? "1" : "0");
			builder.Replace("$(" + prefix + "IsProtected" + suffix + ")", m_Accessor == AccessorMode.Protected ? "1" : "0");
			
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
			return m_TypeToken.GetReflectionHash().ToString("x");
		}
		
		
		public override StringBuilder GenerateDeclarationContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateDeclarationContent(context);

			// Create new variable, as requested
			if (m_DeclarationReflecitonState == ReflectionState.ProvideDefault)
			{
				builder.Append(@"
#if $(PreProcessorCondition)
$(Accessor):
$(FullTypeToken) $(Name);
#endif
");
			}
			
			return ExpandMacros(builder, context);
		}

		public override StringBuilder GenerateImplementationContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateImplementationContent(context);

			// Create new variable as requested
			if (m_DeclarationReflecitonState == ReflectionState.ProvideDefault && m_TypeToken.IsStatic)
			{
				builder.Append(@"
#if $(PreProcessorCondition)
$(FullTypeToken) $(Parent.Name)::$(Name);
#endif
");
			}
			
			return ExpandMacros(builder, context);
		}
	}
}
