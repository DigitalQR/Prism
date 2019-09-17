using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp
{
	public class CppLineReader : TokenReader
	{
		/// <summary>
		/// Common end of line tokens to look for
		/// </summary>
		private char[] m_EndOfLineTokens;

		public CppLineReader(Stream stream)
			: base(stream)
		{
			m_EndOfLineTokens = new char[] { ';', '{', '}' };
		}

		public override bool Next(out string token, out long lineNumber)
		{
			if (FindTokenStart())
			{
				lineNumber = this.LineNumber;

				// Read pre-pro tokens
				if (CurrentChar == '#')
				{
					ReadUntilChar('\n');
					token = CurrentToken;
					return true;
				}

				// Read comments
				if (CurrentChar == '/')
				{
					if (NextChar())
					{
						// Single line
						if (CurrentChar == '/')
						{
							ReadUntilChar('\n');
							token = CurrentToken;
							return true;
						}
						// Block comment
						else if (CurrentChar == '*')
						{
							ReadUntilString("*/");
							token = CurrentToken;
							return true;
						}
					}
				}

				// Deal with macros??

				// Must just be regular thing
				if (!m_EndOfLineTokens.Contains(CurrentChar))
				{
					ReadUntilChar(m_EndOfLineTokens);
				}

				token = CurrentToken;
				return true;
			}

			lineNumber = -1;
			token = null;
			return false;
		}
		
	}
}
