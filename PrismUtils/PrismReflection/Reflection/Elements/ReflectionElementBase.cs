using Prism.Reflection.Behaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements
{
	public struct ReflectionMetaData
	{
		public ElementOrigin m_Origin;
		public string[] m_Namespace;
		public string m_Documentation;
		public string m_PreProcessorCondition;
		public ReflectionState m_DiscoveryState;

		public static ReflectionMetaData NonParsedInfo
		{
			get
			{
				return new ReflectionMetaData
				{
					m_Origin = new ElementOrigin(0),
					m_Namespace = new string[0],
					m_Documentation = "",
					m_PreProcessorCondition = null,
					m_DiscoveryState = ReflectionState.Unknown
				};
			}
		}
	}
	
	/// <summary>
	/// Common defaults for reflection elements
	/// </summary>
	public abstract class ReflectionElementBase : AttributeCollection, IReflectionElement
	{
		private BehaviourTarget m_SupportedTargets;
		private ReflectionMetaData m_MetaData;
		private IReflectionElement m_Parent;

		private StringBuilder m_IncludeContent;
		private StringBuilder m_InlineContent;
		private StringBuilder m_SourceContent;

		public ReflectionElementBase(BehaviourTarget supported, string[] attributes, ReflectionMetaData metaData)
			: base(attributes)
		{
			m_SupportedTargets = supported;
			m_MetaData = metaData;

			m_IncludeContent = new StringBuilder();
			m_InlineContent = new StringBuilder();
			m_SourceContent = new StringBuilder();
		}

		public abstract string Name { get; }
		public virtual string UniqueName { get => Name + "_" + Origin.LineNumber.ToString("X"); }

		public BehaviourTarget SupportedTargets { get => m_SupportedTargets; }
		public ElementOrigin Origin { get => m_MetaData.m_Origin; }
		public string Documentation { get => m_MetaData.m_Documentation; }
		public string[] Namespace { get => m_MetaData.m_Namespace; }
		public string PreProcessorCondition { get => m_MetaData.m_PreProcessorCondition == null ? "1" : m_MetaData.m_PreProcessorCondition; }
		public ReflectionState DiscoveryState { get => m_MetaData.m_DiscoveryState; }
		public IReflectionElement ParentElement { get => m_Parent; set => m_Parent = value; }
		public virtual IEnumerable<IReflectionElement> ChildElements { get => new IReflectionElement[0]; }
		
		public ReflectionElementBase AppendIncludeContent(string format)
		{
			m_IncludeContent.Append(format);
			return this;
		}
		public ReflectionElementBase AppendIncludeContent(string format, params object[] args)
		{
			m_IncludeContent.AppendFormat(format, args);
			return this;
		}
		public virtual string GenerateIncludeContent()
		{
			return m_IncludeContent.ToString();
		}

		public ReflectionElementBase AppendInlineContent(string format)
		{
			if (format.Contains("\n#"))
				throw new ReflectionException("Found # in inlined block");

			m_InlineContent.Append(format);
			return this;
		}
		public ReflectionElementBase AppendInlineContent(string format, params object[] args)
		{
			m_InlineContent.AppendFormat(format, args);
			return this;
		}
		public virtual string GenerateInlineContent()
		{
			return m_InlineContent.ToString();
		}

		public ReflectionElementBase AppendSourceContent(string format)
		{
			m_SourceContent.Append(format);
			return this;
		}
		public ReflectionElementBase AppendSourceContent(string format, params object[] args)
		{
			m_SourceContent.AppendFormat(format, args);
			return this;
		}
		public virtual string GenerateSourceContent()
		{
			return m_SourceContent.ToString();
		}
	}
}
