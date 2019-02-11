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

		/// <summary>
		/// The namespace this token was declared in
		/// </summary>
		private string[] m_TokenNamespace;
		
		protected StructureReflectionBase(string name, string[] tokenNamespace, ConditionState conditionState, string bodyFile, int bodyLine, string tokenParams, string docString)
			: base(conditionState, bodyFile, bodyLine, tokenParams, docString)
		{
			m_DeclerationName = name;
			m_TokenNamespace = tokenNamespace == null ? new string[0] : tokenNamespace;
		}
				
		/// <summary>
		/// Get the appropriate StructureReflection based on the signature
		/// </summary>
		public static StructureReflectionBase RetrieveFromSignature(SignatureInfo sigInfo, string[] tokenNamespace, ConditionState conditionState, string tokenFile, int tokenBodyLine, string tokenParams, string docString)
		{
			if (sigInfo.SignatureType != SignatureInfo.SigType.StructureImplementationBegin)
			{
				throw new ReflectionException(ReflectionErrorCode.ParseUnexpectedSignature, sigInfo, "Cannot retrieve structure reflection from this signature");
			}

			var data = (StructureSignature.ImplementationBeginData)sigInfo.AdditionalParam;

			if (data.StructureType == "class" || data.StructureType == "struct")
			{
				return new StructureReflection(data, tokenNamespace, conditionState, tokenFile, tokenBodyLine, tokenParams, docString);
			}
			else if (data.StructureType == "enum")
			{
				return new EnumStructureReflection(data, tokenNamespace, conditionState, tokenFile, tokenBodyLine, tokenParams, docString);
			}

			throw new ReflectionException(ReflectionErrorCode.ParseUnexpectedSignature, sigInfo, "No reflection structure for this current signature");
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
		/// The namespace this token was declared in
		/// </summary>
		public string[] TokenNamespace
		{
			get { return m_TokenNamespace; }
		}

		/// <summary>
		/// The namespace this token was declared in
		/// </summary>
		public string TokenNamespaceFormatted
		{
			get { return m_TokenNamespace.Length != 0 ? string.Join("::", m_TokenNamespace) : ""; }
		}

		/// <summary>
		/// Add a specific signature to the reflection of this structure
		/// </summary>
		public abstract void AddInternalSignature(SignatureInfo sigInfo, string accessor, ConditionState conditionState, string tokenFile, int tokenLine, string tokenParams, string docString);

	}
}
