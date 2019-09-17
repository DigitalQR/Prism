using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Utils
{
	public static class TokenUtils
	{
		private static char[] s_ContainerOpens = new char[] { '(', '[', '<' };
		private static char[] s_ContainerCloses = new char[] { ')', ']', '>' };

		/// <summary>
		/// Reads until all syntax and split into array
		/// </summary>
		public static string[] SplitSyntax(string source, IEnumerable<char> splitters = null)
		{
			List<string> outputs = new List<string>();

			while (!string.IsNullOrWhiteSpace(source) && NextSyntax(ref source, splitters, out string syntax))
			{
				if (!string.IsNullOrWhiteSpace(syntax))
				{
					// Append blocks together if that makes sense
					if (outputs.Any() && s_ContainerOpens.Contains(syntax.First()))
					{
						outputs[outputs.Count - 1] += syntax.Trim();
					}
					else
					{
						outputs.Add(syntax.Trim());
					}
				}
			}

			return outputs.ToArray();
		}

		/// <summary>
		/// Reads until a valid syntax split has been found
		/// *Modifies source with remaining content
		/// </summary>
		private static bool NextSyntax(ref string content, IEnumerable<char> splitters, out string syntax)
		{
			int depth = 0;
			bool foundStart = false;
			bool readingString = false;

			char c = '\0';
			char prevC = '\0';

			for (int i = 0; i < content.Length; ++i)
			{
				prevC = c;
				c = content[i];

				if (c == '"')
				{
					foundStart = true;

					if (readingString && prevC != '\\')
						readingString = false;
					else
						readingString = true;
				}
				else if (!readingString)
				{ 
					if (s_ContainerOpens.Contains(c))
					{
						foundStart = true;
						++depth;
					}
					else if (s_ContainerCloses.Contains(c))
					{
						foundStart = true;
						--depth;
					}
					else if (depth == 0)
					{
						bool isSplit = (splitters == null) ? char.IsWhiteSpace(c) : splitters.Contains(c);
						if (isSplit)
						{
							if (foundStart)
							{
								syntax = content.Substring(0, i);
								content = (content.Length > i) ? content.Substring(i + 1) : "";
								return true;
							}
						}
						else
						{
							foundStart = true;
						}
					}
				}
			}
			
			syntax = content;
			content = "";
			return !string.IsNullOrWhiteSpace(syntax);
		}
	}
}
