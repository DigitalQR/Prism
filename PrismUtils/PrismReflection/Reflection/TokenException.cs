using Prism.Reflection.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public class TokenException : Exception
	{
		public IReflectableToken Token { get; private set; }

		public TokenException(IReflectableToken token, string message)
			: base(message)
		{
			Token = token;
		}
	}
}
