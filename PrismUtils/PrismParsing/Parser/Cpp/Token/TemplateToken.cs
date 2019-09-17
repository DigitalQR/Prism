using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class TemplateToken : CppTokenType<TemplateToken.ParsedData>
	{
		public class ParsedData
		{
			public string m_TemplateInfo;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			if (input.StartsWith("template") && input.Contains("<") && input.Contains(">"))
			{
				string[] tokens = TokenUtils.SplitSyntax(input);

				if (tokens.First().EndsWith(">"))
				{
					// SplitSyntax may have formatted this, so have to step through until find open block
					string templateDecl = tokens.First();
					string formattedCall = new string(templateDecl.Where((c) => !char.IsWhiteSpace(c)).ToArray());

					int counter = 0;
					for (int i = 0; i < input.Length; ++i)
					{
						char c = input[i];

						if (!char.IsWhiteSpace(c))
						{
							if (c == formattedCall[counter])
							{
								if (++counter == formattedCall.Length)
								{
									string rawMacroCall = input.Substring(0, i + 1);
									input = rawMacroCall;
									data = new ParsedData { m_TemplateInfo = templateDecl };
									return true;
								}
							}
						}
					}
				}
			}
			
			data = null;
			return false;
		}
	}
}
