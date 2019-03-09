using Prism.Reflection.Behaviour;
using Prism.Reflection.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Tokens
{
	/// <summary>
	/// All reflection information found for a file
	/// </summary>
	public class FileToken : ReflectableTokenBase
	{
		/// <summary>
		/// Information about an include statement
		/// </summary>
		public class IncludeStatement
		{
			public string IncludePath { get; private set; }
			public int LineNumber { get; private set; }

			public IncludeStatement(string path, int line)
			{
				IncludePath = path;
				LineNumber = line;
			}
		}
		
		/// <summary>
		/// All the reflected tokens found in this file
		/// </summary>
		private List<IReflectableToken> m_InternalTokens;

		/// <summary>
		/// All the includes found in this file
		/// </summary>
		private List<IncludeStatement> m_Includes;
		

		public FileToken(string fileSource)
			: base(new TokenOrigin(fileSource, 0), BehaviourTarget.File)
		{
			m_InternalTokens = new List<IReflectableToken>();
			m_Includes = new List<IncludeStatement>();
		}

		/// <summary>
		/// All tokens which have been discovered in this header
		/// </summary>
		public override IReadOnlyList<IReflectableToken> InternalTokens => m_InternalTokens;

		/// <summary>
		/// All includes which have been discovered in this header
		/// </summary>
		public IReadOnlyList<IncludeStatement> Includes => m_Includes;

		/// <summary>
		/// Add a token which was discovered in this file
		/// </summary>
		public void AddInternalToken(IReflectableToken token)
		{
			// TODO - Verify token is allowed here?
			m_InternalTokens.Add(token);
		}

		/// <summary>
		/// Add an include information that was found for this file
		/// </summary>
		public void AddInternalInclude(IncludeStatement include)
		{
			m_Includes.Add(include);
		}

		public override StringBuilder GenerateIncludeContent(IReflectableToken context)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(@"#pragma once
#include <Prism.h>

#ifndef NO_REFL_$(UniqueSourceName)
#define REFL_$(UniqueSourcePath)

///
/// DEFINE SUPPORTED TOKENS
///

#ifdef PRISM_DEFER
#undef PRISM_DEFER
#endif
#define PRISM_DEFER(...) __VA_ARGS__

#ifdef $(ClassReflectionToken)
#undef $(ClassReflectionToken)
#endif
#define $(ClassReflectionToken)(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef $(StructReflectionToken)
#undef $(StructReflectionToken)
#endif
#define $(StructReflectionToken)(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef $(EnumReflectionToken)
#undef $(EnumReflectionToken)
#endif
#define $(EnumReflectionToken)(...) PRISM_DEFER(PRISM_REFLECTION_BODY_) ## __LINE__

#ifdef $(FunctionReflectionToken)
#undef $(FunctionReflectionToken)
#endif
#define $(FunctionReflectionToken)(...)

#ifdef $(VariableReflectionToken)
#undef $(VariableReflectionToken)
#endif
#define $(VariableReflectionToken)(...)

///////////////////////////////////////
");
			builder.Append(base.GenerateIncludeContent(context));

			foreach (IReflectableToken token in InternalTokens)
			{
				builder.Append(token.GenerateIncludeContent(this));
				builder.Append(@"
///////////////////////////////////////
");
			}

			builder.Append("\n#endif\n");
			return ExpandMacros(builder);
		}
		
		public override StringBuilder GenerateImplementationContent(IReflectableToken context)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(@"#include ""$(SourceFilePath)""
#ifdef REFL_$(UniqueSourcePath)
");
			builder.Append(base.GenerateImplementationContent(context));


			foreach (IReflectableToken token in InternalTokens)
			{
				builder.Append(token.GenerateImplementationContent(this));
				builder.Append(@"
///////////////////////////////////////
");
			}

			builder.Append("\n#endif\n");
			return ExpandMacros(builder);
		}

		public override StringBuilder GenerateDeclarationContent(IReflectableToken context)
		{
			throw new InvalidOperationException("Cannot call GenerateDeclarationContent for FileToken");
		}

		/// <summary>
		/// Convert a path into a code-safe token
		/// </summary>
		public static string MakePathSafe(string path)
		{
			return path.Replace(' ', '_')
					.Replace('-', '_')
					.Replace('.', '_')
					.Replace(':', '_')
					.Replace('\\', '_')
					.Replace('/', '_');
		}

		/// <summary>
		/// Expand any macros relating to this type (Missing macros will be left)
		/// $(SourceFileName)
		/// $(SourceFilePath)
		/// $(UniqueSourceName)
		/// $(UniqueSourcePath)
		/// </summary>
		/// <param name="input">The raw input string which should have it's macros replaced</param>
		/// <param name="prefix">The prefix to apply to every macro</param>
		/// <param name="suffix">The suffix to apply to each macro</param>
		/// <returns>The string with all relevent macros expanded</returns>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			builder.Replace("$(" + prefix + "SourceFileName" + suffix + ")", Path.GetFileName(Origin.FilePath));
			builder.Replace("$(" + prefix + "SourceFilePath" + suffix + ")", Origin.FilePath);
			builder.Replace("$(" + prefix + "UniqueSourceName" + suffix + ")", MakePathSafe(Path.GetFileName(Origin.FilePath)));
			builder.Replace("$(" + prefix + "UniqueSourcePath" + suffix + ")", MakePathSafe(Origin.FilePath));
			return builder;
		}

	}
}
