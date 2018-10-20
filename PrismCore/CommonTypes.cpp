#include "Include\Prism\CommonTypes.h"
#include "Include\Prism\Type.h"
#include "Include\Prism\Assembly.h"

#define IMPLEMENT_PRISM_TYPE(T) \
T g_Temp_ ## T; \
class PRISMGEN_ASSEMBLY_API T ## _Type : public Prism::Type \
{ \
public: \
	T ## _Type() : Prism::Type(Prism::TypeId::Get<T>(), PRISM_STR(""), PRISM_STR(#T), sizeof(T), false) {} \
}; \
TYPEID_SOURCE(T) \
static PRISMGEN_ASSEMBLY_API T ## _Type g_ ## T ## _Instance;

FOREACH_PRISM_COMMONTYPE(IMPLEMENT_PRISM_TYPE)