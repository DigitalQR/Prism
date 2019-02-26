using Prism.Parsing.Code.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Code.Signatures
{
	public class FriendSignature
	{
		public class FriendData
		{
			public string FriendName;
		}

		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			// Try to check if we are currently looking at a variable
			if (content.StartsWith("friend "))
			{
				// Remove ;
				content = content.Substring(0, content.Length - 1);

				FriendData data = new FriendData();
				data.FriendName = content.Substring("friend ".Length).Trim();

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.FriendDeclare, data);
				return true;
			}

			sigInfo = null;
			return false;
		}
	}
}
