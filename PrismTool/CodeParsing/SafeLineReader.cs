using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing
{
	/// <summary>
	/// Internal reader util feeding into signature parsers
	/// </summary>
	public class SafeLineReader : IDisposable
	{
		/// <summary>
		/// The internal reader that is being used
		/// </summary>
		private StreamReader m_Reader;

		/// <summary>
		/// The current line in the file we are at
		/// </summary>
		private long m_CurrentLine = 0;

		/// <summary>
		/// Any contents which were left over from the current read
		/// </summary>
		private string m_LeftOverContent = "";

		/// <summary>
		/// Any contents which should be appended to this current line (Leftovers during pre-read i.e. { or ;)
		/// </summary>
		private string m_AppendContents = "";


		public SafeLineReader(Stream inputStream)
		{
			m_Reader = new StreamReader(inputStream);
		}

		public Encoding CurrentEncoding { get => m_Reader.CurrentEncoding; }

		public bool EndOfStream { get => m_Reader.EndOfStream; }

		public Stream BaseStream { get => m_Reader.BaseStream; }

		/// <summary>
		/// The current line number this reader is at
		/// </summary>
		public long LineNumber { get => m_CurrentLine; }
		
		/// <summary>
		/// The current left over content this reader has (Will consume this before reading a fresh line)
		/// </summary>
		public string LeftOverContent { set => m_LeftOverContent += value.Trim(); }


		/// <summary>
		/// Consume whatever the next line should be;
		/// </summary>
		private bool ConsumeLine(out string line)
		{
			if (!string.IsNullOrEmpty(m_LeftOverContent))
			{
				line = m_LeftOverContent + m_AppendContents;
				m_LeftOverContent = "";
				m_AppendContents = "";
				return true;
			}
			else if (!string.IsNullOrEmpty(m_AppendContents))
			{
				line = m_AppendContents;
				m_AppendContents = "";
				return true;
			}
			else
			{
				line = "";

				do
				{
					if (m_Reader.EndOfStream)
					{
						line = "";
						return false;
					}
					else
					{
						line = m_Reader.ReadLine().Replace('\t', ' ').Trim();
						++m_CurrentLine;
					}
				} while (line == "");

				return true;
			}
		}

		/// <summary>
		/// Safely read the next line content (Re-use any old content
		/// </summary>
		public bool SafeReadNext(out string line)
		{
			if (ConsumeLine(out line))
			{
				// Split at ; and {
				int endLineIndex = line.IndexOf(';');
				int startBlockIndex = line.IndexOf('{');

				int index = -1;
				if (endLineIndex != -1 && startBlockIndex != -1)
				{
					index = Math.Min(endLineIndex, startBlockIndex);
				}
				else if (endLineIndex == -1)
				{
					index = startBlockIndex;
				}

				if (index != -1)
				{
					m_AppendContents = line.Substring(index + 1).Trim();
					line = line.Substring(0, index + 1).Trim();
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Safely read until line contains a given string
		/// </summary>
		public bool SafeReadUntil(string containStr, out string line)
		{
			// Initial line check
			string content;
			line = "";
			do
			{
				if (!SafeReadNext(out content))
				{
					return false;
				}
				else
				{
					line += content + '\n';
				}
			} while (!content.Contains(containStr));

			line = line.Trim();
			return true;
		}

		/// <summary>
		/// Safely read until line contains on of a given string
		/// </summary>
		public bool SafeReadUntil(string[] containStrs, out string line)
		{
			// Initial line check
			string content;
			bool foundString = false;
			line = "";

			do
			{
				if (!SafeReadNext(out content))
				{
					return false;
				}
				else
				{
					line += content + '\n';
				}
				
				foreach (string containStr in containStrs)
				{
					foundString = content.Contains(containStr);
					if (foundString)
						break;
				}

			} while (!foundString);

			line = line.Trim();
			return true;
		}

		public void Dispose()
		{
			m_Reader.Dispose();
		}
	}
}
