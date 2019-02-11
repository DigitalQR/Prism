using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	public class FunctionInfo
	{
		public string FunctionName;

		public FullTypeInfo ReturnType;
		public VariableInfo[] ParamTypes;
		public int ParamCount;

		public bool IsConst;
		public bool IsVirtual;
		public bool IsStatic;
		public bool IsInlined;
		public bool IsAbstract;

		public string SafeFunctionName
		{
			get
			{
				return FunctionName.Replace("+", "Add")
				.Replace("-", "Sub")
				.Replace("*", "Mult")
				.Replace("/", "Div")
				.Replace("%", "Perc")
				.Replace("^", "Hat")
				.Replace("&", "Amp")
				.Replace("|", "Pipe")
				.Replace("~", "Inv")
				.Replace("!", "Not")
				.Replace(",", "Com")
				.Replace("=", "Equ")
				.Replace("<", "Less")
				.Replace(">", "Great")
				.Replace("(", "BrSrt")
				.Replace(")", "BrEnd")
				.Replace("[", "SqSrt")
				.Replace("]", "SqEnd");
			}
		}
	}

	public class FunctionSignature
	{
		public static bool TryParse(ref int braceBlockDepth, long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			// Try to check if we are currently looking at a variable
			bool hasPresetBody = content.EndsWith("{");

			string searchString = content;
			if (content.EndsWith(";") || hasPresetBody)
			{
				searchString = content.Substring(0, content.Length - 1);
			}

			if (searchString != "")
			{
				searchString = searchString.Replace("override", "").Trim(); // Ignore override completely

				bool isConst = false;
				bool isAbstract = false;

				// Check if abstract
				if (searchString.EndsWith("0"))
				{
					string subString = searchString.Substring(0, searchString.Length - 1).Trim();
					if (subString.EndsWith("="))
					{
						isAbstract = true;
						searchString = subString.Substring(0, subString.Length - 1).Trim();
					}
					// Is not a valid function
					else
					{
						sigInfo = null;
						return false;
					}
				}

				// Check if const
				if (searchString.EndsWith("const"))
				{
					isConst = true;
					searchString = searchString.Substring(0, searchString.Length - "const".Length).Trim();
				}

				// Try to detemine function details
				if (!searchString.StartsWith(",") && (searchString.StartsWith("::") ? true : !searchString.StartsWith(":")) && searchString.EndsWith(")"))
				{
					var data = new FunctionInfo();
					data.IsConst = isConst;
					data.IsAbstract = isAbstract;

					if (searchString.Contains("static"))
					{
						data.IsStatic = true;
						searchString = searchString.Replace("static", "").Trim();
					}
					if (searchString.Contains("inline"))
					{
						data.IsInlined = true;
						searchString = searchString.Replace("inline", "").Trim();
					}
					if (searchString.Contains("virtual"))
					{
						data.IsVirtual = true;
						searchString = searchString.Replace("virtual", "").Trim();
					}

					// Split into function params and function def
					int splitIndex = searchString.IndexOf('(');

					string functionalInfo = searchString.Substring(0, splitIndex).Trim();
					string paramInfo = searchString.Substring(splitIndex).Trim();
					paramInfo = paramInfo.Substring(1, paramInfo.Length - 2).Trim();

					// Parse function name and return type
					{
						// Cheat by treating it as a variable
						SignatureInfo tempSig;
						if (!VariableSignature.TryParse(firstLine, functionalInfo + ";", reader, out tempSig))
						{
							sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Initial function data was not in expected format.");
							return true;
						}

						var varData = (VariableInfo)tempSig.AdditionalParam;
						data.ReturnType = varData.TypeInfo;
						data.FunctionName = varData.VariableName;

						// Is void, so no return type
						if (data.ReturnType.InnerType.TypeName == "void" && data.ReturnType.PointerCount == 0)
						{
							data.ReturnType = null;
						}
					}

					// Parse param info
					if (paramInfo != "" && paramInfo != "void")
					{
						List<VariableInfo> funcParams = new List<VariableInfo>();

						// Context aware split i.e. don't split, if comma is in () or <> etc.
						string current = "";
						int blockDepth = 0;

						foreach (char c in paramInfo)
						{
							if (c == ',' && blockDepth == 0)
							{
								current = current.Trim();
								SignatureInfo tempSig;

								// Invalid format
								if (current == "")
								{
									sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Found empty param when attempting to parse a function.");
									return true;
								}

								// Parse the type info
								if (!VariableSignature.TryParse(firstLine, current + ";", reader, out tempSig))
								{
									sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Failed to parse function param info.");
									return true;
								}

								var varData = (VariableInfo)tempSig.AdditionalParam;
								funcParams.Add(varData);

								current = "";
								continue;
							}
							else if (c == '<' || c == '(' || c == '[' || c == '{')
							{
								++blockDepth;
							}
							else if (c == '>' || c == ')' || c == ']' || c == '}')
							{
								--blockDepth;
							}

							current += c;
						}

						// Parse last param
						{

							current = current.Trim();
							SignatureInfo tempSig;

							// Invalid format
							if (current == "")
							{
								sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Found empty param when attempting to parse a function.");
								return true;
							}

							// Parse the type info
							if (!VariableSignature.TryParse(firstLine, current + ";", reader, out tempSig))
							{
								sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Failed to parse function param info.");
								return true;
							}

							var varData = (VariableInfo)tempSig.AdditionalParam;
							funcParams.Add(varData);
						}


						data.ParamTypes = funcParams.ToArray();
						data.ParamCount = funcParams.Count;
					}

					// Make sure the function body is ignored
					if (hasPresetBody)
					{
						++braceBlockDepth;
					}

					sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.FunctionDeclare, data);
					return true;
				}
			}

			sigInfo = null;
			return false;
		}
	}
}
