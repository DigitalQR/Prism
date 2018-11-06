using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing
{
	public class SignatureInfo
	{
		public enum SigType
		{
			Unknown, 
			InvalidParseFormat,

			CommentBlock,
			NamespaceBegin,
			NamespaceEnd,
			UsingNamespace,

			PreProcessorDirective,
			MacroCall,

			StructureForwardDeclare,
			StructureImplementationBegin,
			StructureImplementationEnd,

			AccessorSet,
			EnumValueEntry,
			FriendDeclare,
			TypeDefDeclare,
			TemplateDeclare,

			VariableDeclare
		}

		private SigType m_SigType;

		private string m_LineConent;

		private long m_LineNumber;

		private object m_AdditionalParam;

		public SignatureInfo(long lineNumber, string content, SigType sigType, object additionalParam = null)
		{
			m_LineNumber = lineNumber;
			m_LineConent = content;
			m_SigType = sigType;
			m_AdditionalParam = additionalParam;
		}
		
		public SigType SignatureType { get => m_SigType; }

		public long LineNumber { get => m_LineNumber; }

		public string LineContent { get => m_LineConent; }

		public object AdditionalParam { get => m_AdditionalParam; }
	}
}
