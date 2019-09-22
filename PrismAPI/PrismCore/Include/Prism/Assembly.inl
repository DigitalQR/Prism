 
namespace Prism 
{
	// Work around for Prism templates
	// Attempt to call RetrievePrismInfo()
	template<typename T>
	struct RetrievePrismInfoHelper
	{		
		// Function Exists
		template<typename A>
		static std::true_type MethodCheck(decltype(&A::RetrievePrismInfo), void*)
		{
			return std::true_type;
		}

		// Doesn't exist
		template<typename A>
		static std::false_type MethodCheck(...)
		{
			return std::false_type();
		}

		typedef decltype(MethodCheck<T>(0, 0)) CheckResult;

		static const Type* Run(std::true_type)
		{
			return T::RetrievePrismInfo();
		}

		static const Type* Run(std::false_type)
		{
			return nullptr;
		}

		static const Type* Run()
		{
			return Run(CheckResult());
		}
	};

	template<typename T>
	TypeInfo Assembly::FindTypeOf() const
	{
		using CheckType = std::remove_pointer_t<std::remove_const_t<T>>;
		TypeInfo info = FindTypeById(TypeId::Get<CheckType>());

		// Attempt to retieve info by calling RetrievePrismInfo (Required for template types)
		if (!info.IsValid())
		{
			const Type* type = RetrievePrismInfoHelper<CheckType>::Run();
			if (type != nullptr)
				return type->m_AssociatedInfo;
			else
				return nullptr;
		}
		else
			return info;
	}
}