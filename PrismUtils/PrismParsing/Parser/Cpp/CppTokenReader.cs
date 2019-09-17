using Prism.Parser.Cpp.Token;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp
{
	public class CppTokenReader : IDisposable
	{
		private CppLineReader m_Reader;
		private string m_PreviousRaw;
		private long m_LineNumber;
		private int m_CurrentBlockDepth;
		private IEnumerable<ICppTokenType> m_OrderedTokens;

		public CppTokenReader(Stream source)
			: this(new CppLineReader(source))
		{}

		public CppTokenReader(CppLineReader reader)
		{
			m_Reader = reader;
			m_PreviousRaw = "";
			
			m_OrderedTokens = new ICppTokenType[] {
				new CommentToken(),
				new PreProToken(),
				new NamespaceToken(),
				new TemplateToken(),
				new FriendToken(),
				new TypeDefToken(),
				
				new CppBlockToken<StructureToken>(
					new AccessorToken(), 
					new ConstructorToken(),
					new FunctionToken(),
					new MacroCallToken(), // Macro check must be done after function but before variable
					new VariableToken()
				).WhenOutside(
					// Don't need other checks, but still check for macro
					new MacroCallToken()
				),
				new CppBlockToken<EnumTypeToken>(
					new EnumValueToken()
				),
			};
		}

		public int CurrentBlockDepth
		{
			get { return m_CurrentBlockDepth; }
		}

		public void Dispose()
		{
			m_Reader.Dispose();
		}
		
		public bool Next(out CppTokenInfo tokenInfo)
		{
			while (NextRaw(out string rawToken))
			{
				// Go through all the tokens find out what this token is
				foreach (ICppTokenType tokenType in m_OrderedTokens)
				{
					string input = rawToken;
					if (tokenType.Accept(this, ref input, out tokenInfo))
					{
						// If token didn't consume entire input, requeue it to be consumed
						if (input != rawToken && rawToken.StartsWith(input))
						{
							string leftovers = rawToken.Substring(input.Length).TrimStart();
							EnqueueLeftovers(input, leftovers);
						}
						
						tokenInfo.m_LineNumber = m_LineNumber;
						return true;
					}
				}

				// Failed to parse token, so ignore any following block
				if (rawToken.EndsWith("{"))
				{
					string block;
					ReadUntilEndOfBlock(out block, m_CurrentBlockDepth);
				}
			}

			tokenInfo = null;
			return false;
		}

		private bool NextRaw(out string token)
		{
			if (m_PreviousRaw.EndsWith("{"))
				++m_CurrentBlockDepth;
			else if (m_PreviousRaw.EndsWith("}"))
				--m_CurrentBlockDepth;
			
			if (m_Reader.Next(out token, out m_LineNumber))
			{
				m_PreviousRaw = token;
				return true;
			}

			return false;
		}

		private void EnqueueLeftovers(string consumed, string leftovers)
		{
			m_PreviousRaw = consumed;
			m_Reader.EnqueueLeftovers(leftovers);
		}

		/// <summary>
		/// Continually read raw tokens until the end of the block is found
		/// </summary>
		internal bool ReadUntilEndOfBlock(out string content, int targetDepth)
		{
			StringWriter writer = new StringWriter();

			while (NextRaw(out string token))
			{
				if (m_CurrentBlockDepth <= targetDepth)
				{
					content = writer.ToString();
					EnqueueLeftovers("", token);
					return true;
				}
				else
				{
					writer.Write(token);
				}
			}

			content = null;
			return false;
		}
	}
}
