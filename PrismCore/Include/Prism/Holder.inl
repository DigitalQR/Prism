///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Type.h"
#include <memory>

namespace Prism 
{
	template<typename T>
	Holder::Holder(T& obj)
		: m_Data(new InternalInstance(&obj, sizeof(T), Type::GetTypeOf<T>()))
		, m_IsPointer(false)
	{
	}
	template<typename T>
	Holder::Holder(const T& obj)
		: m_Data(new InternalInstance(&obj, sizeof(T), Type::GetTypeOf<T>()))
		, m_IsPointer(false)
	{
	}

	template<typename T>
	Holder::Holder(T* obj)
		: m_Data(new InternalInstance(&obj, sizeof(T*), Type::GetTypeOf<T>()))
		, m_IsPointer(true)
	{
	}
	template<typename T>
	Holder::Holder(const T* obj)
		: m_Data(new InternalInstance(&obj, sizeof(T*), Type::GetTypeOf<T>()))
		, m_IsPointer(true)
	{
	}

	template<typename T>
	T* Holder::GetPtrAs()
	{
		if(m_IsPointer)
			return *static_cast<T**>(m_Data->GetData());
		else
			return static_cast<T*>(m_Data->GetData());
	}

	template<typename T>
	const T* Holder::GetPtrAs() const
	{
		if (m_IsPointer)
			return *static_cast<T**>(m_Data->GetData());
		else
			return static_cast<T*>(m_Data->GetData());
	}

	template<typename T>
	T& Holder::GetAs() 
	{
		return *GetPtrAs<T>();
	}

	template<typename T>
	const T& Holder::GetAs() const
	{
		return *GetPtrAs<T>();
	}
}