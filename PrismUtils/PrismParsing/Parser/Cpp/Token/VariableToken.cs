using Prism.Reflection.Elements.Cpp.Data;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class VariableToken : CppTokenType<VariableToken.ParsedData>
	{
		public class ParsedData
		{
			public bool m_IsValid;
			public VariableInfo m_VariableInfo;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			if (input.EndsWith(";"))
			{
				string check = input.Substring(0, input.Length - 1).Replace('\t', ' ').Replace('\n', ' ');

				if (GetVariableInfo(check, out data))
					return true;
			}

			data = null;
			return false;
		}

		public static bool GetVariableInfo(string input, out ParsedData data)
		{
			VariableInfo varInfo = new VariableInfo();
			string[] valueSplit = TokenUtils.SplitSyntax(input, "=");

			if (valueSplit.Length != 0)
			{

				// Has default values (TODO : Fix handling for == in default values and for : bit masks)
				if (valueSplit.Length == 2)
				{
					varInfo.m_DefaultValue = valueSplit[1];
				}

				string varDecl = valueSplit[0].Replace("&", " & ").Replace("*", " * ").Replace("^", " ^ ");

				string[] syntax = TokenUtils.SplitSyntax(varDecl);
				if (syntax.Length >= 2)
				{
					varInfo.m_Name = syntax.Last();

					// Work out type name info
					string[] typeInfoArr = new string[syntax.Length - 1];
					Array.Copy(syntax, typeInfoArr, syntax.Length - 1);
					IEnumerable<string> typeInfo = typeInfoArr;

					// Work out variable properties
					string[] removeParts = new string[] { "static", "volatile", "mutable", "inline" };
					int staticCount = typeInfo.Where((v) => v == "static").Count();
					int volatileCount = typeInfo.Where((v) => v == "volatile").Count();
					int mutableCount = typeInfo.Where((v) => v == "mutable").Count();
					int inlineCount = typeInfo.Where((v) => v == "inline").Count();

					if (staticCount <= 1 && volatileCount <= 1 && mutableCount <= 1 && inlineCount <= 1)
					{
						varInfo.m_IsStatic = staticCount == 1;
						varInfo.m_IsVolatile = volatileCount == 1;
						varInfo.m_IsMutable = mutableCount == 1;
						varInfo.m_IsInlined = inlineCount == 1;

						typeInfo = typeInfo.Where((v) => !removeParts.Contains(v));
						if (GetTypeInfo(typeInfo, out varInfo.m_TypeInfo))
						{
							data = new ParsedData();
							data.m_IsValid = true;
							data.m_VariableInfo = varInfo;
							return true;
						}
					}

					data = new ParsedData();
					data.m_IsValid = false;
					return true;

				}
			}

			data = null;
			return false;
		}

		public static bool GetTypeInfo(string input, out TypeInfo typeInfo)
		{
			string typeDecl = input.Replace("&", " & ").Replace("*", " * ").Replace("^", " ^ ");
			string[] syntax = TokenUtils.SplitSyntax(typeDecl);
			return GetTypeInfo(syntax, out typeInfo);
		}

		public static bool GetTypeInfo(IEnumerable<string> input, out TypeInfo typeInfo)
		{
			// TODO - Support forward declaration
			string[] removes = new string[] { "const", "*", "&" };
			input = input.Select((v) => v.Trim());
			
			int constCount = input.Where((v) => v == "const").Count();
			int pointerCount = input.Where((v) => v == "*").Count();
			int refCount = input.Where((v) => v == "&").Count();
			
			var remain = input.Where((v) => !removes.Contains(v));
			
			if (remain.Count() <= 2 && constCount <= 1 && pointerCount <= 1 && refCount <= 1)
			{
				typeInfo = new TypeInfo();
				typeInfo.m_ForwardDeclaredType = null;

				// Check to see if forward declared
				if (remain.Count() == 2)
				{
					string[] forwardDeclChecks = new string[] { "class", "struct", "interface", "enum" };
					if (forwardDeclChecks.Contains(remain.First()))
					{
						typeInfo.m_ForwardDeclaredType = remain.First();
						remain = remain.Skip(1);
					}
				}

				if (remain.Count() == 1)
				{
					typeInfo.m_Name = remain.First();
					typeInfo.m_IsConst = constCount == 1;
					typeInfo.m_IsPointer = pointerCount == 1;
					typeInfo.m_IsReference = refCount == 1;
					return true;
				}
			}
			
		    typeInfo = null;
			return false;
		}
	}
}
