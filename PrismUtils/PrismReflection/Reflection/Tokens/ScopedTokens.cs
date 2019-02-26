using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Tokens
{
	public enum AccessorMode
	{
		Unknown,
		Public,
		Private,
		Protected
	}

	public static class AccessorModeUtils
	{
		public static AccessorMode Parse(string rawAccessor)
		{
			AccessorMode mode = AccessorMode.Unknown;

			if (rawAccessor.Equals("public", StringComparison.CurrentCultureIgnoreCase))
				mode = AccessorMode.Public;
			else if (rawAccessor.Equals("private", StringComparison.CurrentCultureIgnoreCase))
				mode = AccessorMode.Private;
			else if (rawAccessor.Equals("protected", StringComparison.CurrentCultureIgnoreCase))
				mode = AccessorMode.Protected;

			return mode;
		}
	}
}
