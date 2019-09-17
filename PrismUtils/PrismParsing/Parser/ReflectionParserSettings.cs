using Prism.Reflection;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser
{
	public class ReflectionParserSettings : TokenSettings
	{
		/// <summary>
		/// Should all type variables be reflected
		/// </summary>
		[CommandLineArgument(Name = "implicit-strict", Usage = "Indicates if you want strict checking on implicit reflection", MustExist = false)]
		public bool UseStrictImplicitChecks = true;

		/// <summary>
		/// Should all type variables be reflected
		/// </summary>
		[CommandLineArgument(Name = "implicit-variables", Usage = "Indicates if you all variables inside of a reflected type will be reflected regardless of if the reflection token is used", MustExist = false)]
		public bool UseImplicitVariables = false;

		/// <summary>
		/// Should all type functions be reflected
		/// </summary>
		[CommandLineArgument(Name = "implicit-functions", Usage = "Indicates if you all function inside of a reflected type will be reflected regardless of if the reflection token is used", MustExist = false)]
		public bool UseImplicitFunctions = false;

		/// <summary>
		/// Should all type constructors be reflected
		/// </summary>
		[CommandLineArgument(Name = "implicit-constructors", Usage = "Indicates if you all constructors inside of a reflected type will be reflected regardless of if the reflection token is used", MustExist = false)]
		public bool UseImplicitConstructors = false;
	}
}
