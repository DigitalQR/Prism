using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class MacroCallToken : CppTokenType<MacroCallToken.ParsedData>
	{
		public class ParsedData
		{
			public string m_Name;
			public string[] m_Params;
		}
		
		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData parsedData)
		{
			if (input.Contains("(") && input.Contains(")"))
			{
				string[] tokens = TokenUtils.SplitSyntax(input);

				if (tokens.First().EndsWith(")"))
				{
					// SplitSyntax may have formatted this, so have to step through until find open block
					string macroCall = tokens.First();
					string formattedCall = new string(macroCall.Where((c) => !char.IsWhiteSpace(c)).ToArray());

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
									parsedData = ParseData(macroCall);
									return true;
								}
							}
						}
					}
				}
			}
			
			parsedData = null;
			return false;
		}

		private ParsedData ParseData(string call)
		{
			ParsedData data = new ParsedData();

			int split = call.IndexOf('(');
			if (split != -1)
			{
				data.m_Name = call.Substring(0, split);

				string args = call.Substring(split + 1);
				args = args.Substring(0, args.Length - 1);
				data.m_Params = TokenUtils.SplitSyntax(args, ",");
			}
			else
			{
				data.m_Name = call.Trim();
				data.m_Params = new string[0];
			}

			return data;
		}
	}
}
