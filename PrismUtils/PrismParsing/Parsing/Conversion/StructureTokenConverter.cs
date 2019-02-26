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
	public static class StructureTokenConverter
	{
		/// <summary>
		/// Select the appropriate token based on signature
		/// (Will only work for StructureImplementationBegin Signature)
		/// </summary>
		public static IReflectableToken Convert(SignatureInfo sigInfo, string[] tokenNamespace, ConditionState conditionState, TokenOrigin origin, string tokenParams, string docString)
		{
			if (sigInfo.SignatureType != SignatureInfo.SigType.StructureImplementationBegin)
			{
				throw new ParseException(ParseErrorCode.ParseUnexpectedSignature, sigInfo, "Cannot retrieve structure from this signature");
			}
			
			var data = (StructureSignature.ImplementationBeginData)sigInfo.AdditionalParam;

			if (data.StructureType == "class" || data.StructureType == "struct")
			{
				StructureToken token = new StructureToken(data.DeclareName, data.StructureType, tokenNamespace, conditionState.CurrentCondition, ReflectionState.Discovered, ParseParentInfo(data.ParentStructures));
				token.Documentation = docString;

				return token;
			}
			//else if (data.StructureType == "enum")
			//{
			//	return new EnumStructureReflection(data, tokenNamespace, conditionState, tokenFile, tokenBodyLine, tokenParams, docString);
			//}

			throw new ParseException(ParseErrorCode.ParseUnexpectedSignature, sigInfo, "No structure infomation for this current signature");

		}

		private static ScopedStructure[] ParseParentInfo(StructureSignature.ImplementationBeginData.ParentStructure[] parents)
		{
			if (parents == null || parents.Length == 0)
				return new ScopedStructure[0];

			ScopedStructure[] outParents = new ScopedStructure[parents.Length];

			for (int i = 0; i < parents.Length; ++i)
				outParents[i] = new ScopedStructure(AccessorModeUtils.Parse(parents[i].Access), parents[i].DeclareName);

			return outParents;
		}
	}
}
