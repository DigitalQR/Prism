#pragma once
#include "Definitions.h"

namespace Prism 
{
	///
	/// Some brief information about a the template that the type is inheriting from
	///
	class PRISMCORE_API TemplateInfo
	{
	protected:
		const String m_Name;
		const size_t m_ParamCount;

		TemplateInfo(const String& name, size_t paramCount);

	public:
		///
		/// The typename for this template (Does not include namespace information)
		///
		inline const String& GetTypeName() const { return m_Name; }

		///
		/// How many templated parameters is supported by this template
		///
		inline size_t GetParamCount() const { return m_ParamCount; }
	};
}