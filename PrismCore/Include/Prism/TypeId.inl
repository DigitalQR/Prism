
namespace Prism
{
	template<typename T>
	long TypeId::Get()
	{
		return (long)(typeid(T).hash_code());
	}
}