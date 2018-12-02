#include "Include\Prism\Assembly.h"
#include "Include\Prism\Type.h"
#include "Include\Prism\CommonTypes.h"

#include <mutex>

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
		std::shared_lock<std::shared_mutex> readLock(m_TypeLock);

		for (const auto& it : m_TypeMap)
		{
			if (it.second->m_Type->GetTypeName() == name)
				return it.second;
		}

		return nullptr;
	}

	TypeInfo Assembly::FindTypeFromAssemblyTypeName(const String& name) const
	{
		std::shared_lock<std::shared_mutex> readLock(m_TypeLock);

		auto it = m_TypeMap.find(name);
		if (it == m_TypeMap.end())
			return nullptr;
		else
			return it->second;
	}
	
	TypeInfo Assembly::FindTypeById(long id) const
	{
		std::shared_lock<std::shared_mutex> readLock(m_TypeLock);

		for (const auto& it : m_TypeMap)
		{
			if (it.second->m_Type->GetUniqueId() == id)
				return it.second;
		}

		return nullptr;
	}

	TypeInfo Assembly::RegisterType(Type* type)
	{
		std::lock_guard<std::shared_mutex> writeLock(m_TypeLock);

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
			it->second->m_Type = type;
			return it->second;
		}
	}

	std::vector<TypeInfo> Assembly::SelectTypes(std::function<bool(TypeInfo)> queryCallback) const 
	{
		std::vector<TypeInfo> collection;
		std::shared_lock<std::shared_mutex> readLock(m_TypeLock);

		for (auto const& it : m_TypeMap)
		{
			if(queryCallback && queryCallback(it.second))
				collection.push_back(it.second);
		}

		return collection;
	}

	std::vector<TypeInfo> Assembly::SelectInstancesOf(TypeInfo type, bool includeSelf) const
	{
		std::vector<TypeInfo> collection;
		std::shared_lock<std::shared_mutex> readLock(m_TypeLock);

		for (auto const& it : m_TypeMap)
		{
			if (type == it.second)
			{
				if(includeSelf)
					collection.push_back(it.second);
			}
			else if (TypeInfo(it.second)->IsInstanceOf(type))
				collection.push_back(it.second);
		}

		return collection;
	}
}