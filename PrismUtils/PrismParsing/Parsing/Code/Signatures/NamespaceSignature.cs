using Prism.Parsing.Code.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing.Code.Signatures
{
	public class NamespaceSignature
	{
		/// <summary>
		/// Data for when the active namespace changes
		/// </summary>
		public class ActiveData
		{
			public string[] CurrentNamespace;
			public int NamespaceCount = 0;
		}

		/// <summary>
		/// Data for when a new using namespace call is made
		/// </summary>
		public class UsingData
		{
			public string NamespaceName;
		}

		public static bool TryParse(List<string> namespaceStack, long firstLine, string content, SafeLineReader reader, out SignatureInfo sigInfo)
		{
			// Look for namespace enter
			if (content.StartsWith("namespace "))
			{
				// Keep reading until { is found
				if (!content.Contains('{'))
				{
					string newContent;
					if (!reader.SafeReadUntil("{", out newContent)) // Invalid def (Let it get caught c++ side, so just send unknown sig)
					{
						sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat, "Failed to find namespace block start");
						return true;
					}
					content += '\n' + newContent;
				}

				// Split at {
				int index = content.IndexOf('{');
				reader.LeftOverContent = content.Substring(index + 1);
				content = content.Substring(0, index);

				string namespaceName = content.Substring("namespace ".Length).Trim();
				namespaceStack.Add(namespaceName);

				// Pass on current active namespace
				ActiveData data = new ActiveData();
				data.CurrentNamespace = namespaceStack.ToArray();
				data.NamespaceCount = namespaceStack.Count;

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.NamespaceBegin, data);
				return true;
			}

			// Look for end of active namespace
			else if (content.StartsWith("}"))
			{
				// Assume invalid {} block will be caught elsewhere
				if (namespaceStack.Count != 0)
				{
					// Only care about the bracket, so return rest of the string
					reader.LeftOverContent = content.Substring(1);

					namespaceStack.RemoveAt(namespaceStack.Count - 1);
					
					// Pass on current active namespace
					ActiveData data = new ActiveData();
					data.NamespaceCount = namespaceStack.Count;
					data.CurrentNamespace = (data.NamespaceCount == 0 ? null : namespaceStack.ToArray());

					sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.NamespaceEnd, data);
					return true;
				}
			}

			// Look for using namespace
			else if (content.StartsWith("using namespace "))
			{
				// Keep reading until ; is found
				if (!content.Contains(';'))
				{
					string newContent;
					if (!reader.SafeReadUntil(";", out newContent)) // Invalid def (Let it get caught c++ side, so just send unknown sig)
					{
						sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.InvalidParseFormat);
						return true;
					}
					content += '\n' + newContent;
				}

				// Split at ;
				int index = content.IndexOf(';');
				reader.LeftOverContent = content.Substring(index + 1);
				content = content.Substring(0, index);

				UsingData data = new UsingData();
				data.NamespaceName = content.Substring("using namespace ".Length).Trim();

				sigInfo = new SignatureInfo(firstLine, content, SignatureInfo.SigType.UsingNamespace, "Failed to find using namespace statement end");
				return true;
			}

			sigInfo = null;
			return false;
		}
	}
}
