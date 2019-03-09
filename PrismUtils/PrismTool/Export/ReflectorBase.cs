﻿using Prism.Parsing;
using Prism.Reflection;
using Prism.Reflection.Behaviour;
using Prism.Reflection.Tokens;
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
		/// Header text which should be attached to every export file
		/// </summary>
		private static string s_ExportHeader = @"/////////////////////////////////////////////////////////////////////////////////////////////
/// This file contains auto-generated content as exported by the Prism++ content pipeline ///
///                                                                                       ///
/// DO NOT EDIT THIS FILE                                                                 ///
///  Edit the original file and then re-run PrismTool to update this file                 ///
/////////////////////////////////////////////////////////////////////////////////////////////
";
		/// <summary>
		/// The extension which will be applied to export files
		/// </summary>
		[CommandLineArgument(Name = "export-ext", Usage = "The extention all exported files will prepend to thier current", MustExist = false)]
		protected string m_ExportExtension = ".refl";

		/// <summary>
		/// File information about a 
		/// </summary>
		public class ExportFile
		{
			public enum FileState
			{
				New,
				Updated,
				Removed
			}

			/// <summary>
			/// The full path to this file
			/// </summary>
			public string Path;

			/// <summary>
			/// Is this file an include file
			/// </summary>
			public bool IsInclude;

			/// <summary>
			/// The state of this particular file
			/// </summary>
			public FileState State;
		}

		/// <summary>
		/// The reflection behaviour controller for thie reflector
		/// </summary>
		private BehaviourController m_BehaviourController = new BehaviourController();

		/// <summary>
		/// Where any persistant build data can be stored
		/// </summary>
		public abstract string IntermediateFolder { get; }

		private DateTime m_LastBuildTime;
		private bool m_FetchedBuildTime;

		private DateTime m_LastToolTime;
		private bool m_FetchedToolTime;

		/// <summary>
		/// The last time these files were built
		/// </summary>
		public DateTime LastBuildTime
		{
			get
			{
				if (!m_FetchedBuildTime)
				{
					string buildFile = Path.Combine(IntermediateFolder, "build");
					if (File.Exists(buildFile))
						m_LastBuildTime = File.GetLastWriteTimeUtc(buildFile);
					else
						m_LastBuildTime = DateTime.FromFileTimeUtc(0);

					m_FetchedBuildTime = true;
				}

				return m_LastBuildTime;
			}
			set
			{
				string buildFile = Path.Combine(IntermediateFolder, "build");

				if (!Directory.Exists(IntermediateFolder))
					Directory.CreateDirectory(IntermediateFolder);

				if (File.Exists(buildFile))
					File.SetLastWriteTimeUtc(buildFile, value);
				else
					File.Create(buildFile);

				m_LastBuildTime = value;
			}
		}

		/// <summary>
		/// The last time this tool was updated
		/// </summary>
		public DateTime LastToolUpdateTime
		{
			get
			{
				if (!m_FetchedToolTime)
				{
					string exePath = Assembly.GetEntryAssembly().Location;
					if (File.Exists(exePath))
						m_LastToolTime = File.GetLastWriteTimeUtc(exePath);
					else
						m_LastToolTime = DateTime.FromFileTimeUtc(0);

					m_FetchedToolTime = true;
				}

				return m_LastToolTime;
			}
		}

		/// <summary>
		/// Runs through each file, as specified by the reflector
		/// </summary>
		/// <returns>The new reflection files which have been generated</returns>
		public abstract List<ExportFile> Run();

		/// <summary>
		/// Runs through the given files and exports any reflection data
		/// </summary>
		/// <returns>The new reflection files which have been generated</returns>
		protected List<ExportFile> RunInternal(ReflectionSettings settings, IEnumerable<string> reflectableFiles, string exportDirectory)
		{
			List<ExportFile> outputFiles = new List<ExportFile>();
			
			Console.WriteLine("Generating Prism Reflection -> " + exportDirectory);
			foreach (var file in reflectableFiles)
			{
#if !DEBUG
				try
#endif
				{
					// Don't read in output directory
					if (!file.Equals(exportDirectory, StringComparison.CurrentCultureIgnoreCase))
					{
						List<ExportFile> exports = ReflectFile(settings, file, exportDirectory);
						outputFiles.AddRange(exports);
					}
				}
#if !DEBUG
				catch (ParseException e)
				{
					throw new HeaderParseException(file, e.ErrorCode, e.Signature, e);
				}
#endif
			}
			Console.WriteLine("Prism Reflection Generated.");

			LastBuildTime = DateTime.UtcNow;
			return outputFiles;
		}

		/// <summary>
		/// Reflect a given input file and return the export information about any new files
		/// </summary>
		/// <returns>All the reflection files which have been created from these</returns>
		protected List<ExportFile> ReflectFile(ReflectionSettings settings, string sourcePath, string exportDirectory)
		{
			List<ExportFile> exports = new List<ExportFile>();
			HeaderParser parser = new HeaderParser(settings);

			// Decide whether to reflect the file
			// Only build if: currently rebuilding everything, file has changed, tool has changed
			DateTime fileWriteTime = File.GetLastWriteTimeUtc(sourcePath);
			if(settings.RebuildEverything || (fileWriteTime > LastBuildTime) || (LastBuildTime < LastToolUpdateTime))
			{
				string includeExportPath = Path.Combine(exportDirectory, Path.GetFileNameWithoutExtension(sourcePath) + m_ExportExtension + Path.GetExtension(sourcePath));
				string sourceExportPath = Path.Combine(exportDirectory, Path.GetFileNameWithoutExtension(sourcePath) + m_ExportExtension + ".cpp");

				Console.WriteLine("\tReflecting " + Path.GetFileName(sourcePath) + " -> " + Path.GetFileName(includeExportPath));
				
				// Reflect the file
				using (FileStream stream = new FileStream(sourcePath, FileMode.Open))
				{
					FileToken fileToken = parser.Parse(sourcePath, stream);

					// Perform reflection
					//try
					//{
						m_BehaviourController.ProcessToken(fileToken);
					//}
					//catch (TokenException e)
					//{
					//	// TODO - 
					//	e.Token
					//	throw new HeaderParseException(sourcePath, ParseErrorCode.TokenMissuse, new Parsing.Code.Signatures.SignatureInfo(0, ), e);
					//}

					// If there were no tokens, check if there are artefacts from previous runs (If so, the files need to be wiped)
					if (fileToken.InternalTokens.Count == 0)
					{
						// Check for include refl
						if (File.Exists(includeExportPath))
						{
							ExportFile includeExport = new ExportFile();
							includeExport.IsInclude = true;
							includeExport.State = ExportFile.FileState.Removed;
							includeExport.Path = includeExportPath;

							File.Delete(includeExportPath);
							exports.Add(includeExport);
						}
						// Check for source refl
						if (File.Exists(sourceExportPath))
						{
							ExportFile sourceExport = new ExportFile();
							sourceExport.IsInclude = false;
							sourceExport.State = ExportFile.FileState.Removed;
							sourceExport.Path = sourceExportPath;

							File.Delete(sourceExportPath);
							exports.Add(sourceExport);
						}
					}

					// Export tokens, if any have been found
					else
					{
						/*
						// This has valid tokens to reflect, so check that the includes are present
						bool foundReflInclude = false;
						string requiredPreInclude = includeExportPath.ToLower().Replace('/', '\\');
						int firstTokenLine = file.ParsedTokens.First(). .TokenLineNumber;

						foreach (var fileInclude in file.FileIncludes)
						{
							if (fileInclude.LineNumber > firstTokenLine)
								continue;

							string path = fileInclude.Path.ToLower().Replace('/', '\\');
							if (requiredPreInclude.EndsWith(path))
							{
								foundReflInclude = true;
								break;
							}
						}

						if (!foundReflInclude)
						{
							var token = file.ReflectedTokens[0];
							SignatureInfo fakeSig = new SignatureInfo(token.TokenLineNumber, "", SignatureInfo.SigType.Unknown);
							throw new ReflectionException(ReflectionErrorCode.ParseExpectedInclude, fakeSig, "Expected include to '" + Path.GetFileName(includeExportPath) + "' to appear before first token.");
						}
						*/
						
						// Export results
						ReflectionExporter exporter = new ReflectionExporter(fileToken, settings);

						string includeContent = exporter.GenerateIncludeContent().ToString();
						string sourceContent = exporter.GenerateSourceContent().ToString();
						
						// Fix line-endings
						includeContent = ConditionState.PreExpandDirectives(includeContent).Replace("\r\n", "\n").Replace("\n", "\r\n");
						sourceContent = ConditionState.PreExpandDirectives(sourceContent).Replace("\r\n", "\n").Replace("\n", "\r\n");
						

						// Export header
						{
							ExportFile includeExport = new ExportFile();
							string includeExportDir = Path.GetDirectoryName(includeExportPath);

							if (!Directory.Exists(includeExportDir))
								Directory.CreateDirectory(includeExportDir);

							includeExport.IsInclude = true;
							includeExport.State = !File.Exists(includeExportPath) ? ExportFile.FileState.New : ExportFile.FileState.Updated;
							includeExport.Path = includeExportPath;

							File.WriteAllText(includeExportPath, includeContent);
							exports.Add(includeExport);
						}

						// Export source
						{
							ExportFile sourceExport = new ExportFile();
							string sourceExportDir = Path.GetDirectoryName(sourceExportPath);

							if (!Directory.Exists(sourceExportDir))
								Directory.CreateDirectory(sourceExportDir);

							sourceExport.IsInclude = false;
							sourceExport.State = !File.Exists(sourceExportDir) ? ExportFile.FileState.New : ExportFile.FileState.Updated;
							sourceExport.Path = sourceExportPath;

							File.WriteAllText(sourceExportPath, sourceContent);
							exports.Add(sourceExport);
						}
					}
				}
			}

			return exports;
		}
	}
}
