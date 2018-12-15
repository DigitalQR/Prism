using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Prism.CodeParsing;
using Prism.Export;
using Prism.Reflection;
using Prism.Utils;

namespace Prism
{
	class Program
	{
		/// <summary>
		/// Determine to use folder or project exporting based on cmd arg presence
		/// </summary>
		internal class ReflectionSelector
		{
			[CmdArg(Arg = "src-dir")]
			public string DirectorySource = null;

			[CmdArg(Arg = "src-vsproj")]
			public string VisualStudioSource = null;
		}


		/// <summary>
		/// Format a message in the Visual Studio message format 
		/// {file}({line}): error {code}: {message}
		/// </summary>
		private static string FormatErrorMessage(string message, string source, int line = -1, ReflectionErrorCode errorCode = ReflectionErrorCode.GenericError)
		{
			return source + "(" + line + "): error PRI" + (int)errorCode + ": " + message;
		}

		static void Main(string[] args)
		{
			ReflectionSelector selector = new ReflectionSelector();
			CmdArgs.Parse(selector, args);

#if !DEBUG
			try
#endif
			{
				// Multiple reflection
				if (selector.DirectorySource != null && selector.VisualStudioSource != null)
				{
					throw new Exception("Found --src-dir and --src-vsproj. Only supports a single one at a time.");
				}

				// Reflect directory
				else if (selector.DirectorySource != null)
				{
					DirectoryReflector reflector = new DirectoryReflector(args);
					reflector.Run();
				}

				// Reflect project
				else if (selector.VisualStudioSource != null)
				{
					VisualStudioReflector reflector = new VisualStudioReflector(args);
					reflector.Run();
				}

				// No reflection
				else
				{
					throw new Exception("Not found code source. Exprect --src-dir or --src-vsproj.");
				}
			}
#if !DEBUG
			// Report errors in the VS message format <file>(<line>): error <code>: <message>
			catch (CmdArgParseException e)
			{
				Console.Error.WriteLine("CmdArgParseException caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, "PrismTool.exe"));
				Console.Error.WriteLine(FormatErrorMessage(e.Usage, "PrismTool.exe"));
				Console.Error.WriteLine(e.StackTrace);
				Environment.Exit(-1);
			}
			catch (HeaderReflectionException e)
			{
				Console.Error.WriteLine("HeaderReflectionException Caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, e.FilePath, (int)e.Signature.LineNumber, e.ErrorCode));
				Console.Error.WriteLine(e.InnerException.StackTrace);
				Environment.Exit(-1);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Exception Caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, "PrismTool.exe"));
				Console.Error.WriteLine(e.StackTrace);
				Environment.Exit(-1);
			}
#endif
		}
	}
}
