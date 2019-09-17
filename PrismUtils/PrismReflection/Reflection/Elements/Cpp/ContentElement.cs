using Prism.Reflection.Behaviour;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp
{
	/// <summary>
	/// All reflected information found for a specific cpp source
	/// </summary>
	public class ContentElement : ReflectionElementBase
	{
		private string m_FileName;
		private TokenSettings m_Settings;
		private List<IReflectionElement> m_InternalElements;

		public ContentElement(string sourceFile, TokenSettings settings)
			: base(BehaviourTarget.Content, new string[0], ReflectionMetaData.NonParsedInfo)
		{
			m_FileName = sourceFile;
			m_Settings = settings;
			m_InternalElements = new List<IReflectionElement>();
		}

		public override string Name { get => Path.GetFileNameWithoutExtension(m_FileName).Replace(" ", "_"); }
		public string FileName { get => m_FileName; }
		public TokenSettings Settings { get => m_Settings; }
		public IEnumerable<IReflectionElement> InternalElements { get => m_InternalElements; }
		public override IEnumerable<IReflectionElement> ChildElements { get => m_InternalElements; }

		public void AppendElement(IReflectionElement element)
		{
			m_InternalElements.Add(element);
			element.ParentElement = this;
		}

		public override string GenerateIncludeContent()
		{
			StringBuilder builder = new StringBuilder();

			// Place default macros based on tokens
			builder.Append($@"
///
/// Prism Macros
///
#ifdef PRISM_DEFER
#undef PRISM_DEFER
#endif
#define PRISM_DEFER(...) __VA_ARGS__

#ifdef {m_Settings.ClassToken}
#undef {m_Settings.ClassToken}
#endif
#define {m_Settings.ClassToken}(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef {m_Settings.StructToken}
#undef {m_Settings.StructToken}
#endif
#define {m_Settings.StructToken}(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef {m_Settings.EnumToken}
#undef {m_Settings.EnumToken}
#endif
#define {m_Settings.EnumToken}(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef {m_Settings.ConstructorToken}
#undef {m_Settings.ConstructorToken}
#endif
#define {m_Settings.ConstructorToken}(...) 

#ifdef {m_Settings.FunctionToken}
#undef {m_Settings.FunctionToken}
#endif
#define {m_Settings.FunctionToken}(...)

#ifdef {m_Settings.VariableToken}
#undef {m_Settings.VariableToken}
#endif
#define {m_Settings.VariableToken}(...)
");


			builder.Append(base.GenerateIncludeContent());
			builder.Append(base.GenerateInlineContent()); // Put inline content here too

			foreach (IReflectionElement child in ChildElements)
			{
				string includeContent = child.GenerateIncludeContent();
				string inlineContent = child.GenerateInlineContent();
				if (includeContent != "" || inlineContent != "")
				{
					builder.Append(GenerationUtils.GetInternalMarker(child));
					builder.Append(includeContent);
					builder.Append(GenerationUtils.GetInlineBlock(child, inlineContent));
					builder.Append($@"
#ifdef PRISM_REFLECTION_BODY_{child.Origin.LineNumber}
#undef PRISM_REFLECTION_BODY_{child.Origin.LineNumber}
#endif
#define PRISM_REFLECTION_BODY_{child.Origin.LineNumber} {GenerationUtils.GetInlineBlockName(child)}
");
				}
			}

			return builder.ToString();
		}

		public override string GenerateInlineContent()
		{
			throw new NotImplementedException();
		}

		public override string GenerateSourceContent()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(base.GenerateSourceContent());

			foreach (IReflectionElement child in ChildElements)
			{
				string content = child.GenerateSourceContent();
				if (content != "")
				{
					builder.Append(GenerationUtils.GetInternalMarker(child));
					builder.Append(content);
				}
			}

			return builder.ToString();
		}
	}
}
