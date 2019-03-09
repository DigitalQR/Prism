using Prism.Parsing.Code.Signatures;
using Prism.Reflection;
using Prism.Reflection.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Conversion
{
	public class FunctionTokenConverter
	{
		public static FunctionToken Convert(SignatureInfo sigInfo, string currentScope, ConditionState conditionState, TokenOrigin origin, string tokenParams, string docString)
		{
			FunctionToken token = null;

			if (sigInfo.SignatureType == SignatureInfo.SigType.FunctionDeclare)
			{
				var data = (FunctionInfo)sigInfo.AdditionalParam;

				token = new FunctionToken(
					origin, AccessorModeUtils.Parse(currentScope), data.FunctionName, conditionState.CurrentCondition,
					ConvertToTypeTokens(data.ParamTypes),
					data.ReturnType != null ? data.ReturnType.ToTypeToken() : TypeToken.VoidType, 
					data.ToFunctionProperties(), ReflectionState.Discovered
				);
			}
			else if (sigInfo.SignatureType == SignatureInfo.SigType.StructureConstructor)
			{
				var data = (ConstructorInfo)sigInfo.AdditionalParam;

				FunctionProperties properties = FunctionProperties.Constructor;
				if (data.IsInline) properties |= FunctionProperties.Inline;
				if (data.IsExplicit) properties |= FunctionProperties.Explicit;

				token = new FunctionToken(
					origin, AccessorModeUtils.Parse(currentScope), data.DeclareName + "Constructor", conditionState.CurrentCondition,
					ConvertToTypeTokens(data.ParamTypes), TypeToken.VoidType,
					properties, ReflectionState.Discovered
				);
			}
			else if (sigInfo.SignatureType == SignatureInfo.SigType.StructureDestructor)
			{
				var data = (DestructorInfo)sigInfo.AdditionalParam;

				FunctionProperties properties = FunctionProperties.Destructor;
				if (data.IsInline) properties |= FunctionProperties.Inline;
				if (data.IsVirtual) properties |= FunctionProperties.Virtual;

				token = new FunctionToken(
					origin, AccessorModeUtils.Parse(currentScope), data.DeclareName + "Destructor", conditionState.CurrentCondition,
					ConvertToTypeTokens(null), TypeToken.VoidType,
					properties, ReflectionState.Discovered
				);
			}
			else
			{
				throw new ParseException(ParseErrorCode.ParseUnexpectedSignature, sigInfo, "Cannot retrieve function from this signature");
			}
			
			return token;
		}

		private static NamedTypeToken[] ConvertToTypeTokens(VariableInfo[] rawParams)
		{
			if (rawParams == null || rawParams.Length == 0)
				return new NamedTypeToken[0];
			
			NamedTypeToken[]  paramTypes = new NamedTypeToken[rawParams.Length];

			for (int i = 0; i < rawParams.Length; ++i)
				paramTypes[i] = new NamedTypeToken(rawParams[i].VariableName, rawParams[i].TypeInfo.ToTypeToken());

			return paramTypes;
		}
	}
}
