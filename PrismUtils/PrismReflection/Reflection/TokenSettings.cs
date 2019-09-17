using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public class TokenSettings
	{
		/// <summary>
		/// The token to indicate a class which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CommandLineArgument(Name = "class-token", Usage = "The token which indicates a class which should be reflected", MustExist = false)]
		public string ClassToken = "REFLECT_CLASS";

		/// <summary>
		/// The token to indicate a struct which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CommandLineArgument(Name = "struct-token", Usage = "The token which indicates a struct which should be reflected", MustExist = false)]
		public string StructToken = "REFLECT_STRUCT";

		/// <summary>
		/// The token to indicate a enum which should be reflected
		/// Expected to be found as the first line of the class body
		/// </summary>
		[CommandLineArgument(Name = "enum-token", Usage = "The token which indicates a enum which should be reflected", MustExist = false)]
		public string EnumToken = "REFLECT_ENUM";


		/// <summary>
		/// The token to indicate a variable which should be reflected
		/// Expected to be found just above the variable
		/// </summary>
		[CommandLineArgument(Name = "variable-token", Usage = "The token which indicates a variable which should be reflected", MustExist = false)]
		public string VariableToken = "REFLECT_VAR";

		/// <summary>
		/// The token to indicate a function which should be reflected
		/// Expected to be found just above the function
		/// </summary>
		[CommandLineArgument(Name = "function-token", Usage = "The token which indicates a function which should be reflected", MustExist = false)]
		public string FunctionToken = "REFLECT_FUNC";
		
		/// <summary>
		/// The token to indicate a function which should be reflected
		/// Expected to be found just above the function
		/// </summary>
		[CommandLineArgument(Name = "function-token", Usage = "The token which indicates a constructor which should be reflected", MustExist = false)]
		public string ConstructorToken = "REFLECT_CNTR";
	}
}
