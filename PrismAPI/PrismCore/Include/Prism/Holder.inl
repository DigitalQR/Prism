///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include <memory>

namespace Prism 
{
	template<typename T>
	ManagedInstance<T>::ManagedInstance(const T* source, const TypeInfo& type)
		: m_Data(new T(*source))
		, m_Type(type)
	{
	}

	template<typename T>
	ManagedInstance<T>::~ManagedInstance() 
	{
		delete m_Data;
	}

	///
	/// Instance selector 
	/// Select managed instance, if a valid copy-constructor exists
	///

	namespace Utils
	{
		///
		/// Use the ManagedInstance, if the type has a valid copy-constructor, otherwise use unsafe UnmanagedInstance which just used malloc and free
		///
		template<typename T>
		struct InstanceFactory
		{
		private:
			// Has valid copy-constructor
			static std::shared_ptr<ObjectInstance> Create(const T* source, std::true_type)
			{
				return std::make_shared<ManagedInstance<T>>(source, Assembly::Get().FindTypeOf<T>());
			}

			// Has no valid copy-constructor
			static std::shared_ptr<ObjectInstance> Create(const T* source, std::false_type)
			{
				return std::make_shared<UnmanagedInstance>(source, sizeof(T), Assembly::Get().FindTypeOf<T>());
			}

		public:
			typedef std::bool_constant<std::is_copy_constructible<T>::value> copy_type;

			static std::shared_ptr<ObjectInstance> CreateInstance(const T* source)
			{
				return Create(source, copy_type());
			}
		};

		///
		/// For values passed by reference, make a copy of the type
		///
		template<typename T, typename = std::enable_if_t<!std::is_pointer<T>::value>>
		std::shared_ptr<ObjectInstance> CreateInstance(const T* source)
		{
			return InstanceFactory<T>::CreateInstance(source);
		}

		///
		/// For values passed by pointer, just point towards the value
		///
		template<typename T, typename = std::enable_if_t<std::is_pointer<T>::value>>
		std::shared_ptr<ObjectInstance> CreateInstance(T source)
		{
			return std::make_shared<ManagedInstance<T>>(&source, Assembly::Get().FindTypeOf<T>());
		}
	}
		
	template<typename T, typename>
	Holder::Holder(const T& obj)
		: m_Data(Utils::CreateInstance<T>(&obj))
		, m_IsPointer(false)
	{
	}

	template<typename T, typename>
	Holder::Holder(T obj)
		: m_Data(Utils::CreateInstance<T>(obj))
		, m_IsPointer(true)
	{
	}

	template<typename T>
	T* Holder::GetPtrAs()
	{
		if(m_IsPointer)
			return *static_cast<T**>(m_Data->GetData());
		else
			return static_cast<T*>(m_Data->GetData());
	}

	template<typename T>
	const T* Holder::GetPtrAs() const
	{
		if (m_IsPointer)
			return *static_cast<T**>(m_Data->GetData());
		else
			return static_cast<T*>(m_Data->GetData());
	}

	template<typename T>
	T& Holder::GetAs() 
	{
		return *GetPtrAs<T>();
	}

	template<typename T>
	const T& Holder::GetAs() const
	{
		return *GetPtrAs<T>();
	}
}