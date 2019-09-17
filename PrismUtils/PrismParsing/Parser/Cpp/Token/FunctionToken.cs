using Prism.Reflection.Elements.Cpp.Data;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class FunctionToken : CppTokenType<FunctionToken.ParsedData>
	{
		public class ParsedData
		{
			public bool m_IsValid;
			public FunctionInfo m_FunctionInfo;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			// TODO - Look at approach for operator overloading <> cause issues
			if (input.Contains("(") && input.Contains(")"))
			{
				string check = input.Substring(0, input.Length - 1).Replace('\t', ' ').Replace('\n', ' ');

				if (GetFunctionInfo(check, out data))
				{
					if (input.EndsWith("{"))
					{
						string funcBody;
						bool result = reader.ReadUntilEndOfBlock(out funcBody, reader.CurrentBlockDepth);
						return result;
					}
					return true;
				}
			}

			data = null;
			return false;
		}

		public bool GetFunctionInfo(string input, out ParsedData data)
		{
			string[] valueSplit = TokenUtils.SplitSyntax(input);

			// Check that at least 1 token (thats not the first) ends with a )
			if (IsValidFunctionLayout(valueSplit))
			{
				IEnumerable<string> preParts;
				IEnumerable<string> postParts;
				string methodPart;

				if (BreakoutLayout(valueSplit, out preParts, out methodPart, out postParts))
				{
					return ProcessMethodInfo(preParts, methodPart, postParts, out data);
				}
			}

			data = null;
			return false;
		}

		protected virtual bool IsValidFunctionLayout(IEnumerable<string> tokens)
		{
			// Check that at least 1 token (thats not the first) ends with a )
			if (tokens.Count() >= 2)
			{
				// Check first token doesn't end )
				if(!tokens.First().EndsWith(")"))
					return tokens.Skip(1).Where((v) => v.EndsWith(")")).Any();
			}

			return false;
		}

		protected virtual bool BreakoutLayout(IEnumerable<string> source, out IEnumerable<string> pre, out string method, out IEnumerable<string> post)
		{
			source = source.Select((v) => v.Trim());
			pre = source.TakeWhile((v) => !v.EndsWith(")"));
			post = source.Except(pre);
			method = post.First();
			post = post.Skip(1);

			return pre.Any();
		}

		protected virtual FunctionInfo NewFunctionInfo(IEnumerable<string> pre, string method, IEnumerable<string> post)
		{
			FunctionInfo funcInfo = new FunctionInfo();
			funcInfo.m_IsConstructor = false;
			funcInfo.m_IsDestructor = false;
			funcInfo.m_IsConst = post.Contains("const");
			funcInfo.m_IsInlined = pre.Contains("inline");
			funcInfo.m_IsVirtual = pre.Contains("virtual");
			funcInfo.m_IsExplicit = pre.Contains("explicit");
			funcInfo.m_IsStatic = pre.Contains("static");
			return funcInfo;
		}

		protected bool ProcessMethodInfo(IEnumerable<string> pre, string method, IEnumerable<string> post, out ParsedData data)
		{
			string[] removeParts = new string[] { "virtual", "override", "explicit", "inline", "static" };

			FunctionInfo funcInfo = NewFunctionInfo(pre, method, post);

			if ((funcInfo.m_IsConstructor || funcInfo.m_IsDestructor) || VariableToken.GetTypeInfo(pre.Where((v) => !removeParts.Contains(v)), out funcInfo.m_ReturnInfo))
			{
				// Process method
				int splitIndex = method.IndexOf('(');
				if (splitIndex != -1)
				{
					funcInfo.m_Name = method.Substring(0, splitIndex).Trim();
					string paramsString = method.Substring(splitIndex + 1).Trim();
					paramsString = paramsString.Substring(0, paramsString.Length - 1); // Remove )

					List<VariableInfo> paramInfo = new List<VariableInfo>();
					bool paramsValid = true;

					if (!string.IsNullOrWhiteSpace(paramsString))
					{
						string[] paramsSplit = TokenUtils.SplitSyntax(paramsString, ",");
						foreach (string info in paramsSplit)
						{
							if (VariableToken.GetVariableInfo(info, out VariableToken.ParsedData paramData) && paramData.m_IsValid)
							{
								paramInfo.Add(paramData.m_VariableInfo);
							}
							else
							{
								paramsValid = false;
								break;
							}
						}
					}

					if (paramsValid)
					{
						funcInfo.m_Params = paramInfo.ToArray();

						data = new ParsedData
						{
							m_IsValid = true,
							m_FunctionInfo = funcInfo
						};
						return true;
					}
				}
			}

			data = new ParsedData
			{
				m_IsValid = false,
				m_FunctionInfo = null
			};
			return true;
		}
	}

	public class ConstructorToken : FunctionToken
	{
		private string m_ActiveStructure;

		public override bool Accept(CppTokenReader reader, ref string input, out CppTokenInfo tokenInfo)
		{
			CppBlockToken<StructureToken> structureBlock = Parent as CppBlockToken<StructureToken>;
			if (structureBlock != null && structureBlock.ActiveBlocks.Any())
			{
				var activeStruct = structureBlock.ActiveBlocks.First().Item2 as StructureToken.ParsedData;
				m_ActiveStructure = activeStruct.m_StructureInfo.m_Name;
			}

			if (!string.IsNullOrWhiteSpace(m_ActiveStructure))
			{
				bool result = base.Accept(reader, ref input, out tokenInfo);
				m_ActiveStructure = null;
				return result;
			}

			tokenInfo = null;
			return false;
		}

		protected override bool IsValidFunctionLayout(IEnumerable<string> tokens)
		{
			// Check that at least 1 token (thats not the first) ends with a )
			if (tokens.Count() >= 1)
			{
				// Check a token ends and starts with the structure name )
				if (tokens.Where((v) => v.EndsWith(")")).Any())
				{
					var endWithBracket = tokens.Where((v) => v.EndsWith(")"));

					if (endWithBracket.Count() == 1)
						return endWithBracket.Where((v) => v.StartsWith(m_ActiveStructure) || v.StartsWith("~" + m_ActiveStructure)).Any();
				}
			}

			return false;
		}

		protected override bool BreakoutLayout(IEnumerable<string> source, out IEnumerable<string> pre, out string method, out IEnumerable<string> post)
		{
			base.BreakoutLayout(source, out pre, out method, out post);
			return true;
		}

		protected override FunctionInfo NewFunctionInfo(IEnumerable<string> pre, string method, IEnumerable<string> post)
		{
			FunctionInfo info = base.NewFunctionInfo(pre, method, post);

			if (method.StartsWith(m_ActiveStructure + "("))
				info.m_IsConstructor = true;
			else if (method.StartsWith("~" + m_ActiveStructure + "("))
				info.m_IsDestructor = true;

			return info;
		}
	}
}
