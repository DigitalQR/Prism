using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.CodeParsing.Signatures
{
	/// <summary>
	/// Basic information about a given typename
	/// </summary>
	public struct TypeNameInfo
	{
		public string TypeName;
		public bool IsConst;
	}

	/// <summary>
	/// Contains basic information about a given pointer/reference
	/// </summary>
	public struct PointerInfo
	{
		public bool IsConst;
	}

	/// <summary>
	/// The full type information about any given varable
	/// </summary>
	public class FullTypeInfo
	{
		public TypeNameInfo InnerType;
		public PointerInfo[] PointerData;
		public int PointerCount;
		public bool IsReference;

		public bool IsStatic;
		public bool IsVolatile;
		public bool IsInlined;

		public bool IsConst
		{
			get
			{
				if (PointerCount != 0)
					return PointerData[PointerCount - 1].IsConst;
				else
					return InnerType.IsConst;
			}
		}
	}

	public class TypeSignature
	{
		/// <summary>
		/// Get basic typename info out
		/// Expects input in form:
		/// TYPE,
		/// const TYPE,
		/// TYPE const
		/// </summary>
		public static bool TryGetTypeName(string typeString, out TypeNameInfo typeInfo)
		{
			typeInfo = new TypeNameInfo();
			
			// Get rid of all consts on front or end (Just in case they've written a bunch of useless consts
			while (typeString.StartsWith("const "))
			{
				typeString = typeString.Substring("const ".Length).Trim();
				typeInfo.IsConst = true;
			}
			while (typeString.EndsWith(" const"))
			{
				typeString = typeString.Substring(0, typeString.Length - "const ".Length).Trim();
				typeInfo.IsConst = true;
			}

			typeInfo.TypeName = typeString;
			return true;
		}

		/// <summary>
		/// Get all the type info about
		/// </summary>
		public static bool TryGetFullTypeInfo(string typeString, out FullTypeInfo typeInfo)
		{
			typeInfo = new FullTypeInfo();

			// Check for any specifiers
			if (typeString.Contains("static"))
			{
				typeString = typeString.Replace("static", "").Trim();
				typeInfo.IsStatic = true;
			}
			if (typeString.Contains("inline"))
			{
				typeString = typeString.Replace("inline", "").Trim();
				typeInfo.IsInlined = true;
			}
			if (typeString.Contains("volatile"))
			{
				typeString = typeString.Replace("volatile", "").Trim();
				typeInfo.IsVolatile = true;
			}
			
			// Is reference
			if (typeString.EndsWith("&"))
			{
				typeString = typeString.Substring(0, typeString.Length - 1).Trim();
				typeInfo.IsReference = true;
			}

			// Read from right to left, until ran into something that is not a pointer or pointer info
			List<PointerInfo> reversePointerData = new List<PointerInfo>();
			while (typeString.EndsWith("*") || typeString.EndsWith("const"))
			{
				// Check if we have a pointer next
				if (typeString.EndsWith("const"))
				{
					string subString = typeString.Substring(0, typeString.Length - "const".Length).Trim();
					if (subString.EndsWith("*"))
					{
						PointerInfo info = new PointerInfo();
						info.IsConst = true;
						reversePointerData.Add(info);

						typeString = subString.Substring(0, subString.Length - 1).Trim();
					}
					else
					{
						// Reached end of pointer chain (Probably)
						break;
					}
				}
				// Is just a normal pointer
				else
				{
					PointerInfo info = new PointerInfo();
					info.IsConst = false;
					reversePointerData.Add(info);

					typeString = typeString.Substring(0, typeString.Length - 1).Trim();
				}
			}

			typeInfo.PointerCount = reversePointerData.Count;
			if (typeInfo.PointerCount != 0)
			{
				typeInfo.PointerData = new PointerInfo[typeInfo.PointerCount];
				for (int i = 0; i < typeInfo.PointerCount; ++i)
				{
					typeInfo.PointerData[i] = reversePointerData[typeInfo.PointerCount - i - 1];
				}
			}
			
			return TryGetTypeName(typeString, out typeInfo.InnerType);
		}
	}
}
