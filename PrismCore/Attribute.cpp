#include "Include\Prism\Attribute.h"

namespace Prism 
{
	Attribute::Attribute(Usage usage, bool inherit, bool allowMultiple)
		: m_Usage(usage)
		, m_Inherit(inherit)
		, m_AllowMultiple(allowMultiple)
	{
	}
}