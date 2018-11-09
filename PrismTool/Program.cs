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
			string path = @"G:\Side Projects\PrismTesting\PrismTesting\PrismTesting\MyTestClass.h";


			ReflectionSettings settings = new ReflectionSettings();

			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				HeaderReflection file = HeaderReflection.Generate(settings, stream);

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
