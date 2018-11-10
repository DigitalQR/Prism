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
		/// The supported structure tokens to look for
		/// </summary>
		private StructureToken[] m_SupportedStructureTokens;

		/// <summary>
		/// All the reflected tokens found in this file
		/// </summary>
		private List<TokenReflection> m_ReflectedTokens;
		

		private HeaderReflection(ReflectionSettings settings)
		{
			m_ReflectedTokens = new List<TokenReflection>();
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

		/// <summary>
		/// Fetched the desired header reflection from an input stream
		/// </summary>
		public static HeaderReflection Generate(ReflectionSettings settings, Stream content)
		{
			HeaderReflection reflection = new HeaderReflection(settings);

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
								throw new ReflectionException(ReflectionErrorCode.ParseError, currentSignature, "Prism parser found something unexpected");
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

								if (macroString.StartsWith("#ifdef"))
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
											currentStructure = StructureReflectionBase.RetrieveFromSignature(previousSignature, activeNamespace, macroCondition, (int)currentSignature.LineNumber, data.MacroParams, recentDocString);
											currentAccessScope = supportedStructure.DefaultAccess;
											reflection.m_ReflectedTokens.Add(currentStructure);

											// Reset any previous error (Originated for shared macro token)
											errorMessage = null;
											break;
										}
									}
								}

								if(errorMessage != null)
									throw new ReflectionException(ReflectionErrorCode.TokenMissuse, currentSignature, errorMessage);


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
								if (previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
								{
									var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

									if (data.MacroName == settings.VariableToken)
									{
										macroTokenClaimed = true;

										if (currentStructure != null)
											currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, (int)previousSignature.LineNumber, data.MacroParams, recentDocString);
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
										currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, (int)currentSignature.LineNumber, "", recentDocString);
								}

								break;
							}

						// Add any function to structure
						case SignatureInfo.SigType.FunctionDeclare:
							{
								if (previousSignature.SignatureType == SignatureInfo.SigType.MacroCall)
								{
									var data = (MacroCallSignature.ParseData)previousSignature.AdditionalParam;

									if (data.MacroName == settings.FunctionToken)
									{
										macroTokenClaimed = true;

										if (currentStructure != null)
											currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, (int)previousSignature.LineNumber, data.MacroParams, recentDocString);
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
										currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, (int)currentSignature.LineNumber, "", recentDocString);
								}

								break;
							}

						// Add any enum value to structure
						case SignatureInfo.SigType.EnumValueEntry:
							{
								if (currentStructure != null)
									currentStructure.AddInternalSignature(currentSignature, currentAccessScope, macroCondition, (int)currentSignature.LineNumber, "", recentDocString);

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
								throw new ReflectionException(ReflectionErrorCode.TokenMissuse, previousSignature, "Found unexpected '" + data.MacroName + "' (Expecting function definition to follow)");
							}
							else if (data.MacroName == settings.VariableToken)
							{
								throw new ReflectionException(ReflectionErrorCode.TokenMissuse, previousSignature, "Found unexpected '" + data.MacroName + "' (Expecting variable definition to follow)");
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

		private static bool ValidateStructureReflection(SignatureInfo currentSignature, SignatureInfo previousSignature, string token, string structure)
		{
			var data = (MacroCallSignature.ParseData)currentSignature.AdditionalParam;

			if (data.MacroName == token)
			{
				if (previousSignature.SignatureType == SignatureInfo.SigType.StructureImplementationBegin)
				{
					var structureData = (previousSignature != null ? (StructureSignature.ImplementationBeginData)previousSignature.AdditionalParam : null);
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
