using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class PreProToken : CppTokenType<PreProToken.ParsedData>
	{
		public class ParsedData
		{
			public string[] m_Directive;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData parsedData)
		{
			if (input.StartsWith("#"))
			{
				parsedData = new ParsedData();
				parsedData.m_Directive = TokenUtils.SplitSyntax(input);
				return true;
			}

			parsedData = null;
			return false;
		}
	}
}
