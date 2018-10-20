#pragma once
#include "TypeId.h"

typedef unsigned char uchar;
typedef unsigned short ushort;
typedef unsigned int uint;
typedef unsigned long ulong;

#define FOREACH_PRISM_COMMONTYPE(Macro) \
	Macro(char) \
	Macro(short) \
	Macro(int) \
	Macro(long) \
	Macro(uchar) \
	Macro(ushort) \
	Macro(uint) \
	Macro(ulong) \
	Macro(float) \
	Macro(double) 

#define EXPORT_PRISM_TYPE(T) TYPEID_HEADER(PRISMCORE_API, T)
FOREACH_PRISM_COMMONTYPE(EXPORT_PRISM_TYPE)