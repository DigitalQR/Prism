﻿using Prism.Parsing.Code;
using Prism.Parsing.Code.Reader;
using Prism.Parsing.Code.Signatures;
using Prism.Parsing.Conversion;
using Prism.Reflection.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing
{
	/// <summary>
	/// Generate reflection information for a single file
	/// </summary>
	public class HeaderParser
	{
		/// <summary>
		/// Supported tokens to lookout for
		/// </summary>
		private struct ReflectableStructure
		{
			public string MacroToken;
			public string StructureType;
			public string DefaultAccess;

			public ReflectableStructure(string macroToken, string structureType, string defaultAccess)
			{
				MacroToken = macroToken;
				StructureType = structureType;
				DefaultAccess = defaultAccess;
			}
		}

		/// <summary>
		/// The settings to use when parsing the header
		/// </summary>
		private ReflectionSettings m_Settings;

		/// <summary>
		/// The supported structures to look to reflect
		/// </summary>
		private ReflectableStructure[] m_SupportedReflectableStructures;
		
		public HeaderParser(ReflectionSettings settings)
		{
			m_Settings = settings;

			m_SupportedReflectableStructures = new ReflectableStructure[] {
				new ReflectableStructure(settings.ClassToken, "class", "private"),
				new ReflectableStructure(settings.StructToken, "struct", "public"),
				new ReflectableStructure(settings.EnumToken, "enum", "")
			};
		}
				
		/// <summary>
		/// Fetched the desired header reflection from an input stream
		/// </summary>
		public FileToken Parse(string filePath, Stream content)
		{
			FileToken outputFile = new FileToken(filePath);
			
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
					IReflectableToken currentStructure = null;

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
									throw new ParseException(ParseErrorCode.ParseError, currentSignature, "Prism parser found something unexpected (InvalidParseFormat): '" + currentSignature.AdditionalParam + "'");
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

										FileToken.IncludeStatement include = new FileToken.IncludeStatement(includeFile, (int)currentSignature.LineNumber);
										outputFile.AddInternalInclude(include);
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

									foreach (var supportedStructure in m_SupportedReflectableStructures)
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
												currentStructure = StructureTokenConverter.Convert(
													previousSignature, activeNamespace, macroCondition, 
													new TokenOrigin(filePath, (int)currentSignature.LineNumber),
													data.MacroParams, recentDocString
												);

												currentAccessScope = supportedStructure.DefaultAccess;
												outputFile.AddInternalToken(currentStructure);

												// Reset any previous error (Originated for shared macro token)
												errorMessage = null;
												break;
											}
										}
									}

									if (errorMessage != null)
										throw new ParseException(ParseErrorCode.TokenMissuse, currentSignature, errorMessage);


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
										// TODO - Re-add this check, if (currentStructure.StructureType == data.StructureType && currentStructure.DeclerationName == data.DeclareName)
										{
											currentStructure = null;
										}
									}

									break;
								}

							// Add any variables to structure
							case SignatureInfo.SigType.VariableDeclare:
								{
									StructureToken structure = currentStructure as StructureToken;

									if (previousSignature != null && previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
									{
										var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

										if (data.MacroName == m_Settings.VariableToken)
										{
											macroTokenClaimed = true;
											
											if (structure != null)
											{
												VariableToken token = VariableTokenConverter.Convert(
													currentSignature, currentAccessScope, macroCondition,
													new TokenOrigin(filePath, (int)previousSignature.LineNumber),
													data.MacroParams, recentDocString
												);

												structure.AddProperty(token);
											}
											else
											{
												throw new ParseException(ParseErrorCode.ParseUnexpectedSignature, currentSignature, "Found unexpected '" + m_Settings.VariableToken + "' (Prism does not currently support reflecting outside of a type structure)");
											}
											break;
										}
									}

									if (m_Settings.UseImplicitVariables)
									{
										if (structure != null)
										{
											VariableToken token = VariableTokenConverter.Convert(
												currentSignature, currentAccessScope, macroCondition,
												new TokenOrigin(filePath, (int)currentSignature.LineNumber),
												"", recentDocString
											);

											structure.AddProperty(token);
										}
									}

									break;
								}

							// Add any function to structure
							case SignatureInfo.SigType.FunctionDeclare:
								{
									StructureToken structure = currentStructure as StructureToken;

									if (previousSignature != null && previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
									{
										var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

										if (data.MacroName == m_Settings.FunctionToken)
										{
											macroTokenClaimed = true;
											
											if (structure != null)
											{
												FunctionToken token = FunctionTokenConverter.Convert(
													currentSignature, currentAccessScope, macroCondition,
													new TokenOrigin(filePath, (int)previousSignature.LineNumber),
													data.MacroParams, recentDocString
												);

												structure.AddMethod(token);
											}
											else
											{
												throw new ParseException(ParseErrorCode.ParseUnexpectedSignature, currentSignature, "Found unexpected '" + m_Settings.FunctionToken + "' (Prism does not currently support reflecting outside of a type structure)");
											}
											break;
										}
									}

									if (m_Settings.UseImplicitFunctions)
									{
										if (structure != null)
										{
											FunctionToken token = FunctionTokenConverter.Convert(
												currentSignature, currentAccessScope, macroCondition,
												new TokenOrigin(filePath, (int)currentSignature.LineNumber),
												"", recentDocString
											);

											structure.AddMethod(token);
										}
									}

									break;
								}

							// Add any enum value to structure
							case SignatureInfo.SigType.EnumValueEntry:
								{
									EnumToken enumToken = currentStructure as EnumToken;

									if (enumToken != null)
									{
										var data = (EnumEntrySignature.EnumValueData)currentSignature.AdditionalParam;

										EnumValueToken value = new EnumValueToken(
											new TokenOrigin(filePath, (int)currentSignature.LineNumber), data.Name, 
											macroCondition.CurrentCondition, Reflection.ReflectionState.Discovered
										);

										enumToken.AddValue(value);
									}

									break;
								}

							// Add any constructor/destructor to current structure
							case SignatureInfo.SigType.StructureConstructor:
							case SignatureInfo.SigType.StructureDestructor:
								{
									StructureToken structure = currentStructure as StructureToken;
									if (structure != null)
									{
										FunctionToken token = FunctionTokenConverter.Convert(
											currentSignature, currentAccessScope, macroCondition,
											new TokenOrigin(filePath, (int)currentSignature.LineNumber),
											"", recentDocString
										);

										structure.AddMethod(token);
									}
									break;
								}
						}

						// Check for miss-use of macro tokens
						if (previousSignature != null && previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
						{
							if (!macroTokenClaimed)
							{
								var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

								if (data.MacroName == m_Settings.FunctionToken)
								{
									throw new ParseException(ParseErrorCode.TokenMissuse, previousSignature, "Found unexpected '" + data.MacroName + "' (Expecting function definition to follow. Got " + currentSignature.SignatureType + " )");
								}
								else if (data.MacroName == m_Settings.VariableToken)
								{
									throw new ParseException(ParseErrorCode.TokenMissuse, previousSignature, "Found unexpected '" + data.MacroName + "' (Expecting variable definition to follow. Got " + currentSignature.SignatureType + " )");
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

					return outputFile;
				}
			}
#if !DEBUG
			catch (Exception e)
			{
				if (m_Settings.IgnoreBadForm)
				{
					// Look for export include
					bool foundReflInclude = false;
					string includeExportPath = Path.GetFileNameWithoutExtension(filePath) + m_Settings.ExportExtension + Path.GetExtension(filePath);
					string requiredPreInclude = includeExportPath.ToLower().Replace('/', '\\');

					foreach (var fileInclude in outputFile.Includes)
					{
						string path = fileInclude.IncludePath.Replace('/', '\\');
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
						return new FileToken(filePath);
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
					throw new ParseException(ParseErrorCode.TokenMissuse, currentSignature, "Found unexpected '" + token + "' (Must be first line of " + structure + " body)");
				}
			}

			return false;
		}
	}
}