using Prism.Parser.Cpp.Token;
using Prism.Parsing;
using Prism.Reflection;
using Prism.Reflection.Elements;
using Prism.Reflection.Elements.Cpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp
{	
	/// <summary>
	/// Reads through a cpp file and returns internal structure data of the parsed file
	/// </summary>
	public class CppReflectionReader : IDisposable
	{
		/// <summary>
		/// Data is reflected by having macro "tokens" places next to parsed elements
		/// This structure performs this parsing and throws exceptions when invalid usage is found
		/// </summary>
		private class MacroTokenUsage
		{
			public delegate void UsageCallback(ElementOrigin origin, string[] rawParams, CppTokenInfo token);

			private string m_MacroName;
			private Type m_TargetToken;
			private bool m_ExpectMacroFirst;
			private bool m_IsBlockRead = false;
			private UsageCallback m_Callback;

			private bool m_FoundBlock = false;

			public MacroTokenUsage(string name, Type target, UsageCallback callback, bool blockRead = false)
			{
				m_MacroName = name;
				m_TargetToken = target;
				m_ExpectMacroFirst = true;
				m_Callback = callback;
				m_IsBlockRead = blockRead;
			}

			public MacroTokenUsage(Type target, string name, UsageCallback callback, bool blockRead = false)
			{
				m_MacroName = name;
				m_TargetToken = target;
				m_ExpectMacroFirst = false;
				m_Callback = callback;
				m_IsBlockRead = blockRead;
			}

			public bool Check(CppTokenInfo prev, CppTokenInfo curr)
			{
				// If trying to read block, attempt to track and break out of block + handle any sub-blocks found
				if (m_IsBlockRead && m_FoundBlock)
				{
					if (CheckMacro(curr, out MacroCallToken.ParsedData macroData))
						throw new ParseException("Unexpected placement of '" + m_MacroName + "'", curr);

					if (CheckTarget(curr))
					{
						var block = curr.m_ParsedData as CppBlockInfo;
						if (!block.m_IsBlockBegin)
						{
							m_FoundBlock = false;
						}

						m_Callback(null, new string[0], curr);
						return true;
					}
				}
				else
				{

					if (m_ExpectMacroFirst)
					{
						if (CheckMacro(prev, out MacroCallToken.ParsedData macroData))
						{
							if (CheckTarget(curr))
							{
								m_Callback(prev.ToElementOrigin(), macroData.m_Params, curr);
								m_FoundBlock = true;
								return true;
							}
							else
								throw new ParseException("Unexpected token for '" + m_MacroName + "' (Found token of type '" + curr.m_TokenType.Identifier + "'", curr);
						}
					}
					else
					{
						if (CheckMacro(curr, out MacroCallToken.ParsedData macroData))
						{
							if (CheckTarget(prev))
							{
								m_Callback(curr.ToElementOrigin(), macroData.m_Params, prev);
								m_FoundBlock = true;
								return true;
							}
							else
								throw new ParseException("Unexpected token for '" + m_MacroName + "' (Found token of type '" + prev.m_TokenType.Identifier + "'", prev);
						}
					}
				}

				return false;
			}

			private bool CheckMacro(CppTokenInfo token, out MacroCallToken.ParsedData data)
			{
				if (token.m_TokenType is MacroCallToken)
				{
					data = token.m_ParsedData as MacroCallToken.ParsedData;

					if (data.m_Name == m_MacroName)
					{
						return true;
					}
				}

				data = null;
				return false;
			}

			private bool CheckTarget(CppTokenInfo token)
			{
				if (token.m_TokenType.GetType() == m_TargetToken)
					return true;

				return false;
			}
		}

		private ReflectionParserSettings m_Settings;
		private IEnumerable<MacroTokenUsage> m_MacroUsages;
		private CppTokenReader m_Reader;
		private ContentElement m_ReflectedContent;

		// States
		private StructureElement m_CurrentStructure;
		private EnumTypeElement m_CurrentEnumType;
		private string[] m_CurrentNamespace = new string[0];
		private ConditionState m_CurrentCondition;
		private string m_CurrentComment;
		private string m_CurrentAccessor;
		private bool m_PassOnComment;


		private CppReflectionReader(ReflectionParserSettings settings)
		{
			m_Settings = settings;
			m_CurrentCondition = new ConditionState();
			m_MacroUsages = new MacroTokenUsage[]
			{
				new MacroTokenUsage(typeof(StructureToken), settings.ClassToken, OnStructureToken, true),
				new MacroTokenUsage(typeof(StructureToken), settings.StructToken, OnStructureToken, true),
				new MacroTokenUsage(typeof(EnumTypeToken), settings.EnumToken, OnEnumToken, true),
				new MacroTokenUsage(settings.ConstructorToken, typeof(ConstructorToken), OnFunctionToken),
				new MacroTokenUsage(settings.FunctionToken, typeof(FunctionToken), OnFunctionToken),
				new MacroTokenUsage(settings.VariableToken, typeof(VariableToken), OnVariableToken),
			};
		}

		public CppReflectionReader(ReflectionParserSettings settings, string filePath)
			: this(settings)
		{
			m_ReflectedContent = new ContentElement(filePath, settings);
			m_Reader = new CppTokenReader(new FileStream(filePath, FileMode.Open));
		}

		public CppReflectionReader(ReflectionParserSettings settings, Stream stream)
			: this(settings)
		{
			m_ReflectedContent = new ContentElement("UnknownStream", settings);
			m_Reader = new CppTokenReader(stream);
		}

		public void Dispose()
		{
			m_Reader.Dispose();
		}

		private bool InsideStructure
		{
			get { return m_CurrentStructure != null || m_CurrentEnumType != null; }
		}

		private ReflectionMetaData GetMetaData(CppTokenInfo curr, ElementOrigin origin = null)
		{
			var metaData = new ReflectionMetaData
			{
				m_Origin = origin == null ? curr.ToElementOrigin() : origin,
				m_Documentation = m_CurrentComment,
				m_PreProcessorCondition = m_CurrentCondition.CurrentCondition,
				m_Namespace = m_CurrentNamespace.ToArray(),
				m_DiscoveryState = ReflectionState.Discovered
			};

			m_CurrentComment = "";
			return metaData;
		}

		public bool ParseContent(out ContentElement outContent)
		{
			CppTokenInfo prev = null;
			while (m_Reader.Next(out CppTokenInfo curr))
			{
				// Look for macros placed next to specific tokens
				//
				bool foundMatch = false;
				if (prev != null)
				{
					foreach (var usage in m_MacroUsages)
					{
						if (usage.Check(prev, curr))
						{
							// Found valid usage, so no need to continue
							foundMatch = true;
							break;
						}
					}
				}

				// Perform Implicit checks
				{
					// Constructors
					if (m_Settings.UseImplicitConstructors && curr.m_TokenType is ConstructorToken)
					{
						var data = curr.m_ParsedData as ConstructorToken.ParsedData;
						if ((m_Settings.UseStrictImplicitChecks || data.m_IsValid) && data.m_FunctionInfo.m_IsConstructor)
							OnFunctionToken(null, new string[0], curr);
						foundMatch = true;
					}

					// Functions
					if (m_Settings.UseImplicitFunctions && curr.m_TokenType is FunctionToken)
					{
						var data = curr.m_ParsedData as FunctionToken.ParsedData;
						if (m_Settings.UseStrictImplicitChecks || data.m_IsValid)
							OnFunctionToken(null, new string[0], curr);
						foundMatch = true;
					}

					// Variables
					if (m_Settings.UseImplicitVariables && curr.m_TokenType is VariableToken)
					{
						var data = curr.m_ParsedData as VariableToken.ParsedData;
						if (m_Settings.UseStrictImplicitChecks || data.m_IsValid)
							OnVariableToken(null, new string[0], curr);
						foundMatch = true;
					}
				}

				// Perform other behaviour
				if (!foundMatch)
				{
					// Handle namespace update
					if (curr.m_TokenType is NamespaceToken)
					{
						var data = curr.m_ParsedData as NamespaceToken.ParsedData;
						m_CurrentNamespace = data.m_Namespace ?? new string[0];
					}

					// Handle pre-processor states
					if (curr.m_TokenType is PreProToken)
					{
						var data = curr.m_ParsedData as PreProToken.ParsedData;
						m_CurrentCondition.HandleDirective(data);
					}

					// Update accessor
					if (curr.m_TokenType is AccessorToken)
					{
						var data = curr.m_ParsedData as AccessorToken.ParsedData;
						m_CurrentAccessor = data.m_Name;
					}
					
					// Track comments, they might be dev strings
					if (curr.m_TokenType is CommentToken)
					{
						var data = curr.m_ParsedData as CommentToken.ParsedData;

						if (m_PassOnComment)
							m_CurrentComment += data.m_Content;
						else
							m_CurrentComment = data.m_Content;
					}

					// Add to comment/pass on if token is any of the following
					m_PassOnComment = false;
					if (curr.m_TokenType is CommentToken ||
						curr.m_TokenType is MacroCallToken ||
						curr.m_TokenType is TemplateToken ||
						curr.m_TokenType is EnumTypeToken ||
						curr.m_TokenType is StructureToken)
					{
						m_PassOnComment = true;
					}

					// If we find a structure embedded inside another, ignore it
					if (InsideStructure && curr.m_TokenType is StructureToken)
					{
						var data = curr.m_ParsedData as CppBlockInfo;
						if (data.m_IsBlockBegin)
						{
							string content;
							m_Reader.ReadUntilEndOfBlock(out content, m_Reader.CurrentBlockDepth);
						}
					}

					// If we find an enum value, add it to the enum
					if (curr.m_TokenType is EnumValueToken)
					{
						// If null it means we're not reflecting this enum
						if (m_CurrentEnumType != null)
						{
							var data = curr.m_ParsedData as EnumValueToken.ParsedData;
							m_CurrentEnumType.AddValue(new EnumValueElement(GetMetaData(curr), new string[0], data.m_ValueInfo));
						}
					}
				}

				// Next
				prev = curr;

				if (!m_PassOnComment)
					m_CurrentComment = "";
			}

			outContent = m_ReflectedContent;
			return true;
		}

		private void OnStructureToken(ElementOrigin origin, string[] macroParams, CppTokenInfo info)
		{
			CppBlockInfo blockInfo = info.m_ParsedData as CppBlockInfo;
			var data = blockInfo.m_ParsedData as StructureToken.ParsedData;

			if (blockInfo.m_IsBlockBegin && InsideStructure)
				throw new ParseException("Currently there is not support for reflecting internal structures", info);

			if (blockInfo.m_IsBlockBegin)
			{
				m_CurrentStructure = new StructureElement(GetMetaData(info, origin), macroParams, data.m_StructureInfo);
				m_ReflectedContent.AppendElement(m_CurrentStructure);

				if (data.m_StructureInfo.m_Structure == "class")
					m_CurrentAccessor = "private";
				else if (data.m_StructureInfo.m_Structure == "struct")
					m_CurrentAccessor = "public";
			}
			else
			{
				m_CurrentStructure = null;
				m_CurrentAccessor = null;
			}
		}

		private void OnEnumToken(ElementOrigin origin, string[] macroParams, CppTokenInfo info)
		{
			CppBlockInfo blockInfo = info.m_ParsedData as CppBlockInfo;
			var data = blockInfo.m_ParsedData as EnumTypeToken.ParsedData;

			if (blockInfo.m_IsBlockBegin && InsideStructure)
				throw new ParseException("Currently there is not support for reflecting internal structures", info);

			if (blockInfo.m_IsBlockBegin)
			{
				m_CurrentEnumType = new EnumTypeElement(GetMetaData(info, origin), macroParams, data.m_EnumInfo);
				m_ReflectedContent.AppendElement(m_CurrentEnumType);
			}
			else
			{
				m_CurrentEnumType = null;
			}
		}

		private void OnFunctionToken(ElementOrigin origin, string[] macroParams, CppTokenInfo info)
		{
			if (m_CurrentStructure == null)
				throw new ParseException("Currently there is not support for reflecting functions outside of structures", info);

			var data = info.m_ParsedData as FunctionToken.ParsedData;
			if(!data.m_IsValid)
				throw new ParseException("Syntax was found which currently isn't supported by Prism", info);

			if(m_CurrentAccessor == null)
				throw new ParseException("Accessor is ambiguious to Prism, please explicity declare it above", info);

			m_CurrentStructure.AddFunction(new FunctionElement(GetMetaData(info, origin), macroParams, data.m_FunctionInfo, m_CurrentAccessor));
		}

		private void OnVariableToken(ElementOrigin origin, string[] macroParams, CppTokenInfo info)
		{
			if (!InsideStructure)
				throw new ParseException("Currently there is not support for reflecting variables outside of structures", info);

			var data = info.m_ParsedData as VariableToken.ParsedData;
			if (!data.m_IsValid)
				throw new ParseException("Syntax was found which currently isn't supported by Prism", info);

			if (m_CurrentAccessor == null)
				throw new ParseException("Accessor is ambiguious to Prism, please explicity declare it above", info);

			m_CurrentStructure.AddVariable(new VariableElement(GetMetaData(info, origin), macroParams, data.m_VariableInfo, m_CurrentAccessor));
		}
	}
}
