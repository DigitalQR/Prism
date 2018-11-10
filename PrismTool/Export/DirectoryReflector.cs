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
		[CmdArg(Arg = "src-dir", Usage = "The directory of the files to be read", MustExist = true)]
		private string m_SourceDirectory;

		/// <summary>
		/// Where all the output files will be placed
		/// </summary>
		[CmdArg(Arg = "out-dir", Usage = "The directory where output files will be placed", MustExist = true)]
		private string m_OutputDirectory;

		/// <summary>
		/// The extensions that are supported
		/// </summary>
		[CmdArg(Arg = "parse-ext", Usage = "File extensions which will be read", MustExist = false)]
		private string[] m_WhitelistedExtensions;
		
		/// <summary>
		/// Reflection settings used by this reflector
		/// </summary>
		private ReflectionSettings m_ReflectionSettings;

		public DirectoryReflector(string[] args)
		{
			m_ReflectionSettings = new ReflectionSettings();
			m_WhitelistedExtensions = new string[] { ".h", ".hpp" };

			CmdArgs.Parse(this, args);
			CmdArgs.Parse(m_ReflectionSettings, args);
		}

		public override string IntermediateFolder
		{
			get { return Path.Combine(m_OutputDirectory, ".prism"); }
		}

		public override List<ExportFile> Run()
		{
			List<ExportFile> outputFiles = new List<ExportFile>();

			var sourceFiles = Directory.EnumerateFiles(m_SourceDirectory, "*.*", SearchOption.AllDirectories).Where(f => m_WhitelistedExtensions.Contains(Path.GetExtension(f)));
			string outputDir = m_OutputDirectory.ToLower();

			Console.WriteLine("Generating Prism Reflection -> " + m_OutputDirectory);
			foreach (var file in sourceFiles)
			{
				try
				{
					// Don't read in output directory
					if (!file.ToLower().StartsWith(outputDir))
					{
						List<ExportFile> exports = ReflectFile(m_ReflectionSettings, file, m_OutputDirectory);
						outputFiles.AddRange(exports);
					}
				}
				catch (ReflectionException e)
				{
					throw new HeaderReflectionException(file, e.ErrorCode, e.Signature, e);
				}
			}
			Console.WriteLine("Prism Reflection Generated.");

			LastBuildTime = DateTime.UtcNow;
			return outputFiles;
		}
	}
}
