﻿using Prism.CodeParsing.Signatures;
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
		/// The current depth into {} blocks
		/// </summary>
		private int m_BraceBlockDepth = 0;
		
		public HeaderReader(Stream inputStream)
		{
			m_SafeReader = new SafeLineReader(inputStream);
			m_NamespaceStack = new List<string>();
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
				long firstLine = m_SafeReader.LineNumber;
				string content;
				sigInfo = null;

				// Read next initial line
				if (!m_SafeReader.SafeReadNext(out content))
				{
					return false;
				}

				// Don't try to parse, if we're in an uncaptured {} block
				if (m_BraceBlockDepth == 0)
				{
					// Try to figure out what is currently being read
					parseResult = CommentBlockSignature.TryParse(firstLine, content, m_SafeReader, out sigInfo)
						|| PreProcessorSignature.TryParse(firstLine, content, m_SafeReader, out sigInfo)
						|| StructureSignature.TryParse("class", firstLine, content, m_SafeReader, out sigInfo)
						|| StructureSignature.TryParse("struct", firstLine, content, m_SafeReader, out sigInfo)
						|| MacroCallSignature.TryParse(firstLine, content, m_SafeReader, out sigInfo)
						|| NamespaceSignature.TryParse(m_NamespaceStack, firstLine, content, m_SafeReader, out sigInfo);
				}

				// Look for uncaptured {} block
				if (!parseResult)
				{
					for (int i = 0; i < content.Length; ++i)
					{
						if (content[i] == '{')
						{
							++m_BraceBlockDepth;
							m_SafeReader.LeftOverContent = content.Substring(i + 1);
							break;
						}
						else if (content[i] == '}')
						{
							--m_BraceBlockDepth;
							m_SafeReader.LeftOverContent = content.Substring(i + 1);
							break;
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
