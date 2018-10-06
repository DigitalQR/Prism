///
/// Common definitions used throughout Prism
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include <string>

// TEMP
#define PRISM_WSTRING
#define PRISM_DEVSTR

///
/// Allow for switching between single and wide string
///
namespace Prism
{
#ifdef PRISM_WSTRING
typedef std::wstring String;
#define PRISM_STR(s) L##s
#else
typedef std::string String;
#define PRISM_STR(s) s
#endif
}

///
/// Allow for dev only strings which will be empty if PRISM_DEVSTR is not defined e.g. doc strings
///
#ifdef PRISM_DEVSTR
#undef PRISM_DEVSTR
#define PRISM_DEVSTR(s) PRISM_STR(s)
#else
#define PRISM_DEVSTR(s) PRISM_STR("")
#endif

///
/// Export/Import interface
///
#if defined(_WIN32) || defined(_WIN64)
#define DLL_EXPORT __declspec(dllexport)
#define DLL_IMPORT __declspec(dllimport)
#else
#define DLL_EXPORT 
#define DLL_IMPORT 
#endif

#ifndef PRISM_STATIC
#ifdef _PRISMCORE
#define PRISMCORE_API DLL_EXPORT
#else
#define PRISMCORE_API DLL_IMPORT
#endif
#else
#define PRISMCORE_API
#endif