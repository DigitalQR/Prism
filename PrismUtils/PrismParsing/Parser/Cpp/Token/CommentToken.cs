using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class CommentToken : CppTokenType<CommentToken.ParsedData>
	{
		public class ParsedData
		{
			public string m_Content;
			public bool m_IsBlock;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			if (input.StartsWith("//"))
			{
				data = new ParsedData { m_Content = input.Substring(3), m_IsBlock = false };
				return true;
			}
			else if (input.StartsWith("/*"))
			{
				string content = input.Substring(3);
				content = content.Substring(0, content.Length - 2);
				data = new ParsedData { m_Content = content, m_IsBlock = true };
				return true;
			}

			data = null;
			return false;
		}
	}
}
