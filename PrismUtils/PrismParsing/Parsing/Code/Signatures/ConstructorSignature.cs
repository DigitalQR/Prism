using Prism.Parsing.Code.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Code.Signatures
{
	public class ConstructorInfo
	{
		public string DeclareName;
		public bool IsExplicit;
		public bool IsInline;
		public VariableInfo[] ParamTypes;
		public int ParamCount;

		/// <summary>
		/// Counter used to prevent constructor names overlapping
		/// </summary>
		private static int m_ConstructorIdCounter;

		public FunctionInfo ToFunctionInfo()
		{
			FunctionInfo function = new FunctionInfo();
			function.FunctionName = "Constructor_" + ParamCount + "_" + m_ConstructorIdCounter++;

			function.ReturnType = new FullTypeInfo();
			function.ReturnType.InnerType = new TypeNameInfo();
			function.ReturnType.InnerType.IsConst = false;
			function.ReturnType.InnerType.TypeName = DeclareName;
			function.ReturnType.PointerCount = 1;
			function.ReturnType.PointerData = new PointerInfo[1] { new PointerInfo() };
			function.ReturnType.PointerData[0].IsConst = false;

			function.IsStatic = true;
			function.IsInlined = IsInline;
			function.ParamCount = ParamCount;
			function.ParamTypes = ParamTypes;
			return function;
		}
	}

	public class DestructorInfo
	{
		public string DeclareName;
		public bool IsInline;
		public bool IsVirtual;
	}

	public class ConstructorSignature
	{
		public static bool TryParse(ref int braceBlockDepth, Stack<StructureSignature.ImplementationBeginData> structureStack, long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			if (structureStack.Count != 0)
			{
				string structureName = structureStack.Last().DeclareName;
				if (content.Contains(structureName))
				{
					string search = content;
					bool hasPresetBody = search.EndsWith("{");

					if(hasPresetBody || search.EndsWith(";"))
						search = search.Substring(0, search.Length - 1); // Remove ';' or '{'

					bool IsInline = false;
					bool isExplicit = false;
					bool isVirtual = false;

					if (search.StartsWith("inline "))
					{
						IsInline = true;
						search = search.Substring("inline ".Length).Trim();
					}

					if (search.StartsWith("virtual "))
					{
						isVirtual = true;
						search = search.Substring("virtual ".Length).Trim();
					}

					if (search.StartsWith("explicit "))
					{
						isExplicit = true;
						search = search.Substring("explicit ".Length).Trim();
					}

					// Is destructor
					if (search.StartsWith("~" + structureName))
					{
						DestructorInfo data = new DestructorInfo();
						data.DeclareName = structureName;
						data.IsInline = IsInline;
						data.IsVirtual = isVirtual;

						// Return leftovers
						reader.LeftOverContent = search.Substring(search.IndexOf(')') + 1);
						sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.StructureDestructor, data);
						return true;
					}

					// Is (Maybe) constructor
					if (search.StartsWith(structureName))
					{
						search = search.Substring(structureName.Length).Trim();

						if (search.StartsWith("("))
						{
							ConstructorInfo data = new ConstructorInfo();
							data.DeclareName = structureName;
							data.IsInline = IsInline;
							data.IsExplicit = isExplicit;

							// Find out the param info
							search = search.Substring(1, search.LastIndexOf(')') - 1);
							List<VariableInfo> funcParams = new List<VariableInfo>();

							// Context aware split i.e. don't split, if comma is in () or <> etc.
							string current = "";
							int blockDepth = 0;

							if (search != "")
							{
								for (int i = 0; i < search.Length; ++i)
								{
									char c = search[i];
									if (c == ',' && blockDepth == 0)
									{
										current = current.Trim();
										SignatureInfo tempSig;

										// Invalid format
										if (current == "")
										{
											sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Found empty param when attempting to parse a constructor.");
											return true;
										}

										// Parse the type info
										if (!VariableSignature.TryParse(firstLine, current + ";", reader, out tempSig))
										{
											sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Failed to parse constructor param info.");
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
										if (blockDepth == -1)
										{
											if (i != search.Length - 1)
												search = search.Substring(i + 1);
											break;
										}
									}

									current += c;

									if (i == search.Length - 1)
									{
										search = "";
									}
								}

								// Parse last param
								{

									current = current.Trim();
									SignatureInfo tempSig;

									// Invalid format
									if (current == "")
									{
										sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Found empty param when attempting to parse a constructor.");
										return true;
									}

									// Parse the type info
									if (!VariableSignature.TryParse(firstLine, current + ";", reader, out tempSig))
									{
										sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Failed to parse constructor param info.");
										return true;
									}

									var varData = (VariableInfo)tempSig.AdditionalParam;
									funcParams.Add(varData);
								}
							}

							// Return leftovers
							reader.LeftOverContent = search;

							data.ParamCount = funcParams.Count;
							if (data.ParamCount != 0)
								data.ParamTypes = funcParams.ToArray();

							sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.StructureConstructor, data);
							return true;
						}
					}

					SignatureInfo funcSig;
					if (FunctionSignature.TryParse(ref braceBlockDepth, firstLine, search, reader, out funcSig))
					{
						sigInfo = null;
						return false;
					}
				}
			}
			
			sigInfo = null;
			return false;
		}
	}
}
