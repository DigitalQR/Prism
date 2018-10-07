
namespace Prism
{
	namespace Utils
	{
		template<typename T>
		struct PRISMGEN_ASSEMBLY_API UniqueStruct
		{
			static void dud() {};
		};
	}

	template<typename T>
	long TypeId::Get()
	{
		return reinterpret_cast<long>(&Utils::UniqueStruct<T>::dud);
	}
}

///
/// Utils defs for exporting TypeIds
///

#define TYPEID_HEADER(API, T) \
namespace Prism \
{ \
	template<> \
	long API TypeId::Get<T>(); \
}

#define TYPEID_SOURCE(T) \
namespace Prism \
{ \
	template<> \
	long TypeId::Get<T>() \
	{ \
		return reinterpret_cast<long>(&Utils::UniqueStruct<T>::dud); \
	} \
}