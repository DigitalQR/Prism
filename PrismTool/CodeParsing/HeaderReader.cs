using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing
{
	/// <summary>
	/// A reader to read through a C++ header file signature at a time
	/// </summary>
	public class HeaderReader : IDisposable
	{
		/// <summary>
		/// Active safe reader for this header reader
		/// </summary>
		private SafeLineReader m_SafeReader;

		/// <summary>
		/// The active namespace stack
		/// </summary>
		private List<string> m_NamespaceStack;

		/// <summary>
		/// The active namespace stack
		/// </summary>
		private Stack<StructureSignature.ImplementationBeginData> m_StructureStack;

		/// <summary>
		/// The current depth into {} blocks
		/// </summary>
		private int m_BraceBlockDepth = 0;
		
		public HeaderReader(Stream inputStream)
		{
			m_SafeReader = new SafeLineReader(inputStream);
			m_NamespaceStack = new List<string>();
			m_StructureStack = new Stack<StructureSignature.ImplementationBeginData>();
		}

		public SafeLineReader Reader { get => m_SafeReader; }

		/// <summary>
		/// Safely read the next signature in the stream
		/// </summary>
		/// <returns>Returns if a signature was succesfully found (false if at end of stream)</returns>
		public bool TryReadNext(out SignatureInfo sigInfo)
		{
			bool parseResult = false;
			sigInfo = null;

			// Keep trying to parse until something valid is found to parse on
			while (!parseResult)
			{
				string content;
				sigInfo = null;

				// Read next initial line
				if (!m_SafeReader.SafeReadNext(out content))
				{
					return false;
				}

				long currentLine = m_SafeReader.LineNumber;

				// Don't try to parse, if we're in an uncaptured {} block
				if (m_BraceBlockDepth == 0)
				{
					// Intial shared-parsing
					parseResult = CommentBlockSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo)
						|| PreProcessorSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo)
						|| StructureSignature.TryParse("class", m_StructureStack, currentLine, content, m_SafeReader, out sigInfo)
						|| StructureSignature.TryParse("struct", m_StructureStack, currentLine, content, m_SafeReader, out sigInfo)
						|| StructureSignature.TryParse("enum", m_StructureStack, currentLine, content, m_SafeReader, out sigInfo)
						|| NamespaceSignature.TryParse(m_NamespaceStack, currentLine, content, m_SafeReader, out sigInfo)
						|| TypeDefSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo)
						|| FriendSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo)
						|| TemplateSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo);

					// Structure specific parsing
					// *MacroCallSignature needs to be before EnumEntry but after Constructor
					if (!parseResult && m_StructureStack.Count != 0)
					{
						string currentStructureType = m_StructureStack.Peek().StructureType;
						if (currentStructureType == "enum")
						{
							parseResult = MacroCallSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo)
								|| EnumEntrySignature.TryParse(currentLine, content, m_SafeReader, out sigInfo);
						}
						else
						{
							parseResult = AccessorSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo)
								|| ConstructorSignature.TryParse(ref m_BraceBlockDepth, m_StructureStack, currentLine, content, m_SafeReader, out sigInfo)
								|| MacroCallSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo);
						}
					}

					// Post structure specific parsing
					if (!parseResult)
					{
						parseResult = VariableSignature.TryParse(currentLine, content, m_SafeReader, out sigInfo)
						|| FunctionSignature.TryParse(ref m_BraceBlockDepth, currentLine, content, m_SafeReader, out sigInfo);
					}
				}

				// Look for uncaptured {} block or look for ; so we can process the rest of the line
				if (!parseResult)
				{
					for (int i = 0; i < content.Length; ++i)
					{
						if (content[i] == '{')
						{
							++m_BraceBlockDepth;
						}
						else if (content[i] == '}')
						{
							--m_BraceBlockDepth;
							if (m_BraceBlockDepth == 0)
							{
								m_SafeReader.LeftOverContent = content.Substring(i + 1);
								break;
							}
						}
					}
				}
			}
			
			return true;
		}

		public void Dispose()
		{
			m_SafeReader.Dispose();
		}
	}
}
