using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	public class PreProcessorSignature
	{
		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			if (content.StartsWith("#"))
			{
				// TODO - Extend this to fetch more info about the def (Safely handle macro defs \)

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.PreProcessorDirective);
				return true;
			}
			
			sigInfo = null;
			return false;
		}
	}
}
