using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Writer
{
	interface IReflectionOutput
	{
		/// <summary>
		/// The the content to place in the header file (Through the macro token)
		/// </summary>
		string GenerateHeaderReflectionContent();

		/// <summary>
		/// The the content to place in the include reflection file
		/// </summary>
		string GenerateIncludeReflectionContent();

		/// <summary>
		/// The content to place in the source reflection file
		/// </summary>
		string GenerateSourceReflectionContent();
	}
}
