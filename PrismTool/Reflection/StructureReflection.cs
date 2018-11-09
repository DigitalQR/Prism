using Prism.CodeParsing;
using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public class FunctionReflection : TokenReflection
	{
		/// <summary>
		/// The info about this function
		/// </summary>
		private FunctionInfo m_ReflectionInfo;

		public FunctionReflection(FunctionInfo function, ConditionState conditionState, string tokenParams, string docString)
			: base(conditionState, tokenParams, docString)
		{
			m_ReflectionInfo = function;
		}

		public FunctionInfo ReflectionInfo
		{
			get { return m_ReflectionInfo; }
		}
	}

	public class VariableReflection : TokenReflection
	{
		/// <summary>
		/// The info about this variable
		/// </summary>
		private VariableInfo m_ReflectionInfo;

		public VariableReflection(VariableInfo variable, ConditionState conditionState, string tokenParams, string docString)
			: base(conditionState, tokenParams, docString)
		{
			m_ReflectionInfo = variable;
		}

		public VariableInfo ReflectionInfo
		{
			get { return m_ReflectionInfo; }
		}
	}

	/// <summary>
	/// Reflection structure for normal structure i.e. struct/class
	/// </summary>
	public class StructureReflection : StructureReflectionBase
	{
		private class StructureParent
		{
			public string DeclerationName;
			public string Accessor;
		}
		
		/// <summary>
		/// The type of this structure
		/// </summary>
		private string m_StructureType;

		/// <summary>
		/// The parent stuctures of this new instance
		/// </summary>
		private List<StructureParent> m_StructureParents;

		/// <summary>
		/// All the variables in this structure
		/// </summary>
		private List<VariableReflection> m_Variables;

		/// <summary>
		/// All the functions in this structure
		/// </summary>
		private List<FunctionReflection> m_Functions;

		public StructureReflection(StructureSignature.ImplementationBeginData data, ConditionState conditionState, string tokenParams, string docString)
			: base(data.DeclareName, conditionState, tokenParams, docString)
		{
			m_StructureType = data.StructureType;

			m_Variables = new List<VariableReflection>();
			m_Functions = new List<FunctionReflection>();
			m_StructureParents = new List<StructureParent>();

			if (data.ParentCount != 0)
			{
				foreach (var inheritData in data.ParentStructures)
				{
					StructureParent parent = new StructureParent();
					parent.DeclerationName = inheritData.DeclareName;
					parent.Accessor = inheritData.Access;
					m_StructureParents.Add(parent);
				}
			}
		}

		public override string StructureType
		{
			get { return m_StructureType; }
		}

		public FunctionReflection[] Functions
		{
			get { return m_Functions.ToArray(); }
		}

		public VariableReflection[] Variables
		{
			get { return m_Variables.ToArray(); }
		}

		public override void AddInternalSignature(SignatureInfo sigInfo, string accessor, ConditionState conditionState, string tokenParams, string docString)
		{
			if (sigInfo.SignatureType == SignatureInfo.SigType.FunctionDeclare)
			{
				var data = (FunctionInfo)sigInfo.AdditionalParam;
				FunctionReflection refl = new FunctionReflection(data, conditionState, tokenParams, docString);
				m_Functions.Add(refl);
			}
			else if (sigInfo.SignatureType == SignatureInfo.SigType.VariableDeclare)
			{
				var data = (VariableInfo)sigInfo.AdditionalParam;
				VariableReflection refl = new VariableReflection(data, conditionState, tokenParams, docString);
				m_Variables.Add(refl);
			}
		}
	}
}
