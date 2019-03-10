///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "Attribute.h"

#include <vector>

namespace Prism
{
	///
	/// Any reflected type which can have attributes applied to it
	///
	class PRISMCORE_API AttributeStore 
	{
	protected:
		const Attribute::Usage m_SupportedUsage;
		const std::vector<const Attribute*> m_Attributes;

	public:
		AttributeStore(Attribute::Usage usage, const std::vector<const Attribute*>& attributes);

		///
		/// The supported attribute usage for this store
		///
		inline Attribute::Usage GetSupportedAttributeUsage() const { return m_SupportedUsage; }

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