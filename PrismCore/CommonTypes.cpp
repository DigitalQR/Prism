#include "Include\Prism\Type.h"
#include "Include\Prism\Assembly.h"

#define PRISM_TYPE(T) \
T g_Temp_ ## T; \
class PRISMCORE_API T ## _Type : public Prism::Type \
{ \
public: \
	T ## _Type() : Prism::Type(Prism::Utils::GetTypeId<T>(), PRISM_STR(""), PRISM_STR(#T), sizeof(T)) {} \
}; \
static T ## _Type g_ ## T ## _Instance


namespace PRISM_GEN_NAMESPACE
{
	PRISM_TYPE(char);
	PRISM_TYPE(short);
	PRISM_TYPE(int);
	PRISM_TYPE(long);

	typedef unsigned char unsigned_char;
	typedef unsigned short unsigned_short;
	typedef unsigned int unsigned_int;
	typedef unsigned long unsigned_long;

	PRISM_TYPE(unsigned_char);
	PRISM_TYPE(unsigned_short);
	PRISM_TYPE(unsigned_int);
	PRISM_TYPE(unsigned_long);

	PRISM_TYPE(float);
	PRISM_TYPE(double);

	typedef std::string std_string;
	typedef std::wstring std_wstring;

	PRISM_TYPE(std_string);
	PRISM_TYPE(std_wstring);
}