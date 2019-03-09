using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing
{
	public class ReflectionSettings
	{
		/// <summary>
		/// The extension which will be applied to export files
		/// </summary>
		[CommandLineArgument(Name = "export-ext", Usage = "The extention all exported files will prepend to thier current", MustExist = false)]
		public string ExportExtension = ".refl";

		/// <summary>
		/// The token to indicate a class which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CommandLineArgument(Name = "class-token", ShortName = "c", Usage = "The token which indicates a class which should be reflected", MustExist = false)]
		public string ClassToken = "REFLECT_CLASS";

		/// <summary>
		/// The token to indicate a struct which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CommandLineArgument(Name = "struct-token", ShortName = "s", Usage = "The token which indicates a struct which should be reflected", MustExist = false)]
		public string StructToken = "REFLECT_STRUCT";

		/// <summary>
		/// The token to indicate a enum which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CommandLineArgument(Name = "enum-token", ShortName = "e", Usage = "The token which indicates a enum which should be reflected", MustExist = false)]
		public string EnumToken = "REFLECT_ENUM";

		
		/// <summary>
		/// The token to indicate a variable which should be reflected
		/// Expected to be found just above the variable
		/// </summary>
		[CommandLineArgument(Name = "variable-token", ShortName = "v", Usage = "The token which indicates a variable which should be reflected", MustExist = false)]
		public string VariableToken = "REFLECT_VAR";

		/// <summary>
		/// Should all type variables be reflected
		/// </summary>
		[CommandLineArgument(Name = "impl-variables", ShortName = "i", Usage = "Indicates if you all variables inside of a reflected type will be reflected regardless of if the reflection token is used", MustExist = false)]
		public bool UseImplicitVariables = false;


		/// <summary>
		/// The token to indicate a function which should be reflected
		/// Expected to be found just above the function
		/// </summary>
		[CommandLineArgument(Name = "function-token", ShortName = "f", Usage = "The token which indicates a function which should be reflected", MustExist = false)]
		public string FunctionToken = "REFLECT_FUNC";

		/// <summary>
		/// Should all type functions be reflected
		/// </summary>
		[CommandLineArgument(Name = "impl-functions", ShortName = "i", Usage = "Indicates if you all function inside of a reflected type will be reflected regardless of if the reflection token is used", MustExist = false)]
		public bool UseImplicitFunctions = false;

		
		/// <summary>
		/// Should all files be rebuilt
		/// </summary>
		[CommandLineArgument(Name = "rebuild", ShortName = "r", Usage = "Force all files to be rescanned and rebuilt", MustExist = false)]
		public bool RebuildEverything = false;

		/// <summary>
		/// If an exception is thrown in a file which doesn't have the required reflection include, will it be treated as an error
		/// </summary>
		[CommandLineArgument(Name = "ignore-badform", Usage = "If an exception is thrown in a file which doesn't have the required reflection include, will it be treated as an error", MustExist = false)]
		public bool IgnoreBadForm = true;
	}
}
