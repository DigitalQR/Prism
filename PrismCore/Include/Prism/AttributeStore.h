///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"

#include <vector>

namespace Prism
{
	class Attribute;

	///
	/// Any reflected type which can have attributes applied to it
	///
	class PRISMCORE_API AttributeStore 
	{
	protected:
		const std::vector<const Attribute*> m_Attributes;

	public:
		AttributeStore(const std::vector<const Attribute*>& attributes);

		///
		/// How many attributes does this store contain
		/// @param recurse		Should this method be looked for in parent classes
		///
		virtual size_t GetAttributeCount(bool recurse = true) const;

		///
		/// Fetch the attribute at this index
		/// @param recurse		Should this method be looked for in parent classes
		///
		virtual const Attribute* GetAttributeByIndex(int index, bool recurse = true) const;
	};
}