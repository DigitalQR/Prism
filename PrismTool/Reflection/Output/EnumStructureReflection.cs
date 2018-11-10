using Prism.CodeParsing;
using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	/// <summary>
	/// Reflection structure for enums
	/// </summary>
	public class EnumStructureReflection : StructureReflectionBase
	{
		public class Value
		{
			public string Name;
			public string HardcodedValue;
			public string DocString;
			public string PreProcessorCondition;
		}

		/// <summary>
		/// The values belonging to this enum
		/// </summary>
		private List<Value> m_Values;

		public EnumStructureReflection(StructureSignature.ImplementationBeginData data, string[] tokenNamespace, ConditionState conditionState, int bodyLine, string tokenParams, string docString)
			: base(data.DeclareName, tokenNamespace, conditionState, bodyLine, tokenParams, docString)
		{
			m_Values = new List<Value>();
		}

		public override string StructureType
		{
			get { return "enum"; }
		}

		public Value[] Values
		{
			get { return m_Values.ToArray(); }
		}

		public override void AddInternalSignature(SignatureInfo sigInfo, string accessor, ConditionState conditionState, int tokenLine, string tokenParams, string docString)
		{
			// Ignore other values (Will get caught by compiler anyway)
			if (sigInfo.SignatureType == SignatureInfo.SigType.EnumValueEntry)
			{
				var data = (EnumEntrySignature.EnumValueData)sigInfo.AdditionalParam;

				Value value = new Value();
				value.Name = data.Name;
				value.HardcodedValue = data.HardcodedValue;
				value.DocString = docString;
				value.PreProcessorCondition = conditionState.CurrentCondition;
				m_Values.Add(value);
			}
		}

		public override string GenerateHeaderReflectionContent()
		{
			throw new NotImplementedException();
		}

		public override string GenerateIncludeReflectionContent()
		{
			throw new NotImplementedException();
		}

		public override string GenerateSourceReflectionContent()
		{
			throw new NotImplementedException();
		}
	}
}
