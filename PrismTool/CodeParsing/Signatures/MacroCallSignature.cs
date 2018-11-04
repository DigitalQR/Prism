using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	public class MacroCallSignature
	{
		public class ParseData
		{
			public string MacroName;
			public string MacroParams;
		}

		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			bool foundOpenBracket = false;
			int bracketDepth = 0;
			int index = 0;
			int startBracket = -1;

			// Attempt to look for a macro-call in pattern MY_CALL(...)
			foreach(char c in content)
			{
				if (foundOpenBracket)
				{
					if (c == '(')
					{
						++bracketDepth;
					}
					else if (c == ')')
					{
						--bracketDepth;
						if (bracketDepth == 0)
						{
							reader.LeftOverContent = content.Substring(index + 1);
							content = content.Substring(0, index + 1);

							ParseData data = new ParseData();
							data.MacroName = content.Substring(0, startBracket);
							data.MacroParams = content.Substring(startBracket + 1, content.Length - startBracket - 2);

							sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.MacroCall, data);
							return true;
						}
					}
				}
				else
				{
					if (char.IsWhiteSpace(c))
					{
						break;
					}
					else if (c == '(')
					{
						foundOpenBracket = true;
						startBracket = index;
						++bracketDepth;
					}
				}

				++index;
			}

			

			sigInfo = null;
			return false;
		}
	}
}
