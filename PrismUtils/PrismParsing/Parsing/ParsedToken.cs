using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing
{
	/// <summary>
	/// Required data for all reflection tokens
	/// </summary>
	public abstract class ParsedToken
	{
		/// <summary>
		/// The compilation condition which this structure exist under
		/// </summary>
		private string m_PreProcessorCondition;
		
		
		/// <summary>
		/// Any doc string which is associated with this token
		/// </summary>
		private string m_DocString;

		/// <summary>
		/// The file that this token exists in
		/// </summary>
		private string m_TokenFilePath;

		/// <summary>
		/// The line that the macro token for was place on
		/// </summary>
		private int m_TokenLineNumber;

		/// <summary>
		/// Any params which has been desired to be passed to this token
		/// </summary>
		private string[] m_TokenParams;

		public ParsedToken(ConditionState conditionState, string tokenFile, int tokenLine, string tokenParams, string docString)
		{
			m_PreProcessorCondition = conditionState.CurrentCondition;
			if (m_PreProcessorCondition == "")
				m_PreProcessorCondition = "1";

			m_TokenFilePath = tokenFile;
			m_TokenLineNumber = tokenLine;
			m_DocString = docString;

			// Parse the token params (Taking into account that "",(),{},[] and <> should be considered as sub structure, hence not split)
			{
				List<string> parsedParams = new List<string>();
				string currentParam = "";
				int blockDepth = 0;
				bool inString = false;
				bool inChar = false;

				char lastC;
				char c = '\0';

				for (int i = 0; i < tokenParams.Length; ++i)
				{
					lastC = c;
					c = tokenParams[i];

					if (blockDepth == 0)
					{
						// End of param
						if (c == ',')
						{
							currentParam = currentParam.Trim();
							if (currentParam != "")
								parsedParams.Add(currentParam);

							currentParam = "";
							continue;
						}
						else if (!inChar && c == '"' && lastC != '\\')
						{
							inString = !inString;
						}
						else if (!inString && c == '\'' && lastC != '\\')
						{
							inChar = !inChar;
						}
					}

					if (!inChar && !inString)
					{
						if (c == '(' || c == '{' || c == '[' || c == '<')
							++blockDepth;
						else if (c == ')' || c == '}' || c == ']' || c == '>')
							blockDepth = Math.Max(blockDepth - 1, 0);
					}

					currentParam += c;
				}

				// Add last param
				currentParam = currentParam.Trim();
				if (currentParam != "")
					parsedParams.Add(currentParam);

				m_TokenParams = parsedParams.ToArray();
			}
		}

		/// <summary>
		/// The pre-processor condition for this token to exist
		/// </summary>
		public string PreProcessorCondition
		{
			get { return m_PreProcessorCondition; }
		}

		/// <summary>
		/// The doc-string associated with this token
		/// </summary>
		public string DocString
		{
			get { return m_DocString; }
		}
		public string SafeDocString
		{
			get { return m_DocString.Replace("\"", "\"\""); }
		}

		/// <summary>
		/// Any params that this token has been detected with
		/// </summary>
		public string[] TokenParams
		{
			get { return m_TokenParams; }
		}
		
		/// <summary>
		/// The line that the macro token for was place on
		/// </summary>
		public int TokenLineNumber
		{
			get { return m_TokenLineNumber; }
		}

		/// <summary>
		/// The the content to place in the header file (Through the macro token)
		/// </summary>
		public abstract string GenerateHeaderReflectionContent();

		/// <summary>
		/// The the content to place in the include reflection file
		/// </summary>
		public abstract string GenerateIncludeReflectionContent();

		/// <summary>
		/// The content to place in the source reflection file
		/// </summary>
		public abstract string GenerateSourceReflectionContent();

		/// <summary>
		/// Generate the C++ side attributes which should be placed in the auto-content
		/// </summary>
		public string GenerateAttributeInstancesString(string usage)
		{
			// TODO - Pre-filter any C# side attributes
			string content = "";

			foreach (string param in m_TokenParams)
			{
				// Convert from format MyThing(x,y,z) -> MyThingAttribute(x,y,z)
				int index = param.IndexOf("(");
				
				string attributeName;
				string attrInstance;

				if (index != -1)
				{
					attributeName = param.Substring(0, index);
					attrInstance = param.Insert(index, "Attribute");
				}
				else
				{
					attributeName = param;
					attrInstance = param + "Attribute()";
				}

				// The error message to display, if this attribute doesn't exist
				string errorMessage = m_TokenFilePath + "(" + m_TokenLineNumber + "): error P" + (int)ParseErrorCode.TokenMissuse + ": Cannot find any reflection info for Attribute '" + attributeName + "'";

				content += "__if_exists(" + attributeName + "Attribute::ClassInfo) {\n";

				// Check if usage matches
				content += "([]() -> const Prism::Attribute* { \n";
				content += "const Prism::Attribute* attr = new " + attrInstance + ";\n"; // TODO - check constructor is valid and handle gracefully

				// Add big comment block to inform users to check the constructor (Cannot check it C++ side)
				content += "//////////////////////////////////////////// \n";
				content += "/// NOTICE TO USER\n";
				content += "/// If a compilation error has occured and brought you herei t is likely you are attempting to call a constructor that does exist \n";
				content += "///   Check the construction of '" + attributeName + "' is valid at:\n";
				content += "///   '" + m_TokenFilePath + " @ Line " + m_TokenLineNumber + "'\n";
				content += "//////////////////////////////////////////// \n";

				content += "PRISM_ASSERT(( (int)attr->GetUsageFlags() & (int)Prism::Attribute::Usage::" + usage + " ) != 0, \"Attribute '" + attributeName + "' cannot be used in this context\", R\"(" + m_TokenFilePath + ")\", " + m_TokenLineNumber + ");\n";
				content += "return attr;\n})(),\n";
				content += "}\n";

				content += "__if_not_exists(" + attributeName + "Attribute::ClassInfo) {";
				content += "__pragma(message(R\"(" + errorMessage + ")\")) }\n";

			}

			return content;
		}
	}
}
