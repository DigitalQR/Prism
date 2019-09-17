using Prism.Parser.Cpp.Token;
using Prism.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser
{
	/// <summary>
	/// Internal exception type that will be thrown by CppReflectionReader when an issue occurs
	/// </summary>
	public class ParseException : ReflectionException
	{
		public CppTokenInfo m_TokenInfo;

		public ParseException(string message, CppTokenInfo tokenInfo = null)
			: base(message)
		{
			m_TokenInfo = tokenInfo;
		}
	}
}
