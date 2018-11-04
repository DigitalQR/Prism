using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	public class PreProcessorDefinitionSignature
	{
		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			if (content.StartsWith("#"))
			{
				// TODO - Extend this to fetch more info about the def

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.PreProcessorDefinition);
				return true;
			}
			
			sigInfo = null;
			return false;
		}
	}
}
