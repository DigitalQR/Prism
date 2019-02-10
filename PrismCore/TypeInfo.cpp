#include "Include\Prism\TypeInfo.h"
#include "Include\Prism\Type.h"

namespace Prism 
{
	TypePointer::TypePointer(const Type* type)
		: m_Type(type)
	{

	}

	bool TypePointer::IsClass() const
	{
		return m_Type->IsClass();
	}

	bool TypePointer::IsEnum() const
	{
		return m_Type->IsEnum();
	}


	TypeInfo::TypeInfo()
		: m_TypePtr(nullptr)
	{
	}

	TypeInfo::TypeInfo(const TypePointer* type)
		: m_TypePtr(type)
	{
	}

	TypeInfo::TypeInfo(const TypeInfo& other) 
		: m_TypePtr(other.m_TypePtr)
	{
	}

	TypeInfo& TypeInfo::operator=(const TypeInfo& other) 
	{
		m_TypePtr = other.m_TypePtr;
		return *this;
	}


	ClassInfo::ClassInfo() 
		: m_TypePtr(nullptr)
	{
	
	}

	ClassInfo::ClassInfo(const TypePointer* type)
		: m_TypePtr(type != nullptr && type->IsClass() ? type : nullptr)
	{
	}

	ClassInfo::ClassInfo(const ClassInfo& other)
		: m_TypePtr(other.m_TypePtr)
	{
	}

	ClassInfo& ClassInfo::operator=(const ClassInfo& other) 
	{
		m_TypePtr = other.m_TypePtr;
		return *this;
	}

	ClassInfo::ClassInfo(const TypeInfo& other)
		: ClassInfo(other.m_TypePtr)
	{
	}

	ClassInfo& ClassInfo::operator=(const TypeInfo& other) 
	{
		if (other.IsValid() && other.IsClass())
			m_TypePtr = other.m_TypePtr;
		else
			m_TypePtr = nullptr;
		return *this;
	}


	EnumInfo::EnumInfo()
		: m_TypePtr(nullptr)
	{

	}

	EnumInfo::EnumInfo(const TypePointer* type)
		: m_TypePtr(type != nullptr && type->IsEnum() ? type : nullptr)
	{
	}

	EnumInfo::EnumInfo(const EnumInfo& other)
		: m_TypePtr(other.m_TypePtr)
	{
	}

	EnumInfo& EnumInfo::operator=(const EnumInfo& other)
	{
		m_TypePtr = other.m_TypePtr;
		return *this;
	}

	EnumInfo::EnumInfo(const TypeInfo& other)
		: EnumInfo(other.m_TypePtr)
	{
	}

	EnumInfo& EnumInfo::operator=(const TypeInfo& other)
	{
		if (other.IsValid() && other.IsEnum())
			m_TypePtr = other.m_TypePtr;
		else
			m_TypePtr = nullptr;
		return *this;
	}
}