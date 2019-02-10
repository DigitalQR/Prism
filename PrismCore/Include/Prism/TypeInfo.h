///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "TypeId.h"

namespace Prism
{
	class Type;
	class Class;
	class Enum;

	///
	/// Reloadable type (Allows for hotreload of DLLs)
	///
	class PRISMCORE_API TypePointer
	{
	private:
		const Type* m_Type;

		TypePointer(const Type* type);

		TypePointer(const TypePointer&) = delete;
		void operator=(const TypePointer&) = delete;

		void Set(const Type* type) { m_Type = type; }
	public:
		const Type* Get() const { return m_Type; }

		///
		/// Is the type this is pointing to a class
		///
		bool IsClass() const;

		///
		/// Is the type this is pointing to an enum
		///
		bool IsEnum() const;

		friend class Assembly;
	};

	///
	/// Safe way to pass around type information (Is reload safe)
	///
	class PRISMCORE_API TypeInfo 
	{
	private:
		const TypePointer* m_TypePtr;

	public:
		TypeInfo();
		TypeInfo(const TypePointer* type);

		TypeInfo(const TypeInfo& other);
		TypeInfo& operator=(const TypeInfo& other);

		///
		/// Is this type info currently pointing towards a valid type
		///
		inline const bool IsValid() const { return m_TypePtr != nullptr; }

		///
		/// Is the type this is pointing to a class
		///
		inline bool IsClass() const { return m_TypePtr->IsClass(); }

		///
		/// Is the type this is pointing to an enum
		///
		inline bool IsEnum() const { return m_TypePtr->IsEnum(); }


		///
		/// Get the prism type this is holding 
		/// (Check with IsValid before use)
		///
		inline const Type* Get() const { return m_TypePtr->Get(); }

		///
		/// Get the prism type this is holding 
		/// (Check with IsValid before use)
		///
		inline const Type* operator->() const { return m_TypePtr->Get(); }


		///
		/// Compare type info
		///
		inline bool operator>(const TypeInfo& other) const { return m_TypePtr > other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator<(const TypeInfo& other) const  { return m_TypePtr < other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator>=(const TypeInfo& other) const { return m_TypePtr >= other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator<=(const TypeInfo& other) const { return m_TypePtr <= other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator==(const TypeInfo& other) const { return m_TypePtr == other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator!=(const TypeInfo& other) const { return m_TypePtr != other.m_TypePtr; };

		friend class ClassInfo;
		friend class EnumInfo;
	};

	///
	/// Safe way to pass around class type information (Is reload safe)
	///
	class PRISMCORE_API ClassInfo
	{
	private:
		const TypePointer* m_TypePtr;

	public:
		ClassInfo();
		ClassInfo(const TypePointer* type);

		ClassInfo(const ClassInfo& other);
		ClassInfo& operator=(const ClassInfo& other);

		ClassInfo(const TypeInfo& other);
		ClassInfo& operator=(const TypeInfo& other);

		///
		/// Is this type info currently pointing towards a valid type
		///
		inline const bool IsValid() const { return m_TypePtr != nullptr; }

		///
		/// Implicit type conversion to TypeInfo
		///
		inline operator TypeInfo() const { return TypeInfo(m_TypePtr); }

		///
		/// Get the prism type this is holding 
		/// (Check with IsValid before use)
		///
		inline const Class* Get() const { return (const Class*)m_TypePtr->Get(); }

		///
		/// Get the prism type this is holding 
		/// (Check with IsValid before use)
		///
		inline const Class* operator->() const { return (const Class*)m_TypePtr->Get(); }


		///
		/// Compare class info
		///
		inline bool operator>(const ClassInfo& other) const { return m_TypePtr > other.m_TypePtr; };

		///
		/// Compare class info
		///
		inline bool operator<(const ClassInfo& other) const { return m_TypePtr < other.m_TypePtr; };

		///
		/// Compare class info
		///
		inline bool operator>=(const ClassInfo& other) const { return m_TypePtr >= other.m_TypePtr; };

		///
		/// Compare class info
		///
		inline bool operator<=(const ClassInfo& other) const { return m_TypePtr <= other.m_TypePtr; };

		///
		/// Compare class info
		///
		inline bool operator==(const ClassInfo& other) const { return m_TypePtr == other.m_TypePtr; };

		///
		/// Compare class info
		///
		inline bool operator!=(const ClassInfo& other) const { return m_TypePtr != other.m_TypePtr; };


		///
		/// Compare type info
		///
		inline bool operator>(const TypeInfo& other) const { return m_TypePtr > other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator<(const TypeInfo& other) const { return m_TypePtr < other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator>=(const TypeInfo& other) const { return m_TypePtr >= other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator<=(const TypeInfo& other) const { return m_TypePtr <= other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator==(const TypeInfo& other) const { return m_TypePtr == other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator!=(const TypeInfo& other) const { return m_TypePtr != other.m_TypePtr; };
	};

	///
	/// Safe way to pass around enum type information (Is reload safe)
	///
	class PRISMCORE_API EnumInfo
	{
	private:
		const TypePointer* m_TypePtr;

	public:
		EnumInfo();
		EnumInfo(const TypePointer* type);

		EnumInfo(const EnumInfo& other);
		EnumInfo& operator=(const EnumInfo& other);

		EnumInfo(const TypeInfo& other);
		EnumInfo& operator=(const TypeInfo& other);

		///
		/// Is this type info currently pointing towards a valid type
		///
		inline const bool IsValid() const { return m_TypePtr != nullptr; }

		///
		/// Implicit type conversion to TypeInfo
		///
		inline operator TypeInfo() const { return TypeInfo(m_TypePtr); }

		///
		/// Get the prism type this is holding 
		/// (Check with IsValid before use)
		///
		inline const Enum* Get() const { return (const Enum*)m_TypePtr->Get(); }

		///
		/// Get the prism type this is holding 
		/// (Check with IsValid before use)
		///
		inline const Enum* operator->() const { return (const Enum*)m_TypePtr->Get(); }


		///
		/// Compare emum info
		///
		inline bool operator>(const EnumInfo& other) const { return m_TypePtr > other.m_TypePtr; };

		///
		/// Compare emum info
		///
		inline bool operator<(const EnumInfo& other) const { return m_TypePtr < other.m_TypePtr; };

		///
		/// Compare emum info
		///
		inline bool operator>=(const EnumInfo& other) const { return m_TypePtr >= other.m_TypePtr; };

		///
		/// Compare emum info
		///
		inline bool operator<=(const EnumInfo& other) const { return m_TypePtr <= other.m_TypePtr; };

		///
		/// Compare emum info
		///
		inline bool operator==(const EnumInfo& other) const { return m_TypePtr == other.m_TypePtr; };

		///
		/// Compare emum info
		///
		inline bool operator!=(const EnumInfo& other) const { return m_TypePtr != other.m_TypePtr; };


		///
		/// Compare type info
		///
		inline bool operator>(const TypeInfo& other) const { return m_TypePtr > other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator<(const TypeInfo& other) const { return m_TypePtr < other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator>=(const TypeInfo& other) const { return m_TypePtr >= other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator<=(const TypeInfo& other) const { return m_TypePtr <= other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator==(const TypeInfo& other) const { return m_TypePtr == other.m_TypePtr; };

		///
		/// Compare type info
		///
		inline bool operator!=(const TypeInfo& other) const { return m_TypePtr != other.m_TypePtr; };
	};
}