#include "Include\Prism\Type.h"
#include "Include\Prism\Assembly.h"

namespace Prism 
{
	Type::Type(long uniqueId, const String& space, const String& name, size_t size)
		: m_UniqueId(uniqueId)
		, m_Namespace(space)
		, m_Name(name)
		, m_Size(size)
	{
		Assembly::Get().RegisterType(this);
	}

	String Type::GetInternalName() const 
	{ 
		return m_Namespace.empty() ? m_Name : m_Namespace + String(PRISM_STR(".")) + m_Name; 
	}

}