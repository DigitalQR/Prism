using System;
using System.Collections.Generic;
using System.Text;
using Prism.Reflection.Behaviour;

namespace Prism.Reflection.Tokens
{
	public abstract class ReflectableTokenBase : AttributeCollection, IReflectableToken
	{
		private TokenOrigin m_TokenOrigin;
		private BehaviourTarget m_BehaviourTarget;
		private StringBuilder m_DeclarationContent;
		private StringBuilder m_ImplementationContent;
		private StringBuilder m_IncludeContent;

		public ReflectableTokenBase(TokenOrigin origin, BehaviourTarget supportedTargets, string attribParams)
			: base(attribParams)
		{
			m_TokenOrigin = origin;
			m_BehaviourTarget = supportedTargets;
			m_DeclarationContent = new StringBuilder();
			m_ImplementationContent = new StringBuilder();
			m_IncludeContent = new StringBuilder();
		}

		/// <summary>
		/// Get the origin information for this token
		/// </summary>
		public TokenOrigin Origin => m_TokenOrigin; 

		/// <summary>
		/// Behaviour targets which this token will/can target
		/// </summary>
		public BehaviourTarget SupportedTargets => m_BehaviourTarget;

		/// <summary>
		/// Get a enumerator of all the internal tokens this token may have
		/// (Will return null, if there are no tokens)
		/// </summary>
		public virtual IReadOnlyList<IReflectableToken> InternalTokens => null;

		/// <summary>
		/// Append content which will be added to the refl.h inside of the REFLECT_TOKEN
		/// </summary>
		public void AppendDeclarationContent(StringBuilder builder)
		{
			m_DeclarationContent.Append(builder);
		}

		/// <summary>
		/// Append content which will be added to the refl.cpp
		/// </summary>
		public void AppendImplementationContent(StringBuilder builder)
		{
			m_ImplementationContent.Append(builder);
		}

		/// <summary>
		/// Append content which will be added to the refl.h outside of the REFLECT_TOKEN
		/// </summary>
		public void AppendIncludeContent(StringBuilder builder)
		{
			m_IncludeContent.Append(builder);
		}
		
		public virtual StringBuilder GenerateDeclarationContent(IReflectableToken context)
		{
			return new StringBuilder(m_DeclarationContent.ToString());
		}

		public virtual StringBuilder GenerateImplementationContent(IReflectableToken context)
		{
			return new StringBuilder(m_ImplementationContent.ToString());
		}

		public virtual StringBuilder GenerateIncludeContent(IReflectableToken context)
		{
			StringBuilder builder = new StringBuilder(m_IncludeContent.ToString());

			// Add some nicer complier errors for attributes that haven't been found
			int i = 0;
			foreach (AttributeData attrib in DataAttributes)
			{
				string check = @"
__if_not_exists($(Attribute[%i].Name)Attribute)
{
#pragma message(R""($(SourceFilePath)($(TokenOriginLine)): error P202: Cannot find attribute '$(Attribute[%i].Name)Attribute')"");
}
";
				builder.Append(check.Replace("%i", i.ToString()));
				++i;
			}
			
			return builder;
		}
	}
}
