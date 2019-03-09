using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Prism.Export;
using Prism.Parsing;
using Prism.Reflection;
using Prism.Reflection.Behaviour;
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
			[CommandLineArgument(Name = "src-dir")]
			public string DirectorySource = null;

			[CommandLineArgument(Name = "src-vsproj")]
			public string VisualStudioSource = null;
		}


		/// <summary>
		/// Format a message in the Visual Studio message format 
		/// {file}({line}): error {code}: {message}
		/// </summary>
		private static string FormatErrorMessage(string message, string source, int line = -1, ParseErrorCode errorCode = ParseErrorCode.GenericError)
		{
			return source + "(" + line + "): error PRI" + (int)errorCode + ": " + message;
		}

		static void Main(string[] args)
		{
			CommandLineArguments.ProvideArguments(args);

			ReflectionSelector selector = new ReflectionSelector();
			CommandLineArguments.FillValues(selector);

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
					DirectoryReflector reflector = new DirectoryReflector();
					reflector.Run();
				}

				// Reflect project
				else if (selector.VisualStudioSource != null)
				{
					VisualStudioReflector reflector = new VisualStudioReflector();
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
			catch (CommandLineArgumentParseException e)
			{
				Console.Error.WriteLine("CommandLineArgumentParseException caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, "PrismTool.exe"));
				Console.Error.WriteLine(FormatErrorMessage(e.Usage, "PrismTool.exe"));
				Console.Error.WriteLine(e.StackTrace);
				Environment.Exit(-1);
			}
			catch (HeaderParseException e)
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
