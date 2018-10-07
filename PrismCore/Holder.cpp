#include "Include\Prism\Holder.h"

namespace Prism 
{
	ObjectInstance::ObjectInstance()
	{
	}

	ObjectInstance::~ObjectInstance()
	{
	}

	InternalInstance::InternalInstance(const void* source, size_t size, const TypeInfo* type)
		: m_Size(size)
		, m_Type(type)
	{
		m_Data = malloc(m_Size);
		memcpy_s(m_Data, m_Size, source, m_Size);
	}

	InternalInstance::~InternalInstance() 
	{
		free(m_Data);
	}

	Holder::Holder(const Holder& other)
		: m_Data(other.m_Data)
		, m_IsPointer(other.m_IsPointer)
	{
	}

	Holder& Holder::operator=(const Holder& other) 
	{
		m_Data = other.m_Data;
		return *this;
	}

	bool Holder::operator==(const Holder& other) const 
	{
		return m_Data->GetData() == other.m_Data->GetData();
	}

	void* Holder::GetData()
	{
		return m_Data->GetData();
	}

	const void* Holder::GetData() const
	{
		return m_Data->GetData();
	}

	const TypeInfo* Holder::GetTypeInfo() const 
	{
		return m_Data->GetTypeInfo();
	}
}