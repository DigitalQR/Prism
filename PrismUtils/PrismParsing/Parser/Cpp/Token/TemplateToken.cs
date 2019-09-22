using Prism.Reflection.Elements.Cpp.Data;
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
			public bool m_IsValid;
			public TemplateInfo m_TemplateInfo;
		}
		
		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData parsedData)
		{
			if (input.StartsWith("template") && input.Contains("<") && input.Contains(">"))
			{
				string[] tokens = TokenUtils.SplitSyntax(input);

				if (tokens.First().EndsWith(">"))
				{
					// SplitSyntax may have formatted this, so have to step through until find open block
					string templateCall = tokens.First();
					string formattedCall = new string(templateCall.Where((c) => !char.IsWhiteSpace(c)).ToArray());

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
									parsedData = ParseData(templateCall);
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
			string rawParams = call.Substring("template<".Length);
			rawParams = rawParams.Substring(0, rawParams.Length - 1);

			string[] templateParams = TokenUtils.SplitSyntax(rawParams, ",");

			ParsedData data = new ParsedData();
			data.m_TemplateInfo = new TemplateInfo();
			data.m_TemplateInfo.m_Params = new TemplateParamInfo[templateParams.Length];

			for (int i = 0; i < templateParams.Length; ++i)
			{
				string[] paramParts = TokenUtils.SplitSyntax(templateParams[i], "=");
				string[] lhs = TokenUtils.SplitSyntax(paramParts[0]);
				string[] rhs = null;

				if (paramParts.Length > 1)
					rhs = TokenUtils.SplitSyntax(paramParts[1]);

				// Currently only support something in format <typename> <T> ?= ...
				if (lhs.Length != 2)
				{
					data.m_IsValid = false;
					data.m_TemplateInfo = null;
					return data;
				}

				data.m_TemplateInfo.m_Params[i] = new TemplateParamInfo
				{
					m_Prefix = lhs[0],
					m_Name = lhs[1],
					m_DefaultValue = rhs != null ? rhs[0] : null
				};
			}

			data.m_IsValid = true;
			return data;
		}
	}
}
