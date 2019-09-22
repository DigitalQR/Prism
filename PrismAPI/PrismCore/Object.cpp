#include "Include\Prism\Object.h"

namespace Prism 
{
	ObjectInstance::ObjectInstance()
	{
	}

	ObjectInstance::~ObjectInstance()
	{
	}

	UnmanagedInstance::UnmanagedInstance(const void* source, size_t size, const TypeInfo& type)
		: m_Size(size)
		, m_Type(type)
	{
		m_Data = malloc(m_Size);
		memcpy_s(m_Data, m_Size, source, m_Size);
	}

	UnmanagedInstance::~UnmanagedInstance()
	{
		free(m_Data);
	}

	Object::Object(const Object& other)
		: m_Data(other.m_Data)
		, m_IsPointer(other.m_IsPointer)
	{
	}

	Object& Object::operator=(const Object& other) 
	{
		m_Data = other.m_Data;
		m_IsPointer = other.m_IsPointer;
		return *this;
	}

	bool Object::operator==(const Object& other) const 
	{
		return m_Data->GetData() == other.m_Data->GetData();
	}

	bool Object::operator!=(const Object& other) const
	{
		return m_Data->GetData() == other.m_Data->GetData();
	}

	void* Object::GetData()
	{
		return m_Data->GetData();
	}

	const void* Object::GetData() const
	{
		return m_Data->GetData();
	}

	TypeInfo Object::GetTypeInfo() const 
	{
		return m_Data->GetTypeInfo();
	}
}