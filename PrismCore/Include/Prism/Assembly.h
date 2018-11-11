///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "TypeId.h"
#include "TypeInfo.h"
#include <map>

namespace Prism 
{
	class Type;

	///
	/// Store for any Prism reflection info
	///
	class PRISMCORE_API Assembly 
	{
	private:
		/// All know Prism type
		std::map<String, TypePointer*> m_TypeMap;

	public:
		///
		/// Get the active assembly
		///
		static Assembly& Get();

		///
		/// Attempt to find a type by its C++ type
		/// @returns type information (Check IsValid to see if found succesfully)
		///
		template<typename T>
		TypeInfo FindTypeOf() const;

		///
		/// Attempt to find a type by its typename
		/// @param name		(Case sensitive) Come in the format
		/// @returns type information (Check IsValid to see if found successfully)
		///
		TypeInfo FindTypeFromTypeName(const String& name) const;

		///
		/// Attempt to find a type by its full assmbly name
		/// @param name		(Case sensitive) Come in the format namespace.name where namespace is in the same format e.g. a.b.c
		/// @returns type information (Check IsValid to see if found successfully)
		///
		TypeInfo FindTypeFromAssemblyTypeName(const String& name) const;

	private:
		Assembly();
		Assembly(const Assembly&) = delete;
		void operator=(const Assembly&) = delete;
		
		///
		/// Attempt to find a type by its internal prism id
		/// @param id		The prism instance id of the type
		/// @returns nullptr if not found
		///
		TypeInfo FindTypeById(long id) const;

		///
		/// Register or update a type instance
		/// (Should only be called internally by other prism classes)
		/// @param type			Will be internally stored based on its internal name, so if dupe will overwrite previous instance
		///
		TypeInfo RegisterType(Type* type);

		friend Type;
	};
}

#include "Assembly.inl"