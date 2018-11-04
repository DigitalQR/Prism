﻿using System;
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
		/// The current leftovers which are going to be read during next SafeReadNext
		/// </summary>
		private string m_LeftOverContent = "";


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
		/// Safely read the next line content (Re-use any old content
		/// </summary>
		public bool SafeReadNext(out string line)
		{
			// Re-use anything that was leftover from last read
			if (m_LeftOverContent.Length != 0)
			{
				int index = m_LeftOverContent.IndexOf('\n');

				if (index == -1)
				{
					line = m_LeftOverContent;
					m_LeftOverContent = "";
				}
				else
				{
					line = m_LeftOverContent.Substring(0, index);
					m_LeftOverContent = m_LeftOverContent.Substring(index + 1);
				}
				return true;
			}
			// Attempt to read a new fresh line
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
						line = m_Reader.ReadLine().Trim();
						++m_CurrentLine;
					}
				} while (line == "");

				return true;
			}
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