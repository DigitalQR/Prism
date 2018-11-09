using Prism.CodeParsing;
using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public abstract class StructureReflectionBase : TokenReflection
	{
		/// <summary>
		/// The name of this instance of this structure
		/// </summary>
		private string m_DeclerationName;

		protected StructureReflectionBase(string name, ConditionState conditionState, string tokenParams, string docString)
			: base(conditionState, tokenParams, docString)
		{
			m_DeclerationName = name;
		}
				
		/// <summary>
		/// Get the appropriate StructureReflection based on the signature
		/// </summary>
		public static StructureReflectionBase RetrieveFromSignature(SignatureInfo sigInfo, ConditionState conditionState, string tokenParams, string docString)
		{
			if (sigInfo.SignatureType != SignatureInfo.SigType.StructureImplementationBegin)
			{
				throw new ReflectionException(sigInfo, "Cannot retrieve structure reflection from this signature");
			}

			var data = (StructureSignature.ImplementationBeginData)sigInfo.AdditionalParam;

			if (data.StructureType == "class" || data.StructureType == "struct")
			{
				return new StructureReflection(data, conditionState, tokenParams, docString);
			}
			else if (data.StructureType == "enum")
			{
				return new EnumStructureReflection(data, conditionState, tokenParams, docString);
			}

			throw new ReflectionException(sigInfo, "No reflection structure for this current signature");
		}
		
		/// <summary>
		/// Get the in code keyword used for this type of structure
		/// </summary>
		public abstract string StructureType { get; }

		/// <summary>
		/// Get the in code typename for this new structure
		/// </summary>
		public string DeclerationName
		{
			get { return m_DeclerationName; }
		}

		/// <summary>
		/// Add a specific signature to the reflection of this structure
		/// </summary>
		public abstract void AddInternalSignature(SignatureInfo sigInfo, string accessor, ConditionState conditionState, string tokenParams, string docString);

	}
}
