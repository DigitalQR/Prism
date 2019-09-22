#include "Include\Prism\Class.h"
#include "Include\Prism\Method.h"
#include "Include\Prism\Property.h"

namespace Prism 
{
	Class::Class(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, const std::vector<const Attribute*>& attributes, bool isAbstract, const std::vector<const Method*>& constructors, const std::vector<const Method*>& methods, const std::vector<const Property*>& properties)
		: Type(uniqueId, space, name, documentation, size, attributes, true, false)
		, m_IsAbstract(isAbstract)
		, m_Constructors(constructors)
		, m_Methods(methods)
		, m_Properties(properties)
	{
	}

	Prism::Object Class::CreateNew(const std::vector<Prism::Object>& params) const 
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

	bool Class::HasValidConstructor(const std::vector<Prism::Object>& params) 
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

	bool Class::IsInstanceOf(TypeInfo testType) const
	{
		if (__super::IsInstanceOf(testType))
			return true;

		for (int i = 0; i < (int)GetParentCount(); ++i)
		{
			if (GetParentClass(i)->IsInstanceOf(testType))
				return true;
		}

		return false;
	}

	const Method* Class::GetMethodByName(const String& name, bool recurse) const
	{
		for (const Method* method : m_Methods)
		{
			if (method->GetName() == name)
				return method;
		}

		if (recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				const Method* method = GetParentClass(i)->GetMethodByName(name, recurse);
				if (method != nullptr)
					return method;
			}
		}

		return nullptr;
	}

	const Method* Class::GetMethodByIndex(int index, bool recurse) const
	{
		int localCount = (int)GetMethodCount(false);

		if (index < localCount)
		{
			return m_Methods[index];
		}
		else if (recurse)
		{
			index -= localCount;

			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				ClassInfo parentInfo = GetParentClass(i);
				int parentCount = (int)parentInfo->GetMethodCount(true);

				if (index < parentCount)
				{
					return parentInfo->GetMethodByIndex(index, true);
				}

				index -= parentCount;
			}
		}

		return nullptr;
	}

	size_t Class::GetMethodCount(bool recurse) const
	{ 
		size_t size = m_Methods.size();

		if (recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				size += GetParentClass(i)->GetMethodCount(true);
			}
		}

		return size;
	}

	const Property* Class::GetPropertyByName(const String& name, bool recurse) const
	{
		for (const Property* property : m_Properties)
		{
			if (property->GetName() == name)
				return property;
		}

		if (recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				const Property* property = GetParentClass(i)->GetPropertyByName(name, recurse);
				if (property != nullptr)
					return property;
			}
		}

		return nullptr;
	}

	const Property* Class::GetPropertyByIndex(int index, bool recurse) const 
	{
		int localCount = (int)GetPropertyCount(false);

		if (index < localCount)
		{
			return m_Properties[index];
		}
		else if (recurse)
		{
			index -= localCount;

			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				ClassInfo parentInfo = GetParentClass(i);
				int parentCount = (int)parentInfo->GetPropertyCount(true);

				if (index < parentCount)
				{
					return parentInfo->GetPropertyByIndex(index, true);
				}

				index -= parentCount;
			}
		}

		return nullptr;
	}

	size_t Class::GetPropertyCount(bool recurse) const 
	{
		size_t size = m_Properties.size();

		if (recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				size += GetParentClass(i)->GetPropertyCount(true);
			}
		}

		return size;
	}

	size_t Class::GetAttributeCount(bool recurse) const
	{
		size_t count = AttributeStore::GetAttributeCount(false);

		if (recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				count += GetParentClass(i)->GetInheritableAttributeCount(true);
			}
		}

		return count;
	}

	const Attribute* Class::GetAttributeByIndex(int index, bool recurse) const
	{
		int localCount = (int)GetAttributeCount(false);

		if (index < localCount)
		{
			return AttributeStore::GetAttributeByIndex(index, false);
		}
		else if(recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				const Attribute* attrib = GetParentClass(i)->TraverseFindInheritableAttribute(index, true);
				if (attrib != nullptr)
					return attrib;
			}
		}

		return nullptr;
	}

	size_t Class::GetInheritableAttributeCount(bool recurse) const 
	{
		size_t count = 0;

		for (int i = 0; i < (int)GetAttributeCount(false); ++i)
		{
			const Attribute* attrib = GetAttributeByIndex(i, false);
			if (attrib->CanInherit())
				++count;
		}

		if (recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				count += GetParentClass(i)->GetInheritableAttributeCount(true);
			}
		}

		return count;
	}

	const Attribute* Class::TraverseFindInheritableAttribute(int& index, bool recurse) const
	{
		for (int i = 0; i < (int)GetAttributeCount(false); ++i)
		{
			const Attribute* attrib = GetAttributeByIndex(i, false);
			if (attrib->CanInherit())
			{
				if (index == 0)
					return attrib;
				else
					--index;
			}
		}

		if (recurse)
		{
			for (int i = 0; i < (int)GetParentCount(); ++i)
			{
				const Attribute* attrib = GetParentClass(i)->TraverseFindInheritableAttribute(index, true);
				if (attrib != nullptr)
					return attrib;
			}
		}

		return nullptr;
	}

	Prism::String Class::ToString(Prism::Object inStorage) const 
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

	bool Class::ParseFromString(const String& str, Prism::Object outStorage) const 
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