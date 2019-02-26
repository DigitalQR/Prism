using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection
{
	public enum AttributeStatus
	{
		/// <summary>
		/// The state of this attribute is not currently known
		/// </summary>
		Unknown,

		/// <summary>
		/// The state of this attribute is invalid, so will be ignored
		/// </summary>
		Invalid,

		/// <summary>
		/// Code-side data attribute which can be ran at runtime
		/// </summary>
		Data,

		/// <summary>
		/// Reflection-side behaviour attribute which can perform some behaviour
		/// </summary>
		Behaviour,
	}
		
	public class AttributeData
	{
		private string m_Name;
		private string[] m_Params;
		private BehaviourTarget m_Target;
		private AttributeStatus m_Status;

		/// <summary>
		/// The name of the desired attribute
		/// </summary>
		public string Name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// The params that are being used with this attribute instance
		/// </summary>
		public string[] Params
		{
			get { return m_Params; }
		}

		/// <summary>
		/// The current target of this attribute instance
		/// </summary>
		public BehaviourTarget Target
		{
			get { return m_Target; }
		}

		/// <summary>
		/// The current parse status of this attribute
		/// </summary>
		public AttributeStatus Status
		{
			get { return m_Status; }
			internal set { m_Status = value; }
		}

		public AttributeData(string name, string[] initParams, BehaviourTarget target)
		{
			m_Name = name;
			m_Params = initParams;
			m_Target = target;
		}

		public bool HasParam(string param, StringComparison comparison = StringComparison.CurrentCulture)
		{
			foreach (string str in m_Params)
			{
				if (str.Equals(param, comparison))
					return true;
			}

			return false;
		}
	}
}
