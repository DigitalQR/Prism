using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.CodeParsing;
using Prism.Reflection;
using Prism.Utils;

namespace Prism
{
	class Program
	{
		class MyTestArgs
		{
			[CmdArg(Arg = "my-int", ShortArg = "i", Usage = "A test message", MustExist = true)]
			int myInt;

			[CmdArg(Arg = "my-int-array", ShortArg = "ia", Usage = "You should do a thing please", MustExist = true)]
			int[] myArray;
		}

		static void Main(string[] args)
		{
			//string path = @"G:\Side Projects\PrismTesting\PrismTesting\PrismTesting\MyTestClass.h";
			string path = @"G:\PrivateRepos\Testing\Project1\Project1\Vector3.h";


			ReflectionSettings settings = new ReflectionSettings();

			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				HeaderReflection file = HeaderReflection.Generate(settings, stream);

				string includeContent = "";
				string sourceContent = "";
				
				// Setup defaults
				includeContent = @"#pragma once
#include <Prism.h>

#ifndef PRISM_DEFER
#define PRISM_DEFER(...) __VA_ARGS__
#endif

#ifdef %CLASS_TOKEN%
#undef %CLASS_TOKEN%
#endif
#define %CLASS_TOKEN%(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef %STRUCT_TOKEN%
#undef %STRUCT_TOKEN%
#endif
#define %STRUCT_TOKEN%(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef %ENUM_TOKEN%
#undef %ENUM_TOKEN%
#endif
#define %ENUM_TOKEN%(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef %FUNCTION_TOKEN%
#undef %FUNCTION_TOKEN%
#endif
#define %FUNCTION_TOKEN%(...)

#ifdef %VARIABLE_TOKEN%
#undef %VARIABLE_TOKEN%
#endif
#define %VARIABLE_TOKEN%(...)
";
				// Setup defaults
				sourceContent = @"
#include ""%FILE_PATH%""
";

				// Resolve placeholders
				includeContent = includeContent
					//.Replace("%FILE_PATH%", path)
					.Replace("%CLASS_TOKEN%", settings.ClassToken)
					.Replace("%STRUCT_TOKEN%", settings.StructToken)
					.Replace("%ENUM_TOKEN%", settings.EnumToken)
					.Replace("%FUNCTION_TOKEN%", settings.FunctionToken)
					.Replace("%VARIABLE_TOKEN%", settings.VariableToken);

				sourceContent = sourceContent
					.Replace("%FILE_PATH%", path);

				for (int i = 0; i < file.ReflectedTokenCount; ++i)
				{
					var token = file.ReflectedTokens[i];

					string tokenHeader = token.GenerateHeaderReflectionContent();
					string tokenInclude = token.GenerateIncludeReflectionContent();
					string tokenSource = token.GenerateSourceReflectionContent();

					// Raw include content
					string finalInclude = "";
					string finalSource = tokenSource;

					finalInclude += tokenInclude + "\n";

					// Macro replacement 
					string headerMacro = @"
#if %TOKEN_CONDITION%
#ifdef PRISM_REFLECTION_BODY_%TOKEN_LINE%
#undef PRISM_REFLECTION_BODY_%TOKEN_LINE%
#endif
#define PRISM_REFLECTION_BODY_%TOKEN_LINE% %MACRO_CONTENT%
#endif
";
					finalInclude += headerMacro
						.Replace("%TOKEN_CONDITION%", "" + token.PreProcessorCondition)
						.Replace("%TOKEN_LINE%", "" + token.TokenLineNumber)
						.Replace("%MACRO_CONTENT%", tokenHeader.Replace("\r\n", " \\\n"));


					// Little gap to make debugging easier on the eyes
					includeContent += "\n//=======================//\n" + finalInclude;
					sourceContent += "\n//=======================//\n" + finalSource;

					//string internalMacro = "PRISM_REFLECTION_BODY_" + token.lin
					//headerContent += "#define PRISM_REFLECTION_BODY_"
				}

				// Fix line-endings
				includeContent = includeContent.Replace("\r\n", "\n").Replace("\n", "\r\n");
				sourceContent = sourceContent.Replace("\r\n", "\n").Replace("\n", "\r\n");

				File.WriteAllText(@"G:\PrivateRepos\Testing\Project1\Project1\Vector3.agpc.h", includeContent);
				File.WriteAllText(@"G:\PrivateRepos\Testing\Project1\Project1\Vector3.agpc.cpp", sourceContent);

				Console.WriteLine("Yeeeeet");
			}



				args = new string[10]{
				"--my-int=1234",
				"--my-int=1234",
				"--my-int-array=12342",
				"--my-int-array=12342",
				"--my-int-array=12342",
				"--my-int-array=12342",
				"--my-int-array=12342",
				"--my-int-array=12342",
				"--my-int-array=12342",
				"-unrelatedThing"
			};
			
			MyTestArgs myThing = new MyTestArgs();
			try
			{
				CmdArgs.Parse(myThing, args);
				CmdArgs.Parse(myThing, args);
			}
			catch (CmdArgParseException e)
			{
				Console.Error.WriteLine(e.Message);
				Console.Error.WriteLine(e.Usage);
				Console.Error.WriteLine(e.StackTrace);
#if DEBUG
				throw e;
#endif
			}

			return;
		}
	}
}
