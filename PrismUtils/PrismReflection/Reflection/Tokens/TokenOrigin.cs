using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Tokens
{
	public class TokenOrigin
	{
		public static TokenOrigin Missing { get { return null; } }

		/// <summary>
		/// The file this token originated from
		/// </summary>
		public string FilePath { get; private set; }

		/// <summary>
		/// The line number this token was discovered on
		/// </summary>
		public int LineNumber { get; private set; }
		
		public TokenOrigin(string filePath, int lineNumber)
		{
			FilePath = filePath;
			LineNumber = lineNumber;
		}
	}
}
