using Prism.Parser;
using Prism.Parser.Cpp;
using Prism.Parsing;
using Prism.Reflection;
using Prism.Reflection.Behaviour;
using Prism.Reflection.Elements;
using Prism.Reflection.Elements.Cpp;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Export
{
	public abstract class ReflectorBase
	{
		/// <summary>
		/// The extension which will be applied to export files
		/// </summary>
		[CommandLineArgument(Name = "export-ext", Usage = "The extention all exported files will prepend to thier current", MustExist = false)]
		protected string m_ExportExtension = ".refl";

		/// <summary>
		/// Other assemblies to be loaded
		/// </summary>
		[CommandLineArgument(Name = "custom-behaviour", Usage = "File path to any additional assemblies")]
		private string[] m_AdditionalAssemblies;


		/// <summary>
		/// The reflection behaviour controller for thie reflector
		/// </summary>
		private BehaviourController m_BehaviourController;

		/// <summary>
		/// Settings to be used during parsing/reflection
		/// </summary>
		private ReflectionParserSettings m_ReflectionSettings;

		public ReflectorBase()
		{
			m_ReflectionSettings = new ReflectionParserSettings();
			CommandLineArguments.FillValues(m_ReflectionSettings);
			CommandLineArguments.FillValues(this);

			m_BehaviourController = new BehaviourController();
			m_BehaviourController.PopulateBehaviours();
			m_BehaviourController.PopulateBehaviours(m_AdditionalAssemblies);
			
		}

		/// <summary>
		/// Runs through each file, as specified by the reflector
		/// </summary>
		public abstract bool Run();

		/// <summary>
		/// Runs through the given files and exports any reflection data
		/// </summary>
		protected bool RunInternal(IEnumerable<string> sourceFiles, string outputDirectory)
		{			
			Console.WriteLine("Generating Prism Reflection -> " + outputDirectory);

			if (!Directory.Exists(outputDirectory))
				Directory.CreateDirectory(outputDirectory);

			foreach (string sourceFile in sourceFiles)
			{
				if (!ReflectFile(sourceFile, outputDirectory))
					return false;
			}

			Console.WriteLine("Prism Reflection Generated.");
			return true;
		}

		/// <summary>
		/// Reflect a given input file and return the export information about any new files
		/// </summary>
		/// <returns>All the reflection files which have been created from these</returns>
		protected bool ReflectFile(string sourceFile, string outputDir)
		{
			using (CppReflectionReader reader = new CppReflectionReader(m_ReflectionSettings, sourceFile))
			{
#if !DEBUG
				try
#endif
				{
					if (reader.ParseContent(out ContentElement content))
					{
						m_BehaviourController.ProcessElement(content);

						Console.WriteLine("\tReflecting " + Path.GetFileName(sourceFile));
						ReflectionExporter exporter = new ReflectionExporter(content);

						string includeContent = exporter.GenerateIncludeContent();
						string sourceContent = exporter.GenerateSourceContent();

						string includeGenPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(sourceFile) + m_ExportExtension + ".h");
						string sourceGenPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(sourceFile) + m_ExportExtension + ".cpp");

						WriteContentToFile(includeGenPath, includeContent);
						WriteContentToFile(sourceGenPath, sourceContent);
						return true;
					}
					else
						return false;
				}
#if !DEBUG
				catch (ParseException e)
				{
					throw new FileReflectException(e, sourceFile, (int)e.m_TokenInfo.m_LineNumber);
				}
				catch (BehaviourException e)
				{
					throw new FileReflectException(e, sourceFile, e.m_Element != null ? (int)e.m_Element.Origin.LineNumber : -1);
				}
				catch (ElementException e)
				{
					throw new FileReflectException(e, sourceFile, e.m_Element != null ? (int)e.m_Element.Origin.LineNumber : -1);
				}
				catch (Exception e)
				{
					throw new FileReflectException(e, sourceFile, -1);
				}
#endif
			}
		}

		/// <summary>
		/// Write CR-LF formatted content to a file
		/// </summary>
		private void WriteContentToFile(string filePath, string content)
		{
			File.WriteAllText(filePath, content.Replace("\r\n", "\n").Replace("\n", "\r\n"));
		}
	}
}
