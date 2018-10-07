#include "Include\Prism\Assembly.h"
#include "Include\Prism\Type.h"

namespace Prism 
{
	namespace Utils
	{
		namespace Private
		{
			long g_IdCounter = 0;
		}
	}

	TypeInfo::TypeInfo(Type* type) 
		: m_Type(type)
	{
		
	}

	Assembly::Assembly() 
	{
	}

	Assembly& Assembly::Get() 
	{
		static Assembly assembly;
		return assembly;
	}

	const TypeInfo* Assembly::FindType(const String& internalName) const 
	{
		auto it = m_TypeMap.find(internalName);
		if (it == m_TypeMap.end())
			return nullptr;
		else
			return it->second;
	}
	
	const TypeInfo* Assembly::FindTypeById(long id) const 
	{
		for (const auto& it : m_TypeMap)
		{
			if (it.second->Get()->GetUniqueId() == id)
				return it.second;
		}

		return nullptr;
	}

	void Assembly::RegisterType(Type* type) 
	{
		String name = type->GetInternalName();
		auto it = m_TypeMap.find(name);

		if (it == m_TypeMap.end())
			m_TypeMap[name] = new TypeInfo(type);
		else
			// TODO - Make this thread safe
			it->second->m_Type = type;
	}
}