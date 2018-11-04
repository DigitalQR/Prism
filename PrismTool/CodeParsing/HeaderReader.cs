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
		/// The current depth into {} blocks
		/// </summary>
		private int m_BraceBlockDepth = 0;
		
		public HeaderReader(Stream inputStream)
		{
			m_SafeReader = new SafeLineReader(inputStream);
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

				// Deal with {} blocks
				/*
				if (content.StartsWith("{"))
				{
					++m_BraceBlockDepth;
					m_SafeReader.LeftOverContent = content.Substring(1);
					continue;
				}
				else if (content.StartsWith("}"))
				{
					--m_BraceBlockDepth;
					m_SafeReader.LeftOverContent = content.Substring(1);
					continue;
				}*/

				if (m_BraceBlockDepth == 0)
				{
					// If in unhandle

					// Try to figure out what is currently being read
					parseResult = CommentBlockSignature.TryParse(firstLine, content, m_SafeReader, out sigInfo)
						|| PreProcessorDefinitionSignature.TryParse(firstLine, content, m_SafeReader, out sigInfo)
						|| MacroCallSignature.TryParse(firstLine, content, m_SafeReader, out sigInfo);
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
