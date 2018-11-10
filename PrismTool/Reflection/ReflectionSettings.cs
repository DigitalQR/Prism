using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public class ReflectionSettings
	{
		/// <summary>
		/// The token to indicate a class which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CmdArg(Arg = "class-token", ShortArg = "c", Usage = "The token which indicates a class which should be reflected", MustExist = false)]
		public string ClassToken = "REFLECT_CLASS";

		/// <summary>
		/// The token to indicate a struct which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CmdArg(Arg = "struct-token", ShortArg = "s", Usage = "The token which indicates a struct which should be reflected", MustExist = false)]
		public string StructToken = "REFLECT_STRUCT";

		/// <summary>
		/// The token to indicate a enum which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CmdArg(Arg = "enum-token", ShortArg = "e", Usage = "The token which indicates a enum which should be reflected", MustExist = false)]
		public string EnumToken = "REFLECT_ENUM";

		
		/// <summary>
		/// The token to indicate a variable which should be reflected
		/// Expected to be found just above the variable
		/// </summary>
		[CmdArg(Arg = "variable-token", ShortArg = "v", Usage = "The token which indicates a variable which should be reflected", MustExist = false)]
		public string VariableToken = "REFLECT_VAR";

		/// <summary>
		/// Should all type variables be reflected
		/// </summary>
		[CmdArg(Arg = "impl-variables", ShortArg = "i", Usage = "Indicates if you all variables inside of a reflected type will be reflected regardless of if the reflection token is used", MustExist = false)]
		public bool UseImplicitVariables = false;


		/// <summary>
		/// The token to indicate a function which should be reflected
		/// Expected to be found just above the function
		/// </summary>
		[CmdArg(Arg = "function-token", ShortArg = "f", Usage = "The token which indicates a function which should be reflected", MustExist = false)]
		public string FunctionToken = "REFLECT_FUNC";

		/// <summary>
		/// Should all type functions be reflected
		/// </summary>
		[CmdArg(Arg = "impl-functions", ShortArg = "i", Usage = "Indicates if you all function inside of a reflected type will be reflected regardless of if the reflection token is used", MustExist = false)]
		public bool UseImplicitFunctions = false;

		
		/// <summary>
		/// Should all files be rebuilt
		/// </summary>
		[CmdArg(Arg = "rebuild", ShortArg = "r", Usage = "Force all files to be rescanned and rebuilt", MustExist = false)]
		public bool RebuildEverything = false;
	}
}
