using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Tokens
{
	public class EnumValueToken : ReflectableTokenBase
	{
		private string m_Name;
		private string m_PreProcessorCondition;

		private string m_Documentation;
		private ReflectionState m_DeclarationReflectionState;

		/// <summary>
		/// Code name for this value
		/// </summary>
		public string Name
		{
			get { return m_Name; }
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
			get { return m_DeclarationReflectionState; }
		}

		public EnumValueToken(TokenOrigin origin, string name, string preProcessorCondition, ReflectionState declarationState)
			: base(origin, BehaviourTarget.EnumuratorValue, null)
		{
			m_Name = name;
			m_PreProcessorCondition = preProcessorCondition;
			m_DeclarationReflectionState = declarationState;
			m_Documentation = "";
		}

		/// <summary>
		/// Expand any macros relating to this type (Missing macros will be left)
		/// $(PreProcessorCondition)
		/// $(Name)
		/// $(Documentation)
		/// </summary>
		/// <param name="input">The raw input string which should have it's macros replaced</param>
		/// <param name="prefix">The prefix to apply to every macro</param>
		/// <param name="suffix">The suffix to apply to each macro</param>
		/// <returns>The string with all relevent macros expanded</returns>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			builder.Replace("$(" + prefix + "PreProcessorCondition" + suffix + ")", string.IsNullOrWhiteSpace(m_PreProcessorCondition) ? "1" : m_PreProcessorCondition);
			builder.Replace("$(" + prefix + "Name" + suffix + ")", m_Name);
			builder.Replace("$(" + prefix + "Documentation" + suffix + ")", m_Documentation);

			//ExpandAttributeMacros(builder, prefix, suffix);
			return builder;
		}

		public override StringBuilder GenerateDeclarationContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateDeclarationContent(context);
			return ExpandMacros(builder);
		}

		public override StringBuilder GenerateImplementationContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateImplementationContent(context);
			return ExpandMacros(builder);
		}

		public override StringBuilder GenerateIncludeContent(IReflectableToken context)
		{
			StringBuilder builder = base.GenerateIncludeContent(context);
			return ExpandMacros(builder);
		}
	}
}
