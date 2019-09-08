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

		/// <summary>
		/// Reflection settings used by this reflector
		/// </summary>
		private ReflectionSettings m_ReflectionSettings;

		public FilesReflector(IEnumerable<string> sourceFiles)
		{
			m_SourceFiles = sourceFiles;
			m_ReflectionSettings = new ReflectionSettings();

			CommandLineArguments.FillValues(this);
			CommandLineArguments.FillValues(m_ReflectionSettings);
		}

		public override string IntermediateFolder
		{
			get { return Path.Combine(m_OutputDirectory, ".prism"); }
		}

		public override List<ExportFile> Run()
		{
			return RunInternal(m_ReflectionSettings, m_SourceFiles, m_OutputDirectory);
		}
	}
}
