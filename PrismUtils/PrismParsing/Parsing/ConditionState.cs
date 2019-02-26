using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing
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
				string condition = string.Join(" && ", m_StateTree);
				if (condition == "")
					return "1";
				else
					return condition;
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


		private struct DirectiveState
		{
			public bool RecognisedStatement;
			public bool CurrentWrite;
		};
		
		/// <summary>
		/// Attempt to pre-expande any simple directives
		/// </summary>
		public static string PreExpandDirectives(string raw)
		{
			string output = "";

			Stack<DirectiveState> stateStack = new Stack<DirectiveState>();

			DirectiveState currentState = new DirectiveState();
			currentState.RecognisedStatement = true;
			currentState.CurrentWrite = true;

			foreach (string line in raw.Split('\n'))
			{
				string searchLine = line.Trim();
				if (searchLine.StartsWith("#if"))
				{
					stateStack.Push(currentState);

					if (searchLine == "#if 1")
					{
						currentState.RecognisedStatement = true;
						currentState.CurrentWrite = currentState.CurrentWrite && true;
						continue;
					}
					else if (searchLine == "#if 0")
					{
						currentState.RecognisedStatement = true;
						currentState.CurrentWrite = false;
						continue;
					}
					else
					{
						currentState.RecognisedStatement = false;
						currentState.CurrentWrite = true;
					}
				}
				else
				{
					if (searchLine.StartsWith("#elif "))
					{
						if (currentState.RecognisedStatement)
						{
							if (currentState.CurrentWrite)
							{
								currentState.CurrentWrite = false;
								continue;
							}
							else
							{
								currentState.CurrentWrite = !currentState.CurrentWrite;
								continue;
							}
						}
					}
					else if (searchLine == "#else")
					{
						if (currentState.RecognisedStatement)
						{
							currentState.CurrentWrite = !currentState.CurrentWrite;

							// Make sure we're allowed to be re-enabling it
							if (stateStack.Count != 0)
							{
								currentState.CurrentWrite = currentState.CurrentWrite && stateStack.Peek().CurrentWrite;
							}

							continue;
						}
					}
					else if (searchLine == "#endif")
					{
						bool skip = currentState.RecognisedStatement;

						currentState = stateStack.Pop();

						if (skip)
							continue;
					}
				}

				if (currentState.CurrentWrite)
					output += line + "\n";
			}

			return output;
		}
	}
}
