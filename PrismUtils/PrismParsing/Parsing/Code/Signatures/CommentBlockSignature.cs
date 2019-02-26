using Prism.Parsing.Code.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Code.Signatures
{
	public class CommentBlockSignature
	{			
		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			// Single-line comment block
			if (content.StartsWith("//"))
			{
				string tempLine;

				// Attempt to keep reading until a line without '//' is found
				while (reader.SafeReadNext(out tempLine))
				{
					if (tempLine.StartsWith("//"))
					{
						content += '\n' + tempLine;
					}
					else
					{
						reader.LeftOverContent = tempLine;
						break;
					}
				}

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.CommentBlock);
				return true;
			}
			// Multi-line comment block
			else if (content.StartsWith("/*"))
			{
				string tempLine = "";

				// Attempt to look for the end of the comment block
				do
				{
					if (tempLine != "")
						content += '\n' + tempLine;

					int index = content.IndexOf("*/");
					if (index != -1)
					{
						reader.LeftOverContent = content.Substring(index + 2).Trim();
						content = content.Substring(0, index + 2);
						break;
					}
				}
				while (reader.SafeReadNext(out tempLine));

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.CommentBlock);
				return true;
			}

			sigInfo = null;
			return false;
		}
	}
}
