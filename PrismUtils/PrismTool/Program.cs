using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Prism.Export;
using Prism.Parser;
using Prism.Parser.Cpp;
using Prism.Parser.Cpp.Token;
using Prism.Parsing;
using Prism.Reflection;
using Prism.Reflection.Behaviour;
using Prism.Reflection.Elements.Cpp;
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
			[CommandLineArgument(Name = "src-file")]
			public string[] FileSources = null;

			[CommandLineArgument(Name = "src-dir")]
			public string DirectorySource = null;

			[CommandLineArgument(Name = "run-default-behaviour", Usage = "Should the default behaviour be executed", MustExist = false)]
			public bool RunDefaultBehaviour = true;
		}
		
		/// <summary>
		/// Format a message in the Visual Studio message format 
		/// {file}({line}): error {code}: {message}
		/// </summary>
		private static string FormatErrorMessage(string message, string source, int line = -1)
		{
			return source + "(" + line + "): error PRISM: " + message;
		}

		static void Main(string[] args)
		{			
			ReflectionSelector selector = new ReflectionSelector();
			CommandLineArguments.ProvideArguments(args);
			CommandLineArguments.FillValues(selector);

			if(selector.RunDefaultBehaviour)
				Assembly.Load("PrismDefaultBehaviour");

#if !DEBUG
			try
#endif
			{
				// Multiple reflection
				if (selector.FileSources != null && selector.DirectorySource != null)
				{
					throw new Exception("Found --src-file and --src-dir. Only supports a single one at a time.");
				}

				// Reflect files
				else if (selector.FileSources != null)
				{
					FilesReflector reflector = new FilesReflector(selector.FileSources);
					reflector.Run();
				}

				// Reflect directory
				else if (selector.DirectorySource != null)
				{
					DirectoryReflector reflector = new DirectoryReflector();
					reflector.Run();
				}
				
				// No reflection
				else
				{
					throw new Exception("Not found code source. Expect --src-dir or --src-file.");
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
			catch (FileReflectException e)
			{
				Console.Error.WriteLine("FileReflectException Caught");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, e.m_SourceFile, e.m_SourceLine));
				Console.Error.WriteLine(e.InnerException.StackTrace);
				Environment.Exit(-1);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Exception Caught (" + e.GetType().Name + ")");
				Console.Error.WriteLine(FormatErrorMessage(e.Message, "PrismTool.exe"));
				Console.Error.WriteLine(e.StackTrace);
				Environment.Exit(-1);
			}
#endif
		}
	}
}
