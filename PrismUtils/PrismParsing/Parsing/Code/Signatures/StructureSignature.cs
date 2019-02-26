using Prism.Parsing.Code.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Code.Signatures
{
	/// <summary>
	/// Structure signature to work with both class and struct (Or any other keyword as provided in structureName)
	/// </summary>
	public class StructureSignature
	{
		public class ForwardDeclareData
		{
			public string StructureType;
			public string DeclareName;
		}

		public class ImplementationBeginData
		{
			public struct ParentStructure
			{
				public string Access;
				public string DeclareName;
			}

			public string StructureType;
			public string DeclareName;

			public ParentStructure[] ParentStructures;
			public int ParentCount;
		}

		public class ImplementationEndData
		{
			public string StructureType;
			public string DeclareName;
		}
		
		public static bool TryParse(string structureName, Stack<ImplementationBeginData> structureStack, long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			// Is definately a def
			if (content.StartsWith(structureName))
			{
				// Read until find ; or {
				if (!content.Contains(';') && !content.Contains('{'))
				{
					string newContent;
					if (!reader.SafeReadUntil(new string[] { ";", "{" }, out newContent)) // Invalid def (Let it get caught c++ side, so just send unknown sig)
					{
						sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Failed to find end of " + structureName + " statement.");
						return true;
					}
					content += '\n' + newContent;
				}

				int semiColonIndex = content.IndexOf(';');
				int braceIndex = content.IndexOf('{');
				int activeIndex;
				bool readingForwardDeclare;

				// Decide if reading body of just a forward declare
				if (semiColonIndex == -1)
				{
					readingForwardDeclare = false;
					activeIndex = braceIndex;
				}
				else if (braceIndex == -1)
				{
					readingForwardDeclare = true;
					activeIndex = semiColonIndex;
				}
				else
				{
					if (semiColonIndex < braceIndex)
					{
						readingForwardDeclare = true;
						activeIndex = semiColonIndex;
					}
					else
					{
						readingForwardDeclare = false;
						activeIndex = braceIndex;
					}
				}

				// Get rid of part of line we don't need
				reader.LeftOverContent = content.Substring(activeIndex + 1);
				content = content.Substring(0, activeIndex).Trim();


				if (readingForwardDeclare)
				{
					// Ignore initial parts, as their not needed
					string[] parts = content.Split(' ');


					ForwardDeclareData data = new ForwardDeclareData();
					data.StructureType = structureName;
					data.DeclareName = parts[parts.Length - 1];

					sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.StructureForwardDeclare, data);
					return true;
				}
				else
				{
					ImplementationBeginData data = new ImplementationBeginData();
					data.StructureType = structureName;

					// Parse basic structure info
					// Read until first : (Or end of string
					string lhsDecleration = "";
					string rhsDecleration = "";
					foreach (char c in content)
					{
						if (c != ':')
						{
							lhsDecleration += c;
						}
						else
						{
							break;
						}
					}

					rhsDecleration = content.Substring(lhsDecleration.Length).Replace('\n', ' ').Trim();
					lhsDecleration = lhsDecleration.Replace('\n', ' ').Trim();

					// Find out class name
					{
						string[] parts = lhsDecleration.Split();
						data.DeclareName = parts[parts.Length - 1];
					}

					// Find out parent information
					if (rhsDecleration != "")
					{
						rhsDecleration = rhsDecleration.Substring(1);

						List<ImplementationBeginData.ParentStructure> parents = new List<ImplementationBeginData.ParentStructure>();

						foreach (string parentInfo in rhsDecleration.Split(','))
						{
							ImplementationBeginData.ParentStructure parentData = new ImplementationBeginData.ParentStructure();

							// Don't look for access specifiers for enums
							if (structureName == "enum")
							{
								parentData.Access = "";
								parentData.DeclareName = parentInfo.Trim();
							}
							else
							{
								string[] parts = parentInfo.Trim().Split(' ');
								if (parts.Length == 0) // Private inheritance
								{
									parentData.Access = "private";
									parentData.DeclareName = parts[0];
								}
								else
								{
									parentData.Access = parts[0];
									parentData.DeclareName = parts[parts.Length - 1];
								}
							}

							parents.Add(parentData);
						}

						data.ParentCount = parents.Count;
						data.ParentStructures = parents.ToArray();
					}
					else
					{
						data.ParentCount = 0;
						data.ParentStructures = null;
					}

					sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.StructureImplementationBegin, data);
					structureStack.Push(data);
					return true;
				}
			}

			// Check if it is the end of a structure
			else if (structureStack.Count != 0 && content.StartsWith("}"))
			{
				bool foundSemiColon = true;

				// Read until find ;
				if (!content.Contains(';'))
				{
					string newContent;
					if (!reader.SafeReadUntil(";", out newContent)) // Couldn't find end of structure (Probably wasn't meant for us?)
					{
						reader.LeftOverContent = content.Substring(1) + '\n' + newContent;
						foundSemiColon = false;
					}
					else
					{
						content += '\n' + newContent;
					}
				}

				// Is end of structure?
				if (foundSemiColon)
				{
					content = content.Substring(1).Trim();
					if (content.StartsWith(";"))
					{
						reader.LeftOverContent = content.Substring(1);

						ImplementationBeginData beginData = structureStack.Pop();

						ImplementationEndData endData = new ImplementationEndData();
						endData.StructureType = beginData.StructureType;
						endData.DeclareName = beginData.DeclareName;

						sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.StructureImplementationEnd, endData);
						return true;
					}
				}
			}

			sigInfo = null;
			return false;
		}
	}
}
