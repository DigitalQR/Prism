using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	public class VariableInfo
	{
		public FullTypeInfo TypeInfo;
		public string VariableName;
		public string DefaultValue;
	}

	public class VariableSignature
	{
		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			string searchString = content;
			if (content.EndsWith(";"))
			{
				searchString = content.Substring(0, content.Length - 1).Trim();
			}

			// Try to check if we are currently looking at a variable
			if (!string.IsNullOrEmpty(searchString))
			{
				// Check string has at least 1 space
				if (searchString.Contains(' '))
				{
					// Work backwards to find out variable info
					VariableInfo data = new VariableInfo();

					// Find any default value
					int equalsIndex = searchString.LastIndexOf('=');
					if (equalsIndex != -1)
					{
						data.DefaultValue = searchString.Substring(equalsIndex + 1).Trim();
						searchString = searchString.Substring(0, equalsIndex).Trim();
					}
					else
					{
						data.DefaultValue = null;
					}

					// Remove bit packing if present (e.g. bool myBool : 1;)
					// Find : but make sure it's not following ::
					string tempString = searchString.Replace("::", "%ns%");
					int descIndex = tempString.LastIndexOf(':');
					if (descIndex != -1)
					{
						searchString = tempString.Substring(0, descIndex).Replace("%ns%", "::").Trim();
					}

					// Find variable name (Will appear after last ' ','>','*' or '&'
					int splitIndex = Math.Max(searchString.LastIndexOf(' '), Math.Max(searchString.LastIndexOf('>'), Math.Max(searchString.LastIndexOf('*'), searchString.LastIndexOf('&'))));
					if (splitIndex != -1)
					{
						data.VariableName = searchString.Substring(splitIndex + 1).Trim();

						string typeString = searchString.Substring(0, splitIndex + 1).Trim();

						// If variable name contains (), it was actually a function def
						if (TypeSignature.TryGetFullTypeInfo(typeString, out data.TypeInfo))
						{
							// If variable name contains (), it was actually a function def
							if (!data.VariableName.Contains("(") && !data.VariableName.Contains(")") && data.VariableName != "const" && data.VariableName != "override")
							{
								sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.VariableDeclare, data);
								return true;
							}
						}
					}
				}
			}

			sigInfo = null;
			return false;
		}
	}
}
