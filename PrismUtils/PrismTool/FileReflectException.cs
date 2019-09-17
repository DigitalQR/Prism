using Prism.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism
{
	public class FileReflectException : ReflectionException
	{
		public string m_SourceFile;
		public int m_SourceLine;

		public FileReflectException(Exception inner, string sourceFile, int sourceLine)
			: base(inner.Message, inner)
		{
			m_SourceFile = sourceFile;
			m_SourceLine = sourceLine;
		}
	}
}
