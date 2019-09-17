using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class NamespaceToken : CppTokenType<NamespaceToken.ParsedData>
	{
		public Stack<Tuple<string, int>> m_ActiveNamespace;

		public class ParsedData
		{
			public string[] m_Namespace;
			public bool m_IsPush;
		}

		public NamespaceToken()
		{
			m_ActiveNamespace = new Stack<Tuple<string, int>>();
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			string startToken = "namespace";
			if (input.StartsWith(startToken) && input.EndsWith("{"))
			{
				string namespaceName = input.Substring(startToken.Length, input.Length - startToken.Length - 1).Trim();
				PushNamespace(reader, namespaceName);

				data = new ParsedData
				{
					m_Namespace = m_ActiveNamespace.Select((ns) => ns.Item1).ToArray(),
					m_IsPush = true
				};
				return true;
			}
			else if (input.EndsWith("}"))
			{
				if (TryPopNamespace(reader))
				{
					data = new ParsedData
					{
						m_Namespace = m_ActiveNamespace.Select((ns) => ns.Item1).ToArray(),
						m_IsPush = false
					};
					return true;
				}
			}

			data = null;
			return false;
		}

		private void PushNamespace(CppTokenReader reader, string name)
		{
			Tuple<string, int> nsInfo = new Tuple<string, int>(name, reader.CurrentBlockDepth + 1);
			m_ActiveNamespace.Push(nsInfo);
		}

		private bool TryPopNamespace(CppTokenReader reader)
		{
			if (m_ActiveNamespace.Any())
			{
				Tuple<string, int> nsInfo = m_ActiveNamespace.Peek();
				if (nsInfo.Item2 == reader.CurrentBlockDepth)
				{
					m_ActiveNamespace.Pop();
					return true;
				}
			}

			return false;
		}
	}
}
