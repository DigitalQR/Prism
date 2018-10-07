///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "Type.h"

#include <vector>

namespace Prism 
{
	class Method;
	class Property;

	class PRISMCORE_API Class : public Type
	{
	private:
		const std::vector<const Method*> m_Methods;
		const std::vector<const Property*> m_Properties;

	public:
		///
		/// Attempt to retrieve a function by it's name
		/// @param name			The name of the function to look for
		/// @returns The method, if found, or nullptr
		///
		const Method* GetMethodByName(const String& name) const;

		///
		/// Fetch the method at this index
		///
		inline const Method* GetMethodByIndex(int index) const { return m_Methods[index]; }

		///
		/// The total count of methods that this class has access to
		///
		inline size_t GetMethodCount() const { return m_Methods.size(); }

		///
		/// Attempt to retrieve a property by it's name
		/// @param name			The name of the property to look for
		/// @returns The property, if found, or nullptr
		///
		const Property* GetPropertyByName(const String& name) const;

		///
		/// Fetch the method at this index
		///
		inline const Property* GetPropertyByIndex(int index) const { return m_Properties[index]; }

		///
		/// The total count of properties that this class has access to
		///
		inline size_t GetPropertyCount() const { return m_Properties.size(); }

	protected:
		Class(long uniqueId, const String& space, const String& name, size_t size, const std::vector<const Method*>& methods, const std::vector<const Property*>& properties);
	};
}
