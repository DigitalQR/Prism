using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class TypeDefToken : CppTokenType<TypeDefToken.ParsedData>
	{
		public class ParsedData
		{
			public string m_NewType;
			public string m_SourceType;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			// TODO - Handle typename
			if (input.EndsWith(";") && (input.StartsWith("using ") || input.StartsWith("typedef ")))
			{
				string[] syntax = TokenUtils.SplitSyntax(input.Substring(0, input.Length - 1));
				data = null;

				if (syntax[0] == "using")
				{
					if (syntax[1] != "namespace" && syntax.Length >= 4 && syntax.Contains("="))
					{
						var lhs = syntax.Skip(1).TakeWhile((t) => t != "=");
						var rhs = syntax.Skip(1).Except(lhs).Skip(1);
						data = new ParsedData { m_NewType = string.Join(" ", lhs), m_SourceType = string.Join(" ", rhs) };
						return true;
					}
				}
				else
				{
					if (syntax.Length == 3)
					{
						data = new ParsedData { m_NewType = syntax[2], m_SourceType = syntax[1] };
						return true;
					}
				}

				return false;
			}

			data = null;
			return false;
		}
	}
}
