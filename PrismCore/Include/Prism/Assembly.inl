 
namespace Prism 
{
	namespace Utils
	{
		namespace Private
		{
			extern PRISMCORE_API long g_IdCounter;
		}

		template<typename T>
		long GetTypeId()
		{
			static long id = Private::g_IdCounter++;
			return id;
		}
	}

	template<typename T>
	const TypeInfo* Assembly::FindTypeOf() const
	{
		return FindTypeById(Utils::GetTypeId<T>());
	}
}