using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	/// <summary>
	/// Required data for all reflection tokens
	/// </summary>
	public abstract class TokenReflection
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
		/// Any params which has been desired to be passed to this token
		/// </summary>
		private string[] m_TokenParams;

		public TokenReflection(ConditionState conditionState, string tokenParams, string docString)
		{
			m_PreProcessorCondition = conditionState.CurrentCondition;
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
			protected set { m_PreProcessorCondition = value; }
		}

		/// <summary>
		/// The doc-string associated with this token
		/// </summary>
		public string DocString
		{
			get { return m_DocString; }
		}

		/// <summary>
		/// Any params that this token has been detected with
		/// </summary>
		public string[] TokenParams
		{
			get { return m_TokenParams; }
		}
	}
}
