#pragma once
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