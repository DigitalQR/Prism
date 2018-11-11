///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "TypeInfo.h"

namespace Prism 
{
	class Class;

	///
	/// Prism reflected type info
	///
	class PRISMCORE_API Type 
	{
	protected:
		const long m_UniqueId;
		const String m_Namespace;
		const String m_Name;
		const String m_Documentation;
		const size_t m_Size; 
		
		TypeInfo m_AssociatedInfo;

		const bool m_IsClass : 1;
		const bool m_IsEnum : 1;

		Type(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, bool isClass, bool isEnum);

	public:
		inline long GetUniqueId() const { return m_UniqueId; }

		///
		/// The namespace this type was delcared in.
		/// In the format A.B.C.TypeName
		///
		inline const String& GetNamespace() const { return m_Namespace; }

		///
		/// The typename for this type (Does not include namespace information)
		///
		inline const String& GetTypeName() const { return m_Name; }

		///
		/// The full typename for this type (Including namespace)
		/// In the format A.B.C.TypeName
		///
		String GetAssemblyTypeName() const;

		///
		/// Any documentation string that has been provided for this type
		/// (Will be empty if not compiled with _PRISM_DEV)
		///
		inline const String& GetDocumentation() const { return m_Documentation; }

		///
		/// How large this type is
		///
		inline size_t GetSize() const { return m_Size; }

		///
		/// Is does this type refer to a class
		///
		inline bool IsClass() const { return m_IsClass; }

		///
		/// Is does this type refer to an enum
		///
		inline bool IsEnum() const { return m_IsEnum; }

	private:
		Type(const Type&) = delete;
		void operator=(const Type&) = delete;

		friend class Assembly;
	};
}