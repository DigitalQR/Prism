using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Tokens
{
	[Flags]
	public enum TypeProperties
	{
		None			= 0,
		Const			= 1,
		Pointer			= 2,
		ConstPointer	= 4,
		Reference		= 8,
		Static			= 16,
		Inline			= 32,
		Mutable			= 64,
		Volatile		= 128,
	}

	public class TypeToken
	{
		private static TypeToken s_VoidType = new TypeToken("void", TypeProperties.None);
		public static TypeToken VoidType { get { return s_VoidType; } }

		private string m_TypeName;
		private TypeProperties m_Properties;

		/// <summary>
		/// C++ compiled typename
		/// </summary>
		public string TypeName
		{
			get { return m_TypeName; }
		}

		/// <summary>
		/// The properties for this type
		/// </summary>
		public TypeProperties Properties
		{
			get { return m_Properties; }
		}

		/// <summary>
		/// Is this type a pointer
		/// </summary>
		public bool IsPointer
		{
			get { return (Properties & (TypeProperties.Pointer | TypeProperties.ConstPointer)) != 0; }
		}

		/// <summary>
		/// Is this type a reference
		/// </summary>
		public bool IsReference
		{
			get { return (Properties & TypeProperties.Reference) != 0; }
		}
		
		/// <summary>
		/// Can this type have it's value changed (i.e. is const or const pointer)
		/// </summary>
		public bool IsConst
		{
			get
			{
				if (IsPointer)
					return (Properties & TypeProperties.ConstPointer) != 0;
				else
					return (Properties & TypeProperties.Const) != 0;
			}
		}

		/// <summary>
		/// Is this type for a static variable
		/// </summary>
		public bool IsStatic
		{
			get { return (Properties & TypeProperties.Static) != 0; }
		}

		/// <summary>
		/// Is this type for a inline variable
		/// </summary>
		public bool IsInline
		{
			get { return (Properties & TypeProperties.Inline) != 0; }
		}

		/// <summary>
		/// Is this type for a explicitly mutable variable
		/// </summary>
		public bool IsMutable
		{
			get { return (Properties & TypeProperties.Mutable) != 0; }
		}

		/// <summary>
		/// Is this type for a explicitly volatile variable
		/// </summary>
		public bool IsVolatile
		{
			get { return (Properties & TypeProperties.Volatile) != 0; }
		}

		public TypeToken(string typeName, TypeProperties properties)
		{
			m_TypeName = typeName;
			m_Properties = properties;
		}

		public TypeToken(TypeToken other)
		{
			if (other == null)
				other = VoidType;

			m_TypeName = other.m_TypeName;
			m_Properties = other.m_Properties;
		}

		/// <summary>
		/// Ensure all null TypeToken values are replaced with VoidType
		/// </summary>
		public static void Sanitize(ref TypeToken token)
		{
			if (token == null)
			{
				token = VoidType;
			}
		}

		/// <summary>
		/// Ensure all null TypeToken values are replaced with VoidType
		/// </summary>
		public static void Sanitize(ref TypeToken[] tokens)
		{
			if (tokens == null)
			{
				tokens = new TypeToken[0];
			}

			for (int i = 0; i < tokens.Length; ++i)
			{
				if (tokens[i] == null)
				{
					tokens[i] = VoidType;
				}
			}
		}

		/// <summary>
		/// e.g. "static const int* const&" would return "const int"
		/// </summary>
		public string GetInnerTypeName()
		{
			if ((m_Properties & TypeProperties.Const) != 0)
				return "const " + m_TypeName;
			else
				return m_TypeName;
		}

		/// <summary>
		/// e.g. "static const int* const&" would return "const int* const"
		/// </summary>
		public string GetInnerTypeMinimal()
		{
			if ((m_Properties & TypeProperties.ConstPointer) != 0)
				return GetInnerTypeName() + "* const";
			else if ((m_Properties & TypeProperties.Pointer) != 0)
				return GetInnerTypeName() + "*";
			else
				return GetInnerTypeName();
		}

		/// <summary>
		/// e.g. "static const int* const&" would return "const int* const&"
		/// </summary>
		public string GetInnerTypeFull()
		{
			if ((m_Properties & TypeProperties.Reference) != 0)
				return GetInnerTypeMinimal() + "&";
			else
				return GetInnerTypeMinimal();
		}

		/// <summary>
		/// e.g. "static const int* const&" would return "static const int* const&"
		/// </summary>
		public string GetFullType()
		{
			string current = GetInnerTypeFull();
			
			if ((m_Properties & TypeProperties.Mutable) != 0)
				current = "mutable " + current;
			if ((m_Properties & TypeProperties.Volatile) != 0)
				current = "volatile " + current;
			if ((m_Properties & TypeProperties.Static) != 0)
				current = "static " + current;
			if ((m_Properties & TypeProperties.Inline) != 0)
				current = "inline " + current;

			return current;
		}
		
		public override string ToString()
		{
			return GetFullType();
		}

		/// <summary>
		/// Expand any macros relating to this type (Missing macros will be left)
		/// $(TypeName)
		/// $(IsVoid)
		/// $(PointerToken)			e.g. If is const pointer: "* const". If is nothing: ""
		/// $(ReferenceToken)
		/// $(ConstToken)
		/// $(StaticToken)
		/// $(InlineToken)
		/// $(MutableToken)
		/// $(IsPointer)
		/// $(IsReference)
		/// $(IsConst)
		/// $(IsStatic)
		/// $(IsInline)
		/// $(IsMutable)
		/// $(InnerTypeNameToken)
		/// $(InnerTypeMinimalToken)
		/// $(InnerTypeFullToken)
		/// $(FullTypeToken)
		/// 
		/// </summary>
		/// <param name="input">The raw input string which should have it's macros replaced</param>
		/// <param name="prefix">The prefix to apply to every macro</param>
		/// <param name="suffix">The suffix to apply to each macro</param>
		/// <returns>The string with all relevent macros expanded</returns>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			builder.Replace("$(" + prefix + "TypeName" + suffix + ")", m_TypeName);
			builder.Replace("$(" + prefix + "IsVoid" + suffix + ")", Equals(s_VoidType) ? "1" : "0");

			builder.Replace("$(" + prefix + "PointerToken" + suffix + ")", IsPointer ? (IsConst ? "*" : "* const") : "");
			builder.Replace("$(" + prefix + "ReferenceToken" + suffix + ")", IsReference ? "&" : "");
			builder.Replace("$(" + prefix + "ConstToken" + suffix + ")", IsConst ? "const" : "");
			builder.Replace("$(" + prefix + "StaticToken" + suffix + ")", IsStatic ? "static" : "");
			builder.Replace("$(" + prefix + "InlineToken" + suffix + ")", IsInline ? "inline" : "");
			builder.Replace("$(" + prefix + "MutableToken" + suffix + ")", IsMutable ? "mutable" : "");

			builder.Replace("$(" + prefix + "IsPointer" + suffix + ")", IsPointer ? "1" : "0");
			builder.Replace("$(" + prefix + "IsReference" + suffix + ")", IsReference ? "1" : "0");
			builder.Replace("$(" + prefix + "IsConst" + suffix + ")", IsConst ? "1" : "0");
			builder.Replace("$(" + prefix + "IsStatic" + suffix + ")", IsStatic ? "1" : "0");
			builder.Replace("$(" + prefix + "IsInline" + suffix + ")", IsInline ? "1" : "0");
			builder.Replace("$(" + prefix + "IsMutable" + suffix + ")", IsMutable ? "1" : "0");

			builder.Replace("$(" + prefix + "InnerTypeNameToken" + suffix + ")", GetInnerTypeName());
			builder.Replace("$(" + prefix + "InnerTypeMinimalToken" + suffix + ")", GetInnerTypeMinimal());
			builder.Replace("$(" + prefix + "InnerTypeFullToken" + suffix + ")", GetInnerTypeFull());
			builder.Replace("$(" + prefix + "FullTypeToken" + suffix + ")", GetFullType());

			return builder;
		}

		public int GetReflectionHash()
		{
			int hash = 12;
			hash = hash * 31 + m_TypeName.GetHashCode();
			hash = hash * 31 + m_Properties.GetHashCode();
			return hash;
		}

		public override bool Equals(object obj)
		{
			TypeToken token = obj as TypeToken;
			if (token != null)
			{
				return m_TypeName == token.m_TypeName && m_Properties == token.m_Properties;
			}

			return base.Equals(obj);
		}
	}

	public class NamedTypeToken
	{
		private TypeToken m_TypeToken;
		private string m_Name;

		/// <summary>
		/// Name of this type (i.e. variable name)
		/// </summary>
		public string Name
		{
			get { return m_Name; }
			internal set { m_Name = value; }
		}

		/// <summary>
		/// Get the regular type token for this named token
		/// </summary>
		public TypeToken InnerType
		{
			get { return m_TypeToken; }
		}


		/// <summary>
		/// C++ compiled typename
		/// </summary>
		public string TypeName
		{
			get { return InnerType.TypeName; }
		}

		/// <summary>
		/// The properties for this type
		/// </summary>
		public TypeProperties Properties
		{
			get { return InnerType.Properties; }
		}

		/// <summary>
		/// Is this type a pointer
		/// </summary>
		public bool IsPointer
		{
			get { return InnerType.IsPointer; }
		}

		/// <summary>
		/// Is this type a reference
		/// </summary>
		public bool IsReference
		{
			get { return InnerType.IsReference; }
		}

		/// <summary>
		/// Can this type have it's value changed (i.e. is const or const pointer)
		/// </summary>
		public bool IsConst
		{
			get { return InnerType.IsConst; }
		}

		/// <summary>
		/// Is this type for a static variable
		/// </summary>
		public bool IsStatic
		{
			get { return InnerType.IsStatic; }
		}

		/// <summary>
		/// Is this type for a inline variable
		/// </summary>
		public bool IsInline
		{
			get { return InnerType.IsInline; }
		}

		/// <summary>
		/// Is this type for a explicitly mutable variable
		/// </summary>
		public bool IsMutable
		{
			get { return InnerType.IsMutable; }
		}

		/// <summary>
		/// Is this type for a explicitly volatile variable
		/// </summary>
		public bool IsVolatile
		{
			get { return InnerType.IsVolatile; }
		}

		public NamedTypeToken(string name, TypeToken typeToken)
		{
			m_Name = name;
			m_TypeToken = new TypeToken(typeToken);
		}

		public NamedTypeToken(string name, string typeName, TypeProperties properties)
		{
			m_Name = name;
			m_TypeToken = new TypeToken(typeName, properties);
		}

		/// <summary>
		/// Ensure all null TypeToken values are replaced with VoidType
		/// </summary>
		public static void Sanitize(ref NamedTypeToken token)
		{
			if (token == null)
			{
				token = new NamedTypeToken("Untitled", TypeToken.VoidType);
			}
		}

		/// <summary>
		/// Ensure all null TypeToken values are replaced with VoidType
		/// </summary>
		public static void Sanitize(ref NamedTypeToken[] tokens)
		{
			if (tokens == null)
			{
				tokens = new NamedTypeToken[0];
			}

			for (int i = 0; i < tokens.Length; ++i)
			{
				if (tokens[i] == null)
				{
					tokens[i] = new NamedTypeToken("Untitled", TypeToken.VoidType);
				}
				else if (tokens[i].m_TypeToken == null)
				{
					tokens[i].m_TypeToken = TypeToken.VoidType;
				}
			}
		}
		
		public override string ToString()
		{
			return m_TypeToken.ToString() + " " + m_Name;
		}

		/// <summary>
		/// Expand any macros relating to this type (Missing macros will be left)
		/// $(Name)
		/// $(TypeName)
		/// $(PointerToken)			e.g. If is const pointer: "* const". If is nothing: ""
		/// $(ReferenceToken)
		/// $(ConstToken)
		/// $(StaticToken)
		/// $(InlineToken)
		/// $(MutableToken)
		/// $(IsPointer)
		/// $(IsReference)
		/// $(IsConst)
		/// $(IsStatic)
		/// $(IsInline)
		/// $(IsMutable)
		/// $(InnerTypeNameToken)
		/// $(InnerTypeMinimalToken)
		/// $(InnerTypeFullToken)
		/// $(FullTypeToken)
		/// 
		/// </summary>
		/// <param name="input">The raw input string which should have it's macros replaced</param>
		/// <param name="prefix">The prefix to apply to every macro</param>
		/// <param name="suffix">The suffix to apply to each macro</param>
		/// <returns>The string with all relevent macros expanded</returns>
		public StringBuilder ExpandMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			InnerType.ExpandMacros(builder, prefix, suffix);
			builder.Replace("$(" + prefix + "Name" + suffix + ")", m_Name);
			return builder;
		}

		public int GetReflectionHash()
		{
			int hash = InnerType.GetReflectionHash();
			hash = hash * 31 + m_Name.GetHashCode();
			return hash;
		}

		public override bool Equals(object obj)
		{
			NamedTypeToken token = obj as NamedTypeToken;
			if (token != null)
			{
				return m_Name == token.m_Name && m_TypeToken.Equals(token.m_TypeToken);
			}

			return base.Equals(obj);
		}
	}
}
