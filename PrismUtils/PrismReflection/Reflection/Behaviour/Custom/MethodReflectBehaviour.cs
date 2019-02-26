using System;
using System.Collections.Generic;
using System.Text;
using Prism.Reflection.Tokens;

namespace Prism.Reflection.Behaviour.Custom
{
	public class MethodReflectBehaviour : GlobalReflectionBehaviour
	{
		public MethodReflectBehaviour()
			: base(BehaviourTarget.Function, 0)
		{

		}

		public override void RunBehaviour(IReflectableToken target)
		{
			FunctionToken token = target as FunctionToken;

			if (token == null)
			{
				throw new Exception("Invalid state reached. Internal_ReflectMethod expected a FunctionToken target");
			}

			token.AppendDeclarationContent(GenerateDeclarationContent(token));
			token.AppendImplementationContent(GenerateImplementationContent(token));
		}

		private StringBuilder GenerateDeclarationContent(FunctionToken target)
		{
			StringBuilder builder = new StringBuilder();
			
			// TODO -

			return builder;
		}

		private StringBuilder GenerateImplementationContent(FunctionToken target)
		{
			StringBuilder builder = new StringBuilder();

			// TODO -

			return builder;
		}
	}
}
