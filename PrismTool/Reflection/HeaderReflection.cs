using Prism.CodeParsing;
using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	/// <summary>
	/// Generate reflection information for a single file
	/// </summary>
	public class HeaderReflection
	{
		/// <summary>
		/// Supported tokens to lookout for
		/// </summary>
		private struct StructureToken
		{
			public string MacroToken;
			public string StructureType;
			public string DefaultAccess;

			public StructureToken(string macroToken, string structureType, string defaultAccess)
			{
				MacroToken = macroToken;
				StructureType = structureType;
				DefaultAccess = defaultAccess;
			}
		}

		/// <summary>
		/// When an include is found
		/// </summary>
		public class IncludeFile
		{
			public string Path;
			public int LineNumber;
		}

		/// <summary>
		/// The supported structure tokens to look for
		/// </summary>
		private StructureToken[] m_SupportedStructureTokens;

		/// <summary>
		/// All the reflected tokens found in this file
		/// </summary>
		private List<TokenReflection> m_ReflectedTokens;

		/// <summary>
		/// All the includes found in this file
		/// </summary>
		private List<IncludeFile> m_Includes;
		

		private HeaderReflection(ReflectionSettings settings)
		{
			m_ReflectedTokens = new List<TokenReflection>();
			m_Includes = new List<IncludeFile>();
			m_SupportedStructureTokens = new StructureToken[] {
				new StructureToken(settings.ClassToken, "class", "private"),
				new StructureToken(settings.StructToken, "struct", "public"),
				new StructureToken(settings.EnumToken, "enum", "")
			};
		}

		public TokenReflection[] ReflectedTokens
		{
			get { return m_ReflectedTokens.ToArray(); }
		}

		public int ReflectedTokenCount
		{
			get { return m_ReflectedTokens.Count; }
		}

		public IncludeFile[] FileIncludes
		{
			get { return m_Includes.ToArray(); }
		}

		public int FileIncludeCount
		{
			get { return m_Includes.Count; }
		}

		/// <summary>
		/// Fetched the desired header reflection from an input stream
		/// </summary>
		public static HeaderReflection Generate(ReflectionSettings settings, string filePath, Stream content)
		{
			HeaderReflection reflection = new HeaderReflection(settings);

#if !DEBUG
			try
#endif
			{
				using (HeaderReader reader = new HeaderReader(content))
				{
					SignatureInfo previousSignature = null;
					SignatureInfo currentSignature;

					// Setup file states
					string recentDocString = "";
					bool reuseDocString = false;

					string[] activeNamespace = null;
					string currentAccessScope = "private";

					ConditionState macroCondition = new ConditionState();
					StructureReflectionBase currentStructure = null;

					while (reader.TryReadNext(out currentSignature))
					{
						bool macroTokenClaimed = false;

						/*
							Unknown, 
							InvalidParseFormat,

							UsingNamespace,

							StructureForwardDeclare,

							FriendDeclare,
							TypeDefDeclare,
							TemplateDeclare,
						 */

						// Keep track of most recent comment (Do not store this as last signature)
						if (currentSignature.SignatureType == SignatureInfo.SigType.CommentBlock)
						{
							recentDocString = currentSignature.LineContent;
							continue;
						}

						switch (currentSignature.SignatureType)
						{
							// Handle errors in parsing
							case SignatureInfo.SigType.InvalidParseFormat:
								{
									throw new ReflectionException(ReflectionErrorCode.ParseError, currentSignature, "Prism parser found something unexpected (InvalidParseFormat): '" + currentSignature.AdditionalParam + "'");
								}

							// Keep track of current namespace
							case SignatureInfo.SigType.NamespaceBegin:
							case SignatureInfo.SigType.NamespaceEnd:
								{
									var sigData = (NamespaceSignature.ActiveData)currentSignature.AdditionalParam;
									activeNamespace = sigData.CurrentNamespace;
									break;
								}
							// TODO - Some handling for 'using namespace'

							// Keep track of current pre-processor condition
							case SignatureInfo.SigType.PreProcessorDirective:
								{
									string macroString = currentSignature.LineContent;

									if (macroString.StartsWith("#include"))
									{
										string includeFile = macroString.Substring("#include".Length).Trim();
										// Remove <> or ""
										includeFile = includeFile.Substring(1, includeFile.Length - 2);

										IncludeFile include = new IncludeFile();
										include.Path = includeFile;
										include.LineNumber = (int)currentSignature.LineNumber;

										reflection.m_Includes.Add(include);
									}

									else if (macroString.StartsWith("#ifdef"))
									{
										string condition = macroString.Substring("#ifdef".Length).Trim();
										macroCondition.AddIF("defined(" + condition + ")");
									}
									else if (macroString.StartsWith("#if"))
									{
										string condition = macroString.Substring("#if".Length).Trim();
										macroCondition.AddIF(condition);

									}
									else if (macroString.StartsWith("#elif"))
									{
										string condition = macroString.Substring("#elif".Length).Trim();
										macroCondition.AddELSEIF(condition);
									}
									else if (macroString.StartsWith("#else"))
									{
										macroCondition.AddELSE();
									}
									else if (macroString.StartsWith("#endif"))
									{
										macroCondition.EndIF();
									}

									string a = macroCondition.CurrentCondition;
									break;
								}

							case SignatureInfo.SigType.AccessorSet:
								{
									var data = (AccessorSignature.ParseData)currentSignature.AdditionalParam;
									currentAccessScope = data.Accessor;
									break;
								}

							// Keep an eye out for macro calls of relevence
							case SignatureInfo.SigType.MacroCall:
								{
									var data = (MacroCallSignature.ParseData)currentSignature.AdditionalParam;

									// Look for any structures which are supported
									string errorMessage = null;

									foreach (var supportedStructure in reflection.m_SupportedStructureTokens)
									{
										if (data.MacroName == supportedStructure.MacroToken && ValidateStructureReflection(currentSignature, previousSignature, supportedStructure.MacroToken, supportedStructure.StructureType))
										{
											if (currentStructure != null)
											{
												errorMessage = "Found unexpected '" + supportedStructure.MacroToken + "' (Prism does not currently support reflecting embedded '" + supportedStructure.StructureType + "' structures)";
											}
											else
											{
												// Create new structure for this signature
												currentStructure = StructureReflectionBase.RetrieveFromSignature(previousSignature, activeNamespace, macroCondition, filePath, (int)currentSignature.LineNumber, data.MacroParams, recentDocString);
												currentAccessScope = supportedStructure.DefaultAccess;
												reflection.m_ReflectedTokens.Add(currentStructure);

												// Reset any previous error (Originated for shared macro token)
												errorMessage = null;
												break;
											}
										}
									}

									if (errorMessage != null)
										throw new ReflectionException(ReflectionErrorCode.TokenMissuse, currentSignature, errorMessage);


									reuseDocString = true;
									break;
								}

							// Keep an eye out for the currentStructure ending
							case SignatureInfo.SigType.StructureImplementationBegin:
								{
									// Wait for macro to appear before reflecting
									reuseDocString = true;
									break;
								}

							// Keep an eye out for the currentStructure ending
							case SignatureInfo.SigType.StructureImplementationEnd:
								{
									var data = (StructureSignature.ImplementationEndData)currentSignature.AdditionalParam;
									if (currentStructure != null)
									{
										// Found end of the current structure
										if (currentStructure.StructureType == data.StructureType && currentStructure.DeclerationName == data.DeclareName)
										{
											currentStructure = null;
										}
									}

									break;
								}

							// Add any variables to structure
							case SignatureInfo.SigType.VariableDeclare:
								{
									if (previousSignature != null && previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
									{
										var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

										if (data.MacroName == settings.VariableToken)
										{
											macroTokenClaimed = true;

											if (currentStructure != null)
												currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, filePath, (int)previousSignature.LineNumber, data.MacroParams, recentDocString);
											else
											{
												throw new ReflectionException(ReflectionErrorCode.ParseUnexpectedSignature, currentSignature, "Found unexpected '" + settings.VariableToken + "' (Prism does not currently support reflecting outside of a type structure)");
											}
											break;
										}
									}

									if (settings.UseImplicitVariables)
									{
										if (currentStructure != null)
											currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, filePath, (int)currentSignature.LineNumber, "", recentDocString);
									}

									break;
								}

							// Add any function to structure
							case SignatureInfo.SigType.FunctionDeclare:
								{
									if (previousSignature != null && previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
									{
										var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

										if (data.MacroName == settings.FunctionToken)
										{
											macroTokenClaimed = true;

											if (currentStructure != null)
												currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, filePath, (int)previousSignature.LineNumber, data.MacroParams, recentDocString);
											else
											{
												throw new ReflectionException(ReflectionErrorCode.ParseUnexpectedSignature, currentSignature, "Found unexpected '" + settings.FunctionToken + "' (Prism does not currently support reflecting outside of a type structure)");
											}
											break;
										}
									}

									if (settings.UseImplicitFunctions)
									{
										if (currentStructure != null)
											currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, filePath, (int)currentSignature.LineNumber, "", recentDocString);
									}

									break;
								}

							// Add any enum value to structure
							case SignatureInfo.SigType.EnumValueEntry:
								{
									if (currentStructure != null)
										currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, filePath, (int)currentSignature.LineNumber, "", recentDocString);

									break;
								}

							// Add any constructor to current structure
							case SignatureInfo.SigType.StructureConstructor:
								{
									if (currentStructure != null)
										currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, filePath, (int)currentSignature.LineNumber, "", recentDocString);
									break;
								}

							// Add any destructor to current structure
							case SignatureInfo.SigType.StructureDestructor:
								{
									if (currentStructure != null)
										currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, filePath, (int)currentSignature.LineNumber, "", recentDocString);
									break;
								}

						}

						// Check for miss-use of macro tokens
						if (previousSignature != null && previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
						{
							if (!macroTokenClaimed)
							{
								var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

								if (data.MacroName == settings.FunctionToken)
								{
									throw new ReflectionException(ReflectionErrorCode.TokenMissuse, previousSignature, "Found unexpected '" + data.MacroName + "' (Expecting function definition to follow. Got " + currentSignature.SignatureType + " )");
								}
								else if (data.MacroName == settings.VariableToken)
								{
									throw new ReflectionException(ReflectionErrorCode.TokenMissuse, previousSignature, "Found unexpected '" + data.MacroName + "' (Expecting variable definition to follow. Got " + currentSignature.SignatureType + " )");
								}
							}
						}

						// Should keep same doc-string between statements
						if (reuseDocString)
							reuseDocString = false;
						else
							recentDocString = "";

						// Refresh previous signature
						previousSignature = currentSignature;
					}

					return reflection;
				}
			}
#if !DEBUG
			catch (Exception e)
			{
				if (settings.IgnoreBadForm)
				{
					// Look for export include
					bool foundReflInclude = false;
					string includeExportPath = Path.GetFileNameWithoutExtension(filePath) + settings.ExportExtension + Path.GetExtension(filePath);
					string requiredPreInclude = includeExportPath.ToLower().Replace('/', '\\');

					foreach (var fileInclude in reflection.FileIncludes)
					{
						string path = fileInclude.Path.Replace('/', '\\');
						if (path.EndsWith(requiredPreInclude, StringComparison.CurrentCultureIgnoreCase))
						{
							foundReflInclude = true;
							break;
						}
					}

					// Ignore bad parsing, if expected include is not present
					if (foundReflInclude)
						throw e;
					else
						// Return empty reflection
						return new HeaderReflection(settings);
				}
				else
					throw e;
			}
#endif
		}

		private static bool ValidateStructureReflection(SignatureInfo currentSignature, SignatureInfo previousSignature, string token, string structure)
		{
			var data = (MacroCallSignature.ParseData)currentSignature.AdditionalParam;

			if (data.MacroName == token)
			{
				if (previousSignature != null && previousSignature.SignatureType == SignatureInfo.SigType.StructureImplementationBegin)
				{
					var structureData = (StructureSignature.ImplementationBeginData)previousSignature.AdditionalParam;
					if (structureData.StructureType == structure)
					{
						return true;
					}
				}
				else
				{
					throw new ReflectionException(ReflectionErrorCode.TokenMissuse, currentSignature, "Found unexpected '" + token + "' (Must be first line of " + structure + " body)");
				}
			}

			return false;
		}
	}
}
