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
	/// Base class for Attributes which can be attached during reflection then queried
	/// (Child classes are expected to append Attribute to the class name e.g. IsTestableAttribute)
	///
	class PRISMCORE_API Attribute
	{
	public:
		enum class Usage : unsigned char
		{
			Structs		= 1,
			Classes		= 2,
			Enums		= 4,
			Methods		= 8,
			Properties	= 16,

			DataStructures  = Structs | Classes,
			DataMembers		= Methods | Properties,

			Nothing		= 0,
			Anything	= 255
		};

	protected:
		const Usage m_Usage;
		const bool m_Inherit : 1;
		const bool m_AllowMultiple : 1;

		Attribute(Usage usage = Usage::Anything, bool inherit = false, bool allowMultiple = true);
	public:
		///
		/// What is the acceptable usage for this
		///
		inline Usage GetUsageFlags() const { return m_Usage; }

		///
		/// Can this attribute be inherited from a parent store
		///
		inline bool CanInherit() const { return m_Inherit; }

		///
		/// Are multiple of these attributes allowed
		///
		inline bool IsMultipleAllowed() const { return m_AllowMultiple; }
	};

}