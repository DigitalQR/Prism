using Prism.Parsing;
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
	public class FilesReflector : ReflectorBase
	{
		/// <summary>
		/// Where all the output files will be placed
		/// </summary>
		[CommandLineArgument(Name = "out-dir", Usage = "The directory where output files will be placed", MustExist = true)]
		private string m_OutputDirectory;

		/// <summary>
		/// All the files reflection should be ran on
		/// </summary>
		private IEnumerable<string> m_SourceFiles;
		
		public FilesReflector(IEnumerable<string> sourceFiles)
		{
			m_SourceFiles = sourceFiles;

			CommandLineArguments.FillValues(this);
		}
		
		public override bool Run()
		{
			return RunInternal(m_SourceFiles, m_OutputDirectory);
		}
	}
}
