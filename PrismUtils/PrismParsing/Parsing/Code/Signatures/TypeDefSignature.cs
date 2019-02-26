using Prism.Parsing.Code.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Code.Signatures
{
	public class TypeDefSignature
	{
		public class ParseData
		{
			public string Definition;
			public string TypeName;
		}

		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			if (content.StartsWith("typedef"))
			{
				content = content.Substring("typedef".Length).Trim();

				ParseData data = new ParseData();
				int splitIndex = content.LastIndexOf(' ');
				data.Definition = content.Substring(0, splitIndex).Trim();
				data.TypeName = content.Substring(splitIndex + 1).Trim();

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.TypeDefDeclare, data);
				return true;
			}
			else if (content.StartsWith("using") && content.Contains('='))
			{
				content = content.Substring("using".Length).Trim();

				ParseData data = new ParseData();
				int splitIndex = content.LastIndexOf('=');
				data.TypeName = content.Substring(0, splitIndex).Trim();
				data.Definition = content.Substring(splitIndex + 1).Trim();

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.TypeDefDeclare, data);
				return true;
			}

			sigInfo = null;
			return false;
		}
	}
}
