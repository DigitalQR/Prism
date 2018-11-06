using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	public class TemplateSignature
	{
		public class TemplateParam
		{
			public string ParamInfo;
			public string ParamName;
			public string DefaultValue;
		}

		public class ParseData
		{
			public TemplateParam[] Params;
			public int ParamCount;
		}

		public static bool TryParse(long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			if (content.StartsWith("template<"))
			{
				string searchString = content.Substring("template<".Length);

				List<TemplateParam> paramList = new List<TemplateParam>();
				int i = 0;
				int blockCount = 1;
				string currentParam = "";

				do
				{
					// Keep reading over multiple lines
					if (i >= searchString.Length)
					{
						i = 0;
						if (!reader.SafeReadNext(out searchString)) // Invalid def (Let it get caught c++ side, so just send unknown sig)
						{
							sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Failed to find end of template< ...");
							return true;
						}
					}

					char c = searchString[i];
					if (c == '<')
					{
						++blockCount;
					}
					else if (c == '>')
					{
						--blockCount;
					}

					// Found param, if at end or found unscoped ,
					if ((c == ',' && blockCount == 1) || (c == '>' && blockCount == 0))
					{
						TemplateParam param = new TemplateParam();
						string paramStr = currentParam.Trim();

						// Try to find out
						int equalsIndex = paramStr.IndexOf('=');
						if (equalsIndex != -1)
						{
							param.DefaultValue = paramStr.Substring(equalsIndex + 1).Trim();
							paramStr = paramStr.Substring(0, equalsIndex).Trim();
						}

						// Split typename from template type
						int breakIndex = Math.Max(paramStr.LastIndexOf(' '), paramStr.LastIndexOf('>'));
						param.ParamInfo = paramStr.Substring(0, breakIndex + 1).Trim();
						param.ParamName = paramStr.Substring(breakIndex + 1).Trim();
						

						paramList.Add(param);
						currentParam = "";
					}
					else
					{
						currentParam += c;
					}

					++i;
				} while (blockCount != 0);

				if (i < searchString.Length)
					reader.LeftOverContent = searchString.Substring(i);
				

				ParseData data = new ParseData();
				data.ParamCount = paramList.Count;
				data.Params = (data.ParamCount != 0 ? paramList.ToArray() : null);

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.TemplateDeclare, data);
				return true;
			}
			

			sigInfo = null;
			return false;
		}
	}
}
