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
