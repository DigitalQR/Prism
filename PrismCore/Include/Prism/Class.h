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
		const std::vector<const Method*> m_Constructors;
		const std::vector<const Method*> m_Methods;
		const std::vector<const Property*> m_Properties;

		const bool m_IsAbstract : 1;

	public:
		///
		/// Construct a new object of this type
		/// @param params		The params that should be used in the constructor for this type
		/// @returns The newly constructed object or an empty holder if a valid constructor couldn't be found
		///
		virtual Prism::Holder CreateNew(const std::vector<Prism::Holder>& params = {}) const override;

		///
		/// Check if these params are valid for calling this particular class
		/// @param params		The params which the constructor should be called using
		///
		bool HasValidConstructor(const std::vector<Prism::Holder>& params = {});

		///
		/// Get the parent class at this index
		/// @param index		The local index of the parent
		/// @returns The class info for the parent class at this index
		///
		virtual Prism::ClassInfo GetParentClass(int index) const = 0;

		///
		/// Get the number of (Reflected )parents this class has
		///
		virtual size_t GetParentCount() const = 0;

		///
		/// Is this type info a valid instance of (Child or original) this type
		/// @param testType		The desired parent type we would like to check for
		///
		virtual bool IsInstanceOf(TypeInfo testType) const override;

		///
		/// Attempt to retrieve a function by it's name
		/// @param name			The name of the function to look for
		/// @param recurse		Should this method be looked for in parent classes
		/// @returns The method, if found, or nullptr
		///
		const Method* GetMethodByName(const String& name, bool recurse = true) const;

		///
		/// Fetch the method at this index
		/// @param recurse		Should this method be looked for in parent classes
		///
		const Method* GetMethodByIndex(int index, bool recurse = true) const;

		///
		/// The total count of methods that this class has access to
		///
		size_t GetMethodCount(bool recurse = true) const;

		///
		/// Attempt to retrieve a property by it's name
		/// @param name			The name of the property to look for
		/// @param recurse		Should this method be looked for in parent classes
		/// @returns The property, if found, or nullptr
		///
		const Property* GetPropertyByName(const String& name, bool recurse = true) const;

		///
		/// Fetch the method at this index
		/// @param recurse		Should this method be looked for in parent classes
		///
		const Property* GetPropertyByIndex(int index, bool recurse = true) const;

		///
		/// The total count of properties that this class has access to
		/// @param recurse		Should this method be looked for in parent classes
		///
		size_t GetPropertyCount(bool recurse = true) const;

	protected:
		Class(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, bool isAbstract, const std::vector<const Method*>& constructors, const std::vector<const Method*>& methods, const std::vector<const Property*>& properties);

		///
		/// Convert this given value into a string
		/// (Holder will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual Prism::String ToString(Prism::Holder inStorage) const override;

		///
		/// Attempt to parse a string into an instance of this type 
		/// (Holder will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual bool ParseFromString(const String& str, Prism::Holder outStorage) const override;
	};
}
