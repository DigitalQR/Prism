using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	/// <summary>
	/// Track the condition tree for boolean operations
	/// Used with keeping track of #if statements
	/// </summary>
	public class ConditionState
	{
		/// <summary>
		/// Current active states
		/// </summary>
		private List<string> m_StateTree;

		public ConditionState()
		{
			m_StateTree = new List<string>();
		}

		public string CurrentCondition
		{
			get
			{
				return string.Join(" && ", m_StateTree);
			}
		}

		public void AddIF(string condition)
		{
			m_StateTree.Add("(" + condition + ")");
		}

		public void AddELSE()
		{
			if (m_StateTree.Count != 0)
			{
				int lastIndex = m_StateTree.Count - 1;
				m_StateTree[lastIndex] = "!(" + m_StateTree[lastIndex] + ")";
			}
		}

		public void AddELSEIF(string condition)
		{
			if (m_StateTree.Count != 0)
			{
				int lastIndex = m_StateTree.Count - 1;
				m_StateTree[lastIndex] = "!(" + m_StateTree[lastIndex] + ") && (" + condition + ")";
			}
		}

		public void EndIF()
		{
			if (m_StateTree.Count != 0)
			{
				int lastIndex = m_StateTree.Count - 1;
				m_StateTree.RemoveAt(lastIndex);
			}
		}
	}
}
