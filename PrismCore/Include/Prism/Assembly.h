///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include <map>

namespace Prism 
{
	class Type;
	class Class;

	///
	/// Reload safe type information
	///
	class PRISMCORE_API TypeInfo
	{
	private:
		Type* m_Type;

	public:
		const Type* Get() const { return m_Type; }
		const Type* operator->() const { return m_Type; }

	private:
		TypeInfo(Type* type);
		TypeInfo(const TypeInfo&) = delete;
		void operator=(const TypeInfo&) = delete;

		friend class Assembly;
	};

	///
	/// Store for any Prism reflection info
	///
	class PRISMCORE_API Assembly 
	{
	private:
		/// All know Prism type
		std::map<String, TypeInfo*> m_TypeMap;

	public:
		///
		/// Get the active assembly
		///
		static Assembly& Get();

		///
		/// Attempt to find a type by its C++ type
		///
		template<typename T>
		const TypeInfo* FindTypeOf() const;

		///
		/// Attempt to find a type by its internal name
		/// @param internalName		(Case sensitive) Come in the format namespace.name where namespace is in the same format e.g. a.b.c
		/// @returns nullptr if not found
		///
		const TypeInfo* FindType(const String& internalName) const;

	private:
		Assembly();
		Assembly(const Assembly&) = delete;
		void operator=(const Assembly&) = delete;
		
		///
		/// Attempt to find a type by its internal prism id
		/// @param id		The prism instance id of the type
		/// @returns nullptr if not found
		///
		const TypeInfo* FindTypeById(long id) const;

		///
		/// Register or update a type instance
		/// (Should only be called internally by other prism classes)
		/// @param type			Will be internally stored based on its internal name, so if dupe will overwrite previous instance
		///
		void RegisterType(Type* type);

		friend Type;
	};

	namespace Utils 
	{
		///
		/// Get a unique id for a C++ type
		///
		template<typename T>
		long GetTypeId();
	}
}

#include "Assembly.inl"