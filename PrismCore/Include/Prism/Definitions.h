///
/// Common definitions used throughout Prism
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include <string>

///
/// Allow for switching between single and wide string
/// (Default is wstring, must define _PRISM_SSTRING to use string)
///
namespace Prism
{
#ifdef _PRISM_SSTRING
	typedef std::string String;
	#define PRISM_STR(s) s
#else
	typedef std::wstring String;
	#define PRISM_STR(s) L##s
#endif
}

///
/// Allow for dev only strings which will be empty if PRISM_DEVSTR is not defined e.g. doc strings
/// (Default is non-dev mode, must define _PRISM_DEV to enable dev content)
///
#ifdef _PRISM_DEV
#define PRISM_DEVSTR(s) PRISM_STR(s)
#else
#define PRISM_DEVSTR(s) PRISM_STR("")
#endif

///
/// Export/Import interface
///
#if defined(_WIN32) || defined(_WIN64)
#define PRISM_DLL_EXPORT __declspec(dllexport)
#define PRISM_DLL_IMPORT __declspec(dllimport)
#else
#define PRISM_DLL_EXPORT 
#define PRISM_DLL_IMPORT 
#endif

#ifndef PRISM_STATIC
#ifdef _PRISMCORE
#define PRISMCORE_API PRISM_DLL_EXPORT
#else
#define PRISMCORE_API PRISM_DLL_IMPORT
#endif
#else
#define PRISMCORE_API
#endif