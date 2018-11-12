#pragma once
#include "Definitions.h"
#include "Type.h"
 
#define FOREACH_PRISM_COMMONTYPE(Macro) \
	Macro(,char,char) \
	Macro(,short,short) \
	Macro(,int,int) \
	Macro(,long,long) \
	Macro(,uchar,unsigned char) \
	Macro(,ushort,unsigned short) \
	Macro(,uint,unsigned int) \
	Macro(,ulong,unsigned long) \
	Macro(,float,float) \
	Macro(,double,double) \
	Macro(std,string,std::string) \
	Macro(std,wstring,std::wstring) 

#define IMPLEMENT_PRISM_HEADER(Namespace,StructName,TypeName) \
class PRISMCORE_API TypeInfo_ ## StructName : public Prism::Type \
{ \
public: \
	TypeInfo_ ## StructName(); \
	static TypeInfo_ ## StructName s_AssemblyInstance; \
protected: \
	virtual Prism::String ToString(Prism::Holder inStorage) const override; \
	virtual bool ParseFromString(const String& str, Prism::Holder outStorage) const override; \
}; 

namespace Prism
{
	namespace Common
	{
		class PRISMCORE_API TypeInfo_nullptr : public Prism::Type 
		{
		public:
			TypeInfo_nullptr();
			static TypeInfo_nullptr s_AssemblyInstance;
		};

		FOREACH_PRISM_COMMONTYPE(IMPLEMENT_PRISM_HEADER)
#undef IMPLEMENT_PRISM_HEADER
	}
}