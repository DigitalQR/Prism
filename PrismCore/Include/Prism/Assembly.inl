 
namespace Prism 
{
	// Work around for Prism templates
	// Attempt to call RetrievePrismInfo()
	// (Based on https://stackoverflow.com/questions/12015195/how-to-call-member-function-only-if-object-happens-to-have-it)
	template<typename T>
	struct try_call_RetrievePrismInfo 
	{		
		// Function Exists
		template<typename A>
		static std::true_type test(decltype(&A::RetrievePrismInfo), void*)
		{
			return std::true_type;
		}

		// Doesn't exist
		template<typename A>
		static std::false_type test(...)
		{
			return std::false_type();
		}

		typedef decltype(test<T>(0, 0)) type;

		static const Type* run(std::true_type)
		{
			return T::RetrievePrismInfo();
		}

		static const Type* run(std::false_type)
		{
			return nullptr;
		}

		static const Type* run()
		{
			return run(type());
		}
	};

	template<typename T>
	const TypeInfo* Assembly::FindTypeOf() const
	{
		const TypeInfo* info = FindTypeById(TypeId::Get<T>());

		// Attempt to retieve info by calling RetrievePrismInfo (Required for template types)
		if (info == nullptr)
		{
			const Type* type = try_call_RetrievePrismInfo<T>::run();
			if (type != nullptr)
				return type->m_AssociatedInfo;
			else
				return nullptr;
		}
		else
			return info;
	}
}