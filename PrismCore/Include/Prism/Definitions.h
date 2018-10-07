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

/// Should any generated assembly be exported for DLL
#ifdef PRISM_EXPORT_ASSEMBLY
#define PRISMGEN_ASSEMBLY_API PRISM_DLL_EXPORT
#else
#define PRISMGEN_ASSEMBLY_API 
#endif

///
/// Prism code gen utils
///
#define PRISM_GEN_NAMESPACE PrismArtefact