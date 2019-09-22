///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "AttributeStore.h"
#include "TypeInfo.h"
#include "Object.h"

#include <vector>

namespace Prism 
{
	class Class;

	///
	/// Prism reflected type info
	///
	class PRISMCORE_API Type : public AttributeStore
	{
	protected:
		const long m_UniqueId;
		const String m_Namespace;
		const String m_Name;
		const String m_Documentation;
		const size_t m_Size; 
		
		TypeInfo m_AssociatedInfo;

		const bool m_IsClass : 1;
		const bool m_IsEnum : 1;

		Type(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, const std::vector<const Attribute*>& attributes, bool isClass, bool isEnum);

	public:
		inline long GetUniqueId() const { return m_UniqueId; }

		///
		/// Construct a new object of this type
		/// @param params		The params that should be used in the constructor for this type
		/// @returns The newly constructed object or an empty holder if a valid constructor couldn't be found
		///
		virtual Prism::Object CreateNew(const std::vector<Prism::Object>& params = {}) const = 0;

		///
		/// The namespace this type was delcared in.
		/// In the format A.B.C.TypeName
		///
		inline const String& GetNamespace() const { return m_Namespace; }

		///
		/// The typename for this type (Does not include namespace information)
		///
		inline const String& GetTypeName() const { return m_Name; }

		///
		/// The full typename for this type (Including namespace)
		/// In the format A.B.C.TypeName
		///
		String GetAssemblyTypeName() const;

		///
		/// Any documentation string that has been provided for this type
		/// (Will be empty if not compiled with _PRISM_DEV)
		///
		inline const String& GetDocumentation() const { return m_Documentation; }

		///
		/// How large this type is
		///
		inline size_t GetSize() const { return m_Size; }

		///
		/// Is does this type refer to a class
		///
		inline bool IsClass() const { return m_IsClass; }

		///
		/// Is does this type refer to an enum
		///
		inline bool IsEnum() const { return m_IsEnum; }

		///
		/// Is this type info a valid instance of (Child or original) this type
		/// @param testType		The desired parent type we would like to check for
		///
		virtual bool IsInstanceOf(TypeInfo testType) const;

		///
		/// Attempt to convert an instance of this type into a string
		/// (Passed by reference version)
		/// @returns The string value for this item or empty string, if failed
		///
		template<typename T, typename = std::enable_if_t<!std::is_pointer<T>::value>>
		inline Prism::String ConvertToString(const T& inValue) const
		{
			Prism::Object storageHolder = &inValue;
			if (m_AssociatedInfo == storageHolder.GetTypeInfo())
				return ToString(storageHolder);
			else
				return PRISM_STR("");
		}

		///
		/// Attempt to convert an instance of this type into a string
		/// (Passed by pointer version)
		/// @returns The string value for this item or empty string, if failed
		///
		template<typename T, typename = std::enable_if_t<std::is_pointer<T>::value>>
		inline Prism::String ConvertToString(T inValue) const
		{
			Prism::Object storageHolder = inValue;
			if (m_AssociatedInfo == storageHolder.GetTypeInfo())
				return ToString(storageHolder);
			else
				return PRISM_STR("");
		}


		///
		/// Attempt to parse a string into an instance of this type
		/// (Passed by reference version)
		/// @returns If the conversion was successful
		///
		template<typename T, typename = std::enable_if_t<!std::is_pointer<T>::value>>
		inline bool ConvertFromString(const String& str, T& outStorage) const
		{
			Prism::Object storageHolder = &outStorage;
			if (m_AssociatedInfo == storageHolder.GetTypeInfo())
				return ParseFromString(str, storageHolder);
			else
				return false;
		}

		///
		/// Attempt to parse a string into an instance of this type
		/// (Passed by pointer version)
		/// @returns If the conversion was successful
		///
		template<typename T, typename = std::enable_if_t<std::is_pointer<T>::value>>
		inline bool ConvertFromString(const String& str, T outStorage) const
		{
			Prism::Object storageHolder = outStorage;
			if (m_AssociatedInfo == storageHolder.GetTypeInfo())
				return ParseFromString(str, storageHolder);
			else
				return false;
		}

	protected:
		///
		/// Convert this given value into a string
		/// (Object will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual Prism::String ToString(Prism::Object inStorage) const { return PRISM_STR(""); }

		///
		/// Attempt to parse a string into an instance of this type 
		/// (Object will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual bool ParseFromString(const String& str, Prism::Object outStorage) const { return false; }

	private:
		Type(const Type&) = delete;
		void operator=(const Type&) = delete;

		friend class Assembly;
	};

	///
	/// Specializations for Prism::Object
	///
	
	template<>
	inline Prism::String Type::ConvertToString<Prism::Object>(const Prism::Object& holder) const
	{
		if (m_AssociatedInfo == holder.GetTypeInfo())
			return ToString(holder);
		else
			return PRISM_STR("");
	}

	template<>
	inline Prism::String Type::ConvertToString<Prism::Object*>(Prism::Object* holder) const
	{
		ConvertToString(*holder);
	}
	template<>
	inline Prism::String Type::ConvertToString<const Prism::Object*>(const Prism::Object* holder) const
	{
		ConvertToString(*holder);
	}

	template<>
	inline bool Type::ConvertFromString<Prism::Object>(const String& str, Prism::Object& holder) const
	{
		if (m_AssociatedInfo == holder.GetTypeInfo())
			return ParseFromString(str, holder);
		else
			return false;
	}

	template<>
	inline bool Type::ConvertFromString<Prism::Object*>(const String& str, Prism::Object* holder) const
	{
		return ConvertFromString(str, *holder);
	}
}