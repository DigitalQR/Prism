using Prism.Parsing;
using Prism.Reflection;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Prism.Export
{
	/// <summary>
	/// Reflects all files from a project into an output project
	/// </summary>
	public class VisualStudioReflector : ReflectorBase
	{
		/// <summary>
		/// Where all the source files will be read from
		/// </summary>
		[CommandLineArgument(Name = "src-vsproj", Usage = "The project with all the files to reflect", MustExist = true)]
		private string m_SourceProject;

		/// <summary>
		/// Where all the output files will be placed
		/// </summary>
		[CommandLineArgument(Name = "out-dir", Usage = "The directory where output files will be placed", MustExist = false)]
		private string m_OutputDirectory = null;

		/// <summary>
		/// Where all the reflection files will be outputed to
		/// </summary>
		[CommandLineArgument(Name = "out-vsproj", Usage = "The project where all reflection files should be placed", MustExist = false)]
		private string m_OutputProject = null;

		/// <summary>
		/// The extensions that are supported
		/// </summary>
		[CommandLineArgument(Name = "parse-ext", Usage = "File extensions which will be read", MustExist = false)]
		private string[] m_WhitelistedExtensions;

		/// <summary>
		/// Reflection settings used by this reflector
		/// </summary>
		private ReflectionSettings m_ReflectionSettings;

		public VisualStudioReflector()
		{
			m_ReflectionSettings = new ReflectionSettings();
			m_WhitelistedExtensions = new string[] { ".h", ".hpp" };

			CommandLineArguments.FillValues(this);
			CommandLineArguments.FillValues(m_ReflectionSettings);

			if (!File.Exists(m_SourceProject))
				throw new ParseException(ParseErrorCode.GenericError, null, "Failed to find source project file");
			
			// Check output location exists and haven't provided multiple
			if (m_OutputDirectory != null && m_OutputProject != null)
				throw new ParseException(ParseErrorCode.GenericError, null, "Cannot contain both --out-dir and --out-vsproj arguments");

			else if (m_OutputDirectory != null)
			{
				// Output directory will be found, so it's fine
				//if (!Directory.Exists(m_OutputDirectory))
				//	throw new ReflectionException(ReflectionErrorCode.GenericError, null, "Failed to find output directory");
			}
			else if (m_OutputProject != null)
			{
				if (!File.Exists(m_OutputProject))
					throw new ParseException(ParseErrorCode.GenericError, null, "Failed to find output project file");
			}
			else
				throw new ParseException(ParseErrorCode.GenericError, null, "Missing required --out-dir argument");
		}

		public string OutputDirectory
		{
			get
			{
				// Output to location in project
				if (m_OutputProject != null)
					return Path.Combine(Path.GetFullPath(Path.GetDirectoryName(m_OutputProject)), "Prism");
				// Output to given location
				else
					return Path.GetFullPath(m_OutputDirectory);

			}
		}

		public override string IntermediateFolder
		{
			get { return Path.Combine(OutputDirectory, ".prism"); }
		}

		private List<string> FetchReflectableFiles()
		{
			string projectDir = Path.GetDirectoryName(m_SourceProject);
			List<string> files = new List<string>();

			XmlDocument project = new XmlDocument();
			project.Load(m_SourceProject);
			
			// Scan for headers
			foreach (XmlNode includeNode in project.GetElementsByTagName("ClInclude"))
			{
				if (includeNode.Attributes.Count != 0 && includeNode.Attributes["Include"] != null)
				{
					string filePath = Path.Combine(projectDir, includeNode.Attributes["Include"].Value);
					string ext = Path.GetExtension(filePath);

					// Check is valid header and isn't from reflection
					if (m_WhitelistedExtensions.Contains(ext) && !Path.GetFileNameWithoutExtension(filePath).EndsWith(m_ExportExtension, StringComparison.CurrentCultureIgnoreCase))
						files.Add(filePath);
				}
			}

			return files;
		}

		public override List<ExportFile> Run()
		{
			var sourceFiles = FetchReflectableFiles();
			List<ExportFile> files = RunInternal(m_ReflectionSettings, sourceFiles, OutputDirectory);

			if (m_OutputProject != null)
				WriteToProject(files);

			return files;
		}

		private void WriteToProject(List<ExportFile> files)
		{
			string outProjDir = Path.GetDirectoryName(m_OutputProject);

			// TODO - Add these files to filters (Will fail to load if filter file missmatch)
			// Add these files to the export project
			{
				XmlDocument project = new XmlDocument();
				project.Load(m_OutputProject);

				XmlNode rootNode = project.GetElementsByTagName("Project")[0];
				XmlElement reflGroup = null;
				bool requiresChanges = false;

				// Find reflection group node (Or create it)
				{
					// Attempt to fetch the node where we're currently storing all reflected files
					IEnumerable<XmlNode> enumerator = rootNode.ChildNodes.Cast<XmlNode>();
					reflGroup = (XmlElement)enumerator.Where(n => n.LocalName == "ItemGroup" && n.Attributes["Condition"] != null && n.Attributes["Condition"].Value == "'Prism'=='Prism'").FirstOrDefault();

					// Create a new node to store reflected files under
					if (reflGroup == null)
					{
						reflGroup = project.CreateElement("ItemGroup", rootNode.NamespaceURI);
						reflGroup.SetAttribute("Condition", "'Prism'=='Prism'"); // Work around to let us grab this node later

						rootNode.AppendChild(reflGroup);
					}
				}

				// Add new files to project
				foreach (ExportFile file in files.Where(f => f.State == ExportFile.FileState.New))
				{
					XmlElement fileNode;

					// Create new entry for this reflection
					if (file.IsInclude)
						fileNode = project.CreateElement("ClInclude", reflGroup.NamespaceURI);
					else
						fileNode = project.CreateElement("ClCompile", reflGroup.NamespaceURI);

					string localPath;
					if (file.Path.StartsWith(outProjDir, StringComparison.CurrentCultureIgnoreCase))
						localPath = file.Path.Substring(outProjDir.Length + 1);
					else
						localPath = file.Path;

					fileNode.SetAttribute("Include", localPath);
					reflGroup.AppendChild(fileNode);
					requiresChanges = true;
				}

				// Remove old files from project
				foreach (ExportFile file in files.Where(f => f.State == ExportFile.FileState.Removed))
				{
					XmlNodeList searchList;

					// Create new entry for this reflection
					if (file.IsInclude)
						searchList = reflGroup.GetElementsByTagName("ClInclude");
					else
						searchList = reflGroup.GetElementsByTagName("ClCompile");

					string localPath;
					if (file.Path.StartsWith(outProjDir, StringComparison.CurrentCultureIgnoreCase))
						localPath = file.Path.Substring(outProjDir.Length + 1);
					else
						localPath = file.Path;

					// Search for node to remove
					foreach (XmlNode node in searchList)
					{
						if (node.Attributes["Include"] != null && node.Attributes["Include"].Value == localPath)
						{
							reflGroup.RemoveChild(node);
							requiresChanges = true;
							break;
						}
					}
				}

				// Update project on disk, as there was a change
				if (requiresChanges)
				{
					project.Save(m_OutputProject);
					throw new Exception("New files added to project. (Requires reload before build..)");
					// TODO - Handle this better? throw new Exception("New files added to project. (Requires reload before build..)");
				}
			}
		}
	}
}
