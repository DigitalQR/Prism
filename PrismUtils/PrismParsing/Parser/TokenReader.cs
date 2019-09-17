using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser
{
	public abstract class TokenReader : IDisposable
	{
		private StreamReader m_Reader;
		private Queue<char> m_Leftovers;
		private StringWriter m_CurrentToken;
		private char m_CurrentChar;
		private long m_LineNumber;

		public TokenReader(Stream source)
		{
			if (!source.CanRead)
			{
				throw new InvalidOperationException("Source stream must be readable");
			}

			m_Reader = new StreamReader(source);
			m_Leftovers = new Queue<char>();
			m_CurrentToken = new StringWriter();
			m_CurrentChar = '\0';
			m_LineNumber = 1;
		}

		public void Dispose()
		{
			m_Reader.Dispose();
		}

		public void EnqueueLeftovers(IEnumerable<char> content)
		{
			foreach (char c in content)
			{
				if (c == '\n')
					--m_LineNumber;
				m_Leftovers.Enqueue(c);
			}
		}

		protected char CurrentChar
		{
			get { return m_CurrentChar; }
		}
		
		protected string CurrentToken
		{
			get { return m_CurrentToken.ToString(); }
		}

		public long LineNumber
		{
			get { return m_LineNumber; }
		}
		
		public abstract bool Next(out string token, out long lineNumber);

		/// <summary>
		/// Safely read the next character
		/// </summary>
		protected bool NextChar()
		{
			int read = ReadRaw();

			if (read != -1)
			{
				char readChar = (char)read;

				// Ignore return
				if (readChar == '\r')
				{
					return NextChar();
				}
				else
				{
					m_CurrentChar = readChar;
					m_CurrentToken.Write(m_CurrentChar);

					if (m_CurrentChar == '\n')
						++m_LineNumber;

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Read a raw character from the leftovers or stream
		/// </summary>
		private int ReadRaw()
		{
			if (m_Leftovers.Any())
			{
				return (int)m_Leftovers.Dequeue();
			}
			else
			{
				return m_Reader.Read();
			}
		}
				
		/// <summary>
		/// Safely read until the start of the next token, ignoring whitespaces
		/// </summary>
		protected bool FindTokenStart()
		{
			while (NextChar())
			{
				if (!char.IsWhiteSpace(m_CurrentChar))
				{
					// Reset token writer
					var builder = m_CurrentToken.GetStringBuilder();
					builder.Remove(0, builder.Length);
					m_CurrentToken.Write(m_CurrentChar);
					return true;
				}
			}

			return false;
		}

		protected delegate bool QueryChar(char c);
		protected delegate bool QueryString(string str);

		/// <summary>
		/// Read until the query returns true
		/// </summary>
		/// <returns>Was the token found or false if eos was reached</returns>
		protected bool ReadUntilChar(QueryChar query)
		{
			while (NextChar())
			{
				if (query(CurrentChar))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Read until the query returns true
		/// </summary>
		/// <returns>Was the token found or false if eos was reached</returns>
		protected bool ReadUntilString(QueryString query)
		{
			while (NextChar())
			{
				if (query(CurrentToken))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Read until one of the chars is found
		/// </summary>
		/// <returns>Was the token found or false if eos was reached</returns>
		protected bool ReadUntilChar(params char[] find)
		{
			return ReadUntilChar((c) => find.Contains(c));
		}

		/// <summary>
		/// Read until one of the strings is found
		/// </summary>
		/// <returns>Was the token found or false if eos was reached</returns>
		protected bool ReadUntilString(params string[] find)
		{
			return ReadUntilString((str) => find.Where((s) => CurrentToken.EndsWith(s)).Any());
		}
	}
}
