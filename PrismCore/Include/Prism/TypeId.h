///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"

namespace Prism 
{
	///
	/// Get a unique id for a C++ type
	///
	class PRISMCORE_API TypeId
	{
	public:
		template<typename T>
		static long Get();
	};
}

#include "TypeId.inl"