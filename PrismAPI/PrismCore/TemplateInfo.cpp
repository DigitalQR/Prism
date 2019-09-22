#include "Include\Prism\TemplateInfo.h"

namespace Prism 
{
	TemplateInfo::TemplateInfo(const String& name, size_t paramCount)
		: m_Name(name)
		, m_ParamCount(paramCount)
	{
	}
}