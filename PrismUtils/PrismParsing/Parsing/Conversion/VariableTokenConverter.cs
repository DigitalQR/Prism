using Prism.Parsing;
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
	public static class VariableTokenConverter
	{
		public static VariableToken Convert(SignatureInfo sigInfo, string currentScope, ConditionState conditionState, TokenOrigin origin, string tokenParams, string docString)
		{
			if (sigInfo.SignatureType != SignatureInfo.SigType.VariableDeclare)
			{
				throw new ParseException(ParseErrorCode.ParseUnexpectedSignature, sigInfo, "Cannot retrieve variable from this signature");
			}

			var data = (VariableInfo)sigInfo.AdditionalParam;

			VariableToken token = new VariableToken(AccessorModeUtils.Parse(currentScope), data.VariableName, data.TypeInfo.ToTypeToken(), conditionState.CurrentCondition, ReflectionState.Discovered);
			return token;
		}
	}
}
