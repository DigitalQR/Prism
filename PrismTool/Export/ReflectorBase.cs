﻿using Prism.CodeParsing;
using Prism.Reflection;
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
		[CmdArg(Arg = "export-ext", Usage = "The extention all exported files will prepend to thier current", MustExist = false)]
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
				try
				{
					// Don't read in output directory
					if (!file.Equals(exportDirectory, StringComparison.CurrentCultureIgnoreCase))
					{
						List<ExportFile> exports = ReflectFile(settings, file, exportDirectory);
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

		/// <summary>
		/// Reflect a given input file and return the export information about any new files
		/// </summary>
		/// <returns>All the reflection files which have been created from these</returns>
		protected List<ExportFile> ReflectFile(ReflectionSettings settings, string sourcePath, string exportDirectory)
		{
			List<ExportFile> exports = new List<ExportFile>();

			// Decide whether to reflect the file
			// Only build if: currently rebuilding everything, file has changed, tool has changed
			DateTime fileWriteTime = File.GetLastWriteTimeUtc(sourcePath);
			if(settings.RebuildEverything || (fileWriteTime > LastBuildTime) || (LastBuildTime < LastToolUpdateTime))
			{
				string includeExportPath = Path.Combine(exportDirectory, Path.GetFileNameWithoutExtension(sourcePath) + m_ExportExtension + Path.GetExtension(sourcePath));
				string sourceExportPath = Path.Combine(exportDirectory, Path.GetFileNameWithoutExtension(sourcePath) + m_ExportExtension + ".cpp");

				Console.WriteLine("\tReflecting " + Path.GetFileName(sourcePath) + " -> " + Path.GetFileName(includeExportPath));

				string includeContent = "";
				string sourceContent = "";

				includeContent = @"%FILE_EXPORT_HEADER%
#pragma once
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
				sourceContent = @"%FILE_EXPORT_HEADER%
#include ""%FILE_PATH%""
";

				// Resolve placeholders
				includeContent = includeContent
					.Replace("%FILE_EXPORT_HEADER%", s_ExportHeader)
					.Replace("%CLASS_TOKEN%", settings.ClassToken)
					.Replace("%STRUCT_TOKEN%", settings.StructToken)
					.Replace("%ENUM_TOKEN%", settings.EnumToken)
					.Replace("%FUNCTION_TOKEN%", settings.FunctionToken)
					.Replace("%VARIABLE_TOKEN%", settings.VariableToken);

				sourceContent = sourceContent
					.Replace("%FILE_EXPORT_HEADER%", s_ExportHeader)
					.Replace("%FILE_PATH%", sourcePath);

				// Reflect the file
				using (FileStream stream = new FileStream(sourcePath, FileMode.Open))
				{
					HeaderReflection file = HeaderReflection.Generate(settings, sourcePath, stream);

					// If there were no tokens, check if there are artefacts from previous runs (If so, the files need to be wiped)
					if (file.ReflectedTokenCount == 0)
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
						// This has valid tokens to reflect, so check that the includes are present
						bool foundReflInclude = false;
						string requiredPreInclude = includeExportPath.ToLower().Replace('/', '\\');
						int firstTokenLine = file.ReflectedTokens[0].TokenLineNumber;

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


						// Generate the reflection export data for all tokens found
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


							// Add line breaks to make debugging easier on the eyes
							includeContent += "\n//=========== TOKEN " + token.TokenLineNumber + " START ===========//\n" + finalInclude + "\n//=========== TOKEN " + token.TokenLineNumber + " END ===========//\n";
							sourceContent += "\n//=========== TOKEN " + token.TokenLineNumber + " START ===========//\n" + finalSource + "\n//=========== TOKEN " + token.TokenLineNumber + " END ===========//\n";
						}


						// Fix line-endings
						includeContent = PreExpandDirectives(includeContent).Replace("\r\n", "\n").Replace("\n", "\r\n");
						sourceContent = PreExpandDirectives(sourceContent).Replace("\r\n", "\n").Replace("\n", "\r\n");


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

		private struct DirectiveState
		{
			public bool RecognisedStatement;
			public bool CurrentWrite;
		};

		/// <summary>
		/// Attempt to pre-expande any simple directives
		/// </summary>
		/// <returns></returns>
		private string PreExpandDirectives(string raw)
		{
			string output = "";
			
			Stack<DirectiveState> stateStack = new Stack<DirectiveState>();

			DirectiveState currentState = new DirectiveState();
			currentState.RecognisedStatement = true;
			currentState.CurrentWrite = true;

			foreach (string line in raw.Split('\n'))
			{
				string searchLine = line.Trim();
				if (searchLine.StartsWith("#if"))
				{
					stateStack.Push(currentState);

					if (searchLine == "#if 1")
					{
						currentState.RecognisedStatement = true;
						currentState.CurrentWrite = currentState.CurrentWrite && true;
						continue;
					}
					else if (searchLine == "#if 0")
					{
						currentState.RecognisedStatement = true;
						currentState.CurrentWrite = false;
						continue;
					}
					else
					{
						currentState.RecognisedStatement = false;
						currentState.CurrentWrite = true;
					}
				}
				else
				{
					if (searchLine.StartsWith("#elif "))
					{
						if (currentState.RecognisedStatement)
						{
							if (currentState.CurrentWrite)
							{ 
								currentState.CurrentWrite = false;
								continue;
							}
							else
							{
								currentState.CurrentWrite = !currentState.CurrentWrite;
								continue;
							}							
						}
					}
					else if (searchLine == "#else")
					{
						if (currentState.RecognisedStatement)
						{
							currentState.CurrentWrite = !currentState.CurrentWrite;

							// Make sure we're allowed to be re-enabling it
							if (stateStack.Count != 0)
							{
								currentState.CurrentWrite = currentState.CurrentWrite && stateStack.Peek().CurrentWrite;
							}

							continue;
						}
					}
					else if (searchLine == "#endif")
					{
						bool skip = currentState.RecognisedStatement;

						currentState = stateStack.Pop();

						if(skip)
							continue;
					}
				}
				
				if (currentState.CurrentWrite)
					output += line + "\n";
			}

			return output;
		}
	}
}
