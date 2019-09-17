using Prism.Reflection.Elements.Cpp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class FriendToken : CppTokenType<FriendToken.ParsedData>
	{
		public class ParsedData
		{
			public TypeInfo m_DeclaredType;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			if (input.StartsWith("friend ") && input.EndsWith(";"))
			{
				string typeInfo = input.Substring("friend ".Length);
				typeInfo = typeInfo.Substring(0, typeInfo.Length - 1);

				if (VariableToken.GetTypeInfo(typeInfo, out TypeInfo info))
				{
					data = new ParsedData { m_DeclaredType = info };
					return true;
				}
			}

			data = null;
			return false;
		}
	}
}
