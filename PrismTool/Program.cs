using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.CodeParsing;
using Prism.Export;
using Prism.Reflection;
using Prism.Utils;

namespace Prism
{
	class Program
	{
		/// <summary>
		/// Format a message in the Visual Studio message format 
		/// {file}({line}): error {code}: {message}
		/// </summary>
		private static string FormatErrorMessage(string message, string source, int line = -1, ReflectionErrorCode errorCode = ReflectionErrorCode.GenericError)
		{
			return source + "(" + line + "): error PER" + (int)errorCode + ": " + message;
		}

		static void Main(string[] args)
		{
#if !DEBUG
			try
			{
#endif
			DirectoryReflector reflector = new DirectoryReflector(args);
			reflector.Run();
#if !DEBUG
			}

			// Report errors in the VS message format <file>(<line>): error <code>: <message>
			catch (CmdArgParseException e)
			{
				Console.Error.WriteLine("CmdArgParseException caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, "PrismTool.exe"));
				Console.Error.WriteLine(FormatErrorMessage(e.Usage, "PrismTool.exe"));
				Console.Error.WriteLine(e.StackTrace);
				Environment.Exit(400);
			}
			catch (HeaderReflectionException e)
			{
				Console.Error.WriteLine("HeaderReflectionException Caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, e.FilePath, (int)e.Signature.LineNumber, e.ErrorCode));
				Console.Error.WriteLine(e.InnerException.StackTrace);
				Environment.Exit(400);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Exception Caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, "PrismTool.exe"));
				Console.Error.WriteLine(e.StackTrace);
				Environment.Exit(400);
			}
#endif
		}
	}
}
