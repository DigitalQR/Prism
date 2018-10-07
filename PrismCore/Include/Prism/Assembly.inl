 
namespace Prism 
{
	template<typename T>
	const TypeInfo* Assembly::FindTypeOf() const
	{
		return FindTypeById(TypeId::Get<T>());
	}
}