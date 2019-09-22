#include "Include\Prism\Enum.h"

namespace Prism
{
	Enum::Value::Value()
		: Name()
		, NumberValue()
	{
	}

	Enum::Value::Value(const String& name, size_t value)
		: Name(name)
		, NumberValue(value)
	{
	}

	Enum::Enum(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, const std::vector<const Attribute*>& attributes,  const std::vector<Value>& values)
		: Type(uniqueId, space, name, documentation, size, attributes, false, true)
		, m_Values(values)
	{
	}

	Prism::Object Enum::CreateNew(const std::vector<Prism::Object>& params) const 
	{
		if (params.size() != 0)
			return nullptr;

		switch (m_Size)
		{
		case 1:
		{
			if (params.size() == 0)
				return new __int8;
			else if (params.size() == 1 && params[0].GetTypeInfo() == m_AssociatedInfo)
				return new __int8(params[0].GetAs<__int8>());
			else
				return nullptr;
		}

		case 2:
		{
			if (params.size() == 0)
				return new __int16;
			else if (params.size() == 1 && params[0].GetTypeInfo() == m_AssociatedInfo)
				return new __int16(params[0].GetAs<__int16>());
			else
				return nullptr;
		}

		case 4:
		{
			if (params.size() == 0)
				return new __int32;
			else if (params.size() == 1 && params[0].GetTypeInfo() == m_AssociatedInfo)
				return new __int32(params[0].GetAs<__int32>());
			else
				return nullptr;
		}

		case 8:
		{
			if (params.size() == 0)
				return new __int64;
			else if (params.size() == 1 && params[0].GetTypeInfo() == m_AssociatedInfo)
				return new __int64(params[0].GetAs<__int64>());
			else
				return nullptr;
		}

		default:
			return nullptr;
		}
	}

	Prism::String Enum::ToString(Prism::Object inStorage) const 
	{
		switch (m_Size)
		{
		case 1:
		{
			__int8& inValue = *inStorage.GetPtrAs<__int8>();

			for (const auto& value : m_Values)
			{
				if ((__int8)value.NumberValue == inValue)
				{
					return value.Name;
				}
			}

			return PRISM_STR("");
		}

		case 2:
		{
			__int16& inValue = *inStorage.GetPtrAs<__int16>();

			for (const auto& value : m_Values)
			{
				if ((__int16)value.NumberValue == inValue)
				{
					return value.Name;
				}
			}

			return PRISM_STR("");
		}

		case 4:
		{
			__int32& inValue = *inStorage.GetPtrAs<__int32>();

			for (const auto& value : m_Values)
			{
				if ((__int32)value.NumberValue == inValue)
				{
					return value.Name;
				}
			}

			return PRISM_STR("");
		}

		case 8:
		{
			__int64& inValue = *inStorage.GetPtrAs<__int64>();

			for (const auto& value : m_Values)
			{
				if ((__int64)value.NumberValue == inValue)
				{
					return value.Name;
				}
			}

			return PRISM_STR("");
		}

		default:
			return PRISM_STR("");
		}
	}

	bool Enum::ParseFromString(const String& str, Prism::Object outStorage) const 
	{
		switch (m_Size)
		{
		case 1:
		{
			__int8& outValue = *outStorage.GetPtrAs<__int8>();

			for (const auto& value : m_Values)
			{
				if (value.Name == str)
				{
					outValue = (__int8)value.NumberValue;
					return true;
				}
			}

			return false;
		}

		case 2:
		{
			__int16& outValue = *outStorage.GetPtrAs<__int16>();

			for (const auto& value : m_Values)
			{
				if (value.Name == str)
				{
					outValue = (__int16)value.NumberValue;
					return true;
				}
			}

			return false;
		}

		case 4:
		{
			__int32& outValue = *outStorage.GetPtrAs<__int32>();

			for (const auto& value : m_Values)
			{
				if (value.Name == str)
				{
					outValue = (__int32)value.NumberValue;
					return true;
				}
			}

			return false;
		}

		case 8:
		{
			__int64& outValue = *outStorage.GetPtrAs<__int64>();

			for (const auto& value : m_Values)
			{
				if (value.Name == str)
				{
					outValue = (__int64)value.NumberValue;
					return true;
				}
			}

			return false;
		}

		default:
			return false;
		}
	}
}