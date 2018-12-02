#include "Include\Prism\AttributeStore.h"

namespace Prism 
{
	AttributeStore::AttributeStore(const std::vector<const Attribute*>& attributes) 
		: m_Attributes(attributes)
	{
	}

	size_t AttributeStore::GetAttributeCount(bool recurse) const 
	{
		return m_Attributes.size();
	}

	const Attribute* AttributeStore::GetAttributeByIndex(int index, bool recurse) const 
	{
		if (index < (int)m_Attributes.size())
			return m_Attributes[index];
		else
			return nullptr;
	}
}