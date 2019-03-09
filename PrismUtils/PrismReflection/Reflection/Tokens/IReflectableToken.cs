using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Tokens
{
	public interface IReflectableToken
	{
		/// <summary>
		/// All the supported behaviours of this target
		/// </summary>
		BehaviourTarget SupportedTargets { get; }

		/// <summary>
		/// Get a enumerator of all the internal tokens this token may have
		/// (Will return null, if there are no tokens)
		/// </summary>
		IReadOnlyList<IReflectableToken> InternalTokens { get; }

		/// <summary>
		/// Generate any content which will be added to the refl.h outside of the REFLECT_TOKEN
		/// </summary>
		/// <param name="context">If there is any context that this reflectable token is inside of e.g. function context will be class, class context will be null</param>
		StringBuilder GenerateIncludeContent(IReflectableToken context);

		/// <summary>
		/// Generate any content which will be added to the refl.h inside of the REFLECT_TOKEN
		/// </summary>
		/// <param name="context">If there is any context that this reflectable token is inside of e.g. function context will be class, class context will be null</param>
		StringBuilder GenerateDeclarationContent(IReflectableToken context);

		/// <summary>
		/// Generate any content which will be added to the refl.cpp
		/// </summary>
		/// <param name="context">If there is any context that this reflectable token is inside of e.g. function context will be class, class context will be null</param>
		StringBuilder GenerateImplementationContent(IReflectableToken context);
	}
}
