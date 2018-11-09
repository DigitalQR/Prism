using Prism.CodeParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public class ReflectionException : Exception
	{
		public SignatureInfo Signature { get; private set; }

		public ReflectionException(SignatureInfo signature, string message) 
			: base(message)
		{
			Signature = signature;
		}
	}
}
