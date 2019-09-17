using Prism.Parsing;
using Prism.Reflection;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Export
{
	/// <summary>
	/// Reflects all files in a given folder
	/// </summary>
	public class DirectoryReflector : ReflectorBase
	{
		/// <summary>
		/// Where all the source files will be read from
		/// </summary>
		[CommandLineArgument(Name = "src-dir", Usage = "The directory of the files to be read", MustExist = true)]
		private string m_SourceDirectory;

		/// <summary>
		/// Where all the output files will be placed
		/// </summary>
		[CommandLineArgument(Name = "out-dir", Usage = "The directory where output files will be placed", MustExist = true)]
		private string m_OutputDirectory;

		/// <summary>
		/// The extensions that are supported
		/// </summary>
		[CommandLineArgument(Name = "parse-ext", Usage = "File extensions which will be read", MustExist = false)]
		private string[] m_WhitelistedExtensions;
		
		public DirectoryReflector()
		{
			m_WhitelistedExtensions = new string[] { ".h", ".hpp" };

			CommandLineArguments.FillValues(this);
		}
		
		public override bool Run()
		{
			var sourceFiles = Directory.EnumerateFiles(m_SourceDirectory, "*.*", SearchOption.AllDirectories).Where(f => m_WhitelistedExtensions.Contains(Path.GetExtension(f)));
			return RunInternal(sourceFiles, m_OutputDirectory);
		}
	}
}
