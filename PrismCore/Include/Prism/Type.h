///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include <string>

namespace Prism 
{
	class PRISMCORE_API Type 
	{
	private:
		const String m_Name;
		const size_t m_Size;

	public:
		inline const String& GetName() const { return m_Name; }
		inline size_t GetSize() const { return m_Size; }

		template<typename T>
		static const Type* GetTypeOf();

	protected:
		Type(const String& name, size_t size);
	};
	
	template<typename T>
	static const Type* Type::GetTypeOf() 
	{
		return nullptr;
	}
}