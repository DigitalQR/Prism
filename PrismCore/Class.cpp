#include "Include\Prism\Class.h"
#include "Include\Prism\Method.h"
#include "Include\Prism\Property.h"

namespace Prism 
{
	Class::Class(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, bool isAbstract, const std::vector<const Method*>& constructors, const std::vector<const Method*>& methods, const std::vector<const Property*>& properties)
		: Type(uniqueId, space, name, documentation, size, true, false)
		, m_IsAbstract(isAbstract)
		, m_Constructors(constructors)
		, m_Methods(methods)
		, m_Properties(properties)
	{
	}

	Prism::Holder Class::CreateNew(const std::vector<Prism::Holder>& params) const 
	{
		if (m_IsAbstract)
			return nullptr;

		for (const Method* constructor : m_Constructors)
		{
			if (constructor->AreValidParams(nullptr, params))
				return constructor->Call(nullptr, params);
		}

		return nullptr;
	}

	bool Class::HasValidConstructor(const std::vector<Prism::Holder>& params) 
	{
		if (m_IsAbstract)
			return false;

		for (const Method* constructor : m_Constructors)
		{
			if (constructor->AreValidParams(nullptr, params))
				return true;
		}

		return false;
	}

	const Method* Class::GetMethodByName(const String& name) const 
	{
		for (const Method* method : m_Methods)
		{
			if (method->GetName() == name)
				return method;
		}

		return nullptr;
	}

	const Property* Class::GetPropertyByName(const String& name) const
	{
		for (const Property* property : m_Properties)
		{
			if (property->GetName() == name)
				return property;
		}

		return nullptr;
	}

	Prism::String Class::ToString(Prism::Holder inStorage) const 
	{
		const Method* toStringMethod = GetMethodByName(PRISM_STR("ToString"));
		if (toStringMethod && !toStringMethod->IsStatic() && toStringMethod->GetParamCount() == 0)
		{
			// Check that the FromString is in format Prism::String ToString();
			if (toStringMethod->GetReturnInfo()->Type == Assembly::Get().FindTypeOf<Prism::String>())
			{
				return toStringMethod->Call(inStorage).GetAs<Prism::String>();
			}
		}

		return PRISM_STR("");
	}

	bool Class::ParseFromString(const String& str, Prism::Holder outStorage) const 
	{
		const Method* fromStringMethod = GetMethodByName(PRISM_STR("FromString"));
		if (fromStringMethod && fromStringMethod->IsStatic() && fromStringMethod->GetParamCount() == 2)
		{
			const auto& param1 = fromStringMethod->GetParamInfo(0);
			const auto& param2 = fromStringMethod->GetParamInfo(1);

			// Check that the FromString is in format bool FromString(Prism::String inString, <Type> outValue);
			if (fromStringMethod->GetReturnInfo()->Type == Assembly::Get().FindTypeOf<bool>()
				&& (param1->Type == Assembly::Get().FindTypeOf<Prism::String>())
				&& (param2->Type == m_AssociatedInfo)
			   ) 
			{
				return fromStringMethod->Call(nullptr, { str, outStorage }).GetAs<bool>();
			}
		}

		return false;
	}
}