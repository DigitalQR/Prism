#include "Include\Prism\AttributeStore.h"

namespace Prism 
{
	inline std::vector<const Attribute*> CullBasedOnUsage(const std::vector<const Attribute*>& attributes, Attribute::Usage usage)
	{
		std::vector<const Attribute*> culledAttibutes;

		for (const Attribute* attrib : attributes)
		{
			if (attrib->SupportsUsage(usage))
			{
				// TODO - Check multiple attributes are supported
				culledAttibutes.push_back(attrib);
			}
			else
			{
				throw std::runtime_error(("Incompatible attribute within the current context (" + std::to_string((int)usage) + ")").c_str());
			}
		}

		culledAttibutes.shrink_to_fit();
		return culledAttibutes;
	}

	AttributeStore::AttributeStore(Attribute::Usage usage, const std::vector<const Attribute*>& attributes)
		: m_SupportedUsage(usage)
		, m_Attributes(CullBasedOnUsage(attributes, usage))
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