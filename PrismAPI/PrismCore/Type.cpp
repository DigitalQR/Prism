#include "Include\Prism\Type.h"
#include "Include\Prism\Assembly.h"

namespace Prism 
{
	Type::Type(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, const TemplateInfo* templateInfo, const std::vector<const Attribute*>& attributes, bool isClass, bool isEnum)
		: AttributeStore(isClass ? Attribute::Usage::Classes : (isEnum ? Attribute::Usage::Enums : Attribute::Usage::Nothing), attributes)
		, m_UniqueId(uniqueId)
		, m_Namespace(space)
		, m_Documentation(documentation)
		, m_Name(name)
		, m_Size(size)
		, m_TemplateInfo(templateInfo)
		, m_IsClass(isClass)
		, m_IsEnum(isEnum)
	{
		m_AssociatedInfo = Assembly::Get().RegisterType(this);
	}

	String Type::GetAssemblyTypeName() const
	{ 
		return m_Namespace.empty() ? m_Name : m_Namespace + String(PRISM_STR(".")) + m_Name; 
	}

	bool Type::IsInstanceOf(TypeInfo testType) const 
	{
		return testType.Get() == this;
	}
}