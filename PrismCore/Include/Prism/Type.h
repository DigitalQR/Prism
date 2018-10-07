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

		Type(long uniqueId, const String& space, const String& name, size_t size);

	public:
		inline long GetUniqueId() const { return m_UniqueId; }
		inline const String& GetNamespace() const { return m_Namespace; }
		inline const String& GetName() const { return m_Name; }
		inline size_t GetSize() const { return m_Size; }

		String GetInternalName() const;

	private:
		Type(const Type&) = delete;
		void operator=(const Type&) = delete;
	};
}