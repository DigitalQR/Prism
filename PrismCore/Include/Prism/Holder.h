///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "Assembly.h"

#include <memory>

namespace Prism 
{
	///
	/// Abstract wrapper for holding data
	///
	class PRISMCORE_API ObjectInstance 
	{
	protected:
		ObjectInstance();
		virtual ~ObjectInstance();

	public:
		///
		/// Retrieve the pointer for the internal object
		///
		virtual void* GetData() = 0;

		///
		/// Retrieve the pointer for the internal object
		///
		virtual const void* GetData() const = 0;

		///
		/// Retrieve the Prism::Type of the object that is currently being held
		/// @returns Will return valid type if it exists, otherwise will return nullptr
		///
		virtual const TypeInfo* GetTypeInfo() const = 0;
	};

	///
	/// Wrapper for holding internal types i.e. non-prism types 
	///
	class PRISMCORE_API UnmanagedInstance : public ObjectInstance
	{
	private:
		void* m_Data;
		const size_t m_Size;
		const TypeInfo* m_Type;

	public:
		UnmanagedInstance(const void* source, size_t size, const TypeInfo* type);
		virtual ~UnmanagedInstance();

		virtual void* GetData() { return m_Data; }
		virtual const void* GetData() const { return m_Data; }

		size_t GetSize() const { return m_Size; }
		const TypeInfo* GetTypeInfo() const { return m_Type; }
	};

	///
	/// Wrapper for correctly managed internal data
	///
	template<typename T>
	class ManagedInstance : public ObjectInstance
	{
	private:
		T* m_Data;
		const TypeInfo* m_Type;

	public:
		ManagedInstance(const T* source, const TypeInfo* type);
		virtual ~ManagedInstance();

		virtual void* GetData() { return m_Data; }
		virtual const void* GetData() const { return m_Data; }

		size_t GetSize() const { return sizeof(T); }
		const TypeInfo* GetTypeInfo() const { return m_Type; }
	};

	///
	/// Object holder which will hold any type
	/// It will create memory if required or use
	///
	class PRISMCORE_API Holder
	{
	private:
		std::shared_ptr<ObjectInstance> m_Data;
		bool m_IsPointer;

	public:
		Holder(const Holder& other);

		template<typename T>
		Holder(T& obj);
		template<typename T>
		Holder(const T& obj);

		template<typename T>
		Holder(T* obj);
		template<typename T>
		Holder(const T* obj);


		///
		/// Copy internal data from other holder
		///
		Holder& operator=(const Holder& other);

		///
		/// Is internal data the same
		///
		bool operator==(const Holder& other) const;

		///
		/// Is the data internal held being treated as a pointer
		///
		inline bool IsPointer() const { return m_IsPointer; }

		///
		/// Get the internal pointer casted to some type
		///
		template<typename T>
		T* GetPtrAs();

		///
		/// Get the internal pointer casted to some type
		///
		template<typename T>
		const T* GetPtrAs() const;

		///
		/// Get the internal pointer casted to some type
		///
		template<typename T>
		T& GetAs();

		///
		/// Get the internal pointer casted to some type
		///
		template<typename T>
		const T& GetAs() const;

		///
		/// Get the internal pointer to the data this holder contains
		///
		void* GetData();

		///
		/// Get the internal pointer to the data this holder contains
		///
		const void* GetData() const;

		///
		/// Retrieve the Prism::Type of the object that is currently being held
		/// @returns Will return valid type if it exists, otherwise will return nullptr
		///
		const TypeInfo* GetTypeInfo() const;
	};
}

#include "Holder.inl"