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
	ManagedInstance<T>::ManagedInstance(const T* source, const TypeInfo* type)
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
	}
	
	template<typename T>
	Holder::Holder(T& obj)
		: m_Data(Utils::InstanceFactory<T>::CreateInstance(&obj))
		, m_IsPointer(false)
	{
	}
	template<typename T>
	Holder::Holder(const T& obj)
		: m_Data(Utils::InstanceFactory<T>::CreateInstance(&obj))
		, m_IsPointer(false)
	{
	}

	template<typename T>
	Holder::Holder(T* obj)
		: m_Data(Utils::InstanceFactory<T>::CreateInstance(obj))
		, m_IsPointer(true)
	{
	}
	template<typename T>
	Holder::Holder(const T* obj)
		: m_Data(Utils::InstanceFactory<T>::CreateInstance(obj))
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