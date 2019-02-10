using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	public class AccessorSignature
	{
		public class ParseData
		{
			public string Accessor;
		}

		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			ParseData data = new ParseData();
			bool found = false;
			string searchString = content;

			if (CheckForAccessor("public", ref searchString, reader))
			{
				data.Accessor = "public";
				found = true;
			}
			else if (CheckForAccessor("private", ref searchString, reader))
			{
				data.Accessor = "private";
				found = true;
			}
			else if (CheckForAccessor("protected", ref searchString, reader))
			{
				data.Accessor = "protected";
				found = true;
			}

			if (found)
			{
				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.AccessorSet, data);
				return true;
			}

			sigInfo = null;
			return false;
		}

		private static bool CheckForAccessor(string accessor, ref string content, SafeLineReader reader)
		{
			if (content.StartsWith(accessor))
			{
				// Look for following ':'
				content = content.Substring(accessor.Length).Trim();

				string tempLine;
				if (content == "" && reader.SafeReadNext(out tempLine))
				{
					content = (content + tempLine).Trim();
				}

				if(content.StartsWith(":"))
				{
					reader.LeftOverContent = content.Substring(1);
					return true;
				}
				else
				{
					reader.LeftOverContent = content;
				}
			}

			return false;
		}
	}
}
