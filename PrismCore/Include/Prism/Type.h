///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"

namespace Prism 
{
	class Class;
	class TypeInfo;

	///
	/// Prism reflected type info
	///
	class PRISMCORE_API Type 
	{
	protected:
		const long m_UniqueId;
		const String m_Namespace;
		const String m_Name;
		const size_t m_Size; 
		
		const TypeInfo* m_AssociatedInfo;

		const bool m_IsClass : 1;

		Type(long uniqueId, const String& space, const String& name, size_t size, bool isClass);

	public:
		inline long GetUniqueId() const { return m_UniqueId; }
		inline const String& GetNamespace() const { return m_Namespace; }
		inline const String& GetName() const { return m_Name; }
		inline size_t GetSize() const { return m_Size; }

		inline bool IsClass() const { return m_IsClass; }

		String GetInternalName() const;

		///
		/// Safely convert this type into a class
		/// (Will be nullptr if invalid)
		///
		const Class* AsClass() const;

	private:
		Type(const Type&) = delete;
		void operator=(const Type&) = delete;

		friend class Assembly;
	};
}