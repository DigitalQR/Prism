#include "Include\Prism\Assembly.h"
#include "Include\Prism\Type.h"
#include "Include\Prism\CommonTypes.h"

namespace Prism 
{
	Assembly::Assembly() 
	{
		// Forcefully re-register the null type info (Store lib being built with missing symbols)
		this->RegisterType(&Common::TypeInfo_nullptr::s_AssemblyInstance);
	}

	Assembly& Assembly::Get() 
	{
		static Assembly assembly;
		return assembly;
	}

	TypeInfo Assembly::FindTypeFromTypeName(const String& name) const
	{
		for (const auto& it : m_TypeMap)
		{
			if (it.second->m_Type->GetTypeName() == name)
				return it.second;
		}

		return nullptr;
	}

	TypeInfo Assembly::FindTypeFromAssemblyTypeName(const String& name) const
	{
		auto it = m_TypeMap.find(name);
		if (it == m_TypeMap.end())
			return nullptr;
		else
			return it->second;
	}
	
	TypeInfo Assembly::FindTypeById(long id) const
	{
		for (const auto& it : m_TypeMap)
		{
			if (it.second->m_Type->GetUniqueId() == id)
				return it.second;
		}

		return nullptr;
	}

	TypeInfo Assembly::RegisterType(Type* type)
	{
		String name = type->GetAssemblyTypeName();
		auto it = m_TypeMap.find(name);

		if (it == m_TypeMap.end())
		{
			TypePointer* info = new TypePointer(type);
			m_TypeMap[name] = info;
			return info;
		}
		else
		{
			// TODO - Make this thread safe
			it->second->m_Type = type;
			return it->second;
		}
	}
}