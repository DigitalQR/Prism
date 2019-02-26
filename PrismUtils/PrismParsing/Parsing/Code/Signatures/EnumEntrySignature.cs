using Prism.Parsing.Code.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Code.Signatures
{
	public class EnumEntrySignature
	{
		public class EnumValueData
		{
			public string Name;
			public string HardcodedValue;
		}

		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			// Assume are enum values, if nothing else could parse them
			int index = content.IndexOf(',');
			string entry;
			if (index == -1)
			{
				entry = content;
			}
			else
			{
				entry = content.Substring(0, index).Trim();
				reader.LeftOverContent = content.Substring(index + 1);
			}

			EnumValueData data = new EnumValueData();

			int assignIndex = entry.IndexOf('=');
			if (assignIndex == -1)
			{
				data.Name = entry;
				data.HardcodedValue = null;
			}
			else
			{
				data.Name = entry.Substring(0, assignIndex).Trim();
				data.HardcodedValue = entry.Substring(assignIndex + 1).Trim();
			}

			sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.EnumValueEntry, data);
			return true;
		}
	}
}
