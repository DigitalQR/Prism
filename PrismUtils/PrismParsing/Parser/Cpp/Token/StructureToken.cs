using Prism.Reflection.Elements.Cpp.Data;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	public class StructureToken : CppTokenType<StructureToken.ParsedData>
	{
		internal static string[] s_StructureTypes = new string[] { "class", "struct", "interface" };
		internal static string[] s_AccessorTypes = new string[] { "public", "private", "protected" };
		
		public class ParsedData
		{
			public StructureInfo m_StructureInfo;
		}
		
		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			if (input.EndsWith("{"))
			{
				string check = input.Substring(0, input.Length - 1).Replace('\t', ' ').Replace('\n', ' ');
				string lhs = null;
				string rhs = null;
				int splitIndex = input.IndexOf(':');
				if (splitIndex != -1)
				{
					lhs = check.Substring(0, splitIndex).Trim();
					rhs = check.Substring(splitIndex + 1).Trim();
				}
				else
				{
					lhs = check.Trim();
				}

				// Check if definately structure
				string[] lhsParts = lhs.Split(' ');
				if (lhsParts.Length > 1 && s_StructureTypes.Contains(lhsParts[0]))
				{
					StructureInfo structInfo = new StructureInfo();
					structInfo.m_Structure = lhsParts[0];
					structInfo.m_Name = lhsParts.Last().Trim();

					// Parse inheritance info
					if (!string.IsNullOrWhiteSpace(rhs))
					{
						List<InheritanceInfo> parents = new List<InheritanceInfo>();
						string[] inheritParts = TokenUtils.SplitSyntax(rhs, ",");

						foreach (string rawParent in inheritParts)
						{
							InheritanceInfo info = new InheritanceInfo();
							string[] parentPaths = TokenUtils.SplitSyntax(rawParent);

							if (parentPaths.Length > 1)
							{
								info.m_Accessor = parentPaths[0];
								info.m_Name = parentPaths[1];
							}
							else
							{
								info.m_Name = rawParent.Trim();
							}

							parents.Add(info);
						}

						structInfo.m_Parents = parents.ToArray();
					}
					else
					{
						structInfo.m_Parents = new InheritanceInfo[0];
					}

					data = new ParsedData { m_StructureInfo = structInfo };
					return true;
				}
			}

			data = null;
			return false;
		}
	}

	public class AccessorToken : CppTokenType<AccessorToken.ParsedData>
	{
		public class ParsedData
		{
			public string m_Name;
		}

		protected override bool Accept(CppTokenReader reader, ref string input, out ParsedData data)
		{
			foreach (string accessor in StructureToken.s_AccessorTypes)
			{
				string check = accessor + ":";
				if (input.StartsWith(check))
				{
					input = check;
					data = new ParsedData { m_Name = accessor };
					return true;
				}
			}

			data = null;
			return false;
		}
	}
}
