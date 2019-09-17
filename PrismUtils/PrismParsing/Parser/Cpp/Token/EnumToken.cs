using Prism.Reflection.Elements.Cpp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class EnumTypeToken : CppTokenType<EnumTypeToken.ParsedData>
	{
		public class ParsedData
		{
			public EnumTypeInfo m_EnumInfo;
		}
		
		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			string enumToken = "enum";
			
			if (input.StartsWith(enumToken) && input.EndsWith("{"))
			{
				string check = input.Substring(enumToken.Length, input.Length - enumToken.Length - 1).TrimStart();
				string[] inheritParts = check.Split(':');
				
				EnumTypeInfo enumInfo = new EnumTypeInfo();

				if (inheritParts.Length > 1)
				{
					enumInfo.m_BaseType = inheritParts[1].Trim();
				}

				string[] structParts = inheritParts[0].Split(' ');
				if (structParts.Length > 1)
				{
					enumInfo.m_ScopeStructure = structParts[0].Trim();
					enumInfo.m_Name = structParts[1].Trim();
				}
				else
				{
					enumInfo.m_Name = inheritParts[0].Trim();
				}

				data = new ParsedData { m_EnumInfo = enumInfo };
				return true;
			}

			data = null;
			return false;
		}
	}

	public class EnumValueToken : CppTokenType<EnumValueToken.ParsedData>
	{
		public class ParsedData
		{
			public EnumValueInfo m_ValueInfo;
		}
		
		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			EnumValueInfo valueInfo = new EnumValueInfo();
			
			StringBuilder builder = new StringBuilder();
			int i = 0;
			bool foundStart = false;
			bool foundName = false;
			bool foundEnd = false;

			for (; i < input.Length; ++i)
			{
				char c = input[i];

				if (!foundStart && char.IsWhiteSpace(c))
				{
					continue;
				}
				else
				{
					foundStart = true;

					if (c == '=')
					{
						// Found multiple = so probably not value
						if (foundName)
						{
							break;
						}

						foundName = true;
						valueInfo.m_Name = builder.ToString().Trim();
						builder.Clear();
					}
					else if (c == ',' || c == '}')
					{
						if (!foundName)
							valueInfo.m_Name = builder.ToString().Trim();
						else
							valueInfo.m_DefaultValue = builder.ToString().Trim();

						foundEnd = true;
						break;
					}
					else
					{
						builder.Append(c);
					}
				}
			}

			if (foundEnd && !string.IsNullOrWhiteSpace(valueInfo.m_Name))
			{
				string consumed = input.Substring(0, i + 1);
				input = consumed;
				data = new ParsedData { m_ValueInfo = valueInfo };
				return true;
			}
			
			data = null;
			return false;
		}
	}
}
