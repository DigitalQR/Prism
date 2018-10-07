#pragma once
#include "TypeId.h"

typedef unsigned char unsigned_char;
typedef unsigned short unsigned_short;
typedef unsigned int unsigned_int;
typedef unsigned long unsigned_long;
typedef std::string std_string;
typedef std::wstring std_wstring;

#define FOREACH_PRISM_COMMONTYPE(Macro) \
	Macro(char) \
	Macro(short) \
	Macro(int) \
	Macro(long) \
	Macro(unsigned_char) \
	Macro(unsigned_short) \
	Macro(unsigned_int) \
	Macro(unsigned_long) \
	Macro(float) \
	Macro(double) \
	Macro(std_string) \
	Macro(std_wstring) 

#define EXPORT_PRISM_TYPE(T) TYPEID_HEADER(PRISMCORE_API, T)
FOREACH_PRISM_COMMONTYPE(EXPORT_PRISM_TYPE)