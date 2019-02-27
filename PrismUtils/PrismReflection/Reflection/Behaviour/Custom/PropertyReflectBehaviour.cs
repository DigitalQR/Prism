using Prism.Reflection.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Behaviour.Custom
{
	public class PropertyReflectBehaviour : GlobalReflectionBehaviour
	{
		public PropertyReflectBehaviour()
			: base(BehaviourTarget.Variable, 0)
		{
		}

		public override void RunBehaviour(IReflectableToken target)
		{
			VariableToken token = target as VariableToken;

			if (token == null)
			{
				throw new Exception("Invalid state reached. Internal_ReflectProperty expected a VariableToken target");
			}

			token.AppendIncludeContent(GenerateIncludeContent(token));
			token.AppendDeclarationContent(GenerateDeclarationContent(token));
			token.AppendImplementationContent(GenerateImplementationContent(token));
		}

		private StringBuilder GenerateIncludeContent(VariableToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(@"
#if $(PreProcessorCondition)
#define VAR_$(ReflectedName)_DECL() \
private: \
class VariableInfo_$(ReflectedName) : public Prism::Property \
{ \
public: \
	VariableInfo_$(ReflectedName)(); \
\
	virtual Prism::TypeInfo GetParentInfo() const override; \
	virtual Prism::TypeInfo GetTypeInfo() const override; \
\
	virtual void Set(Prism::Holder, Prism::Holder) const override; \
	virtual Prism::Holder Get(Prism::Holder) const override;\
};
#else
#define VAR_$(ReflectedName)_DECL()
#endif
");
			return builder;
		}

		private StringBuilder GenerateDeclarationContent(VariableToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("VAR_$(ReflectedName)_DECL() ");
			return builder;
		}

		private StringBuilder GenerateImplementationContent(VariableToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(@"
#if $(PreProcessorCondition)
$(Parent.Name)::VariableInfo_$(ReflectedName)::VariableInfo_$(ReflectedName)()
	: Prism::Property(
		PRISM_STR(""$(Name)""), PRISM_DEVSTR(R""($(Documentation))""),
		{ /* TODO - Attributes in PropertyReflectBehaviour.cs */ },
		Prism::Accessor::$(AccessorPretty),
		$(IsPointer), $(IsStatic), $(IsConst)
	)
{
}

Prism::TypeInfo $(Parent.Name)::VariableInfo_$(ReflectedName)::GetParentInfo() const
{
	return Prism::Assembly::Get().FindTypeOf<$(Parent.Name)>();
}

Prism::TypeInfo $(Parent.Name)::VariableInfo_$(ReflectedName)::GetTypeInfo() const
{
	return Prism::Assembly::Get().FindTypeOf<$(TypeName)>();
}

void $(Parent.Name)::VariableInfo_$(ReflectedName)::Set(Prism::Holder target, Prism::Holder value) const
{
#if !($(IsConst))
#if $(IsStatic)
#if $(IsPointer)
	$(Parent.Name)::$(Name) = value.GetPtrAs<$(TypeName)>();
#else
	$(Parent.Name)::$(Name) = value.GetAs<$(TypeName)>();
#endif
#else
#if $(IsPointer)
	$(Parent.Name)* obj = target.GetPtrAs<$(Parent.Name)>();
	obj->$(Name) = value.GetPtrAs<$(TypeName)>();
#else
	$(Parent.Name)* obj = target.GetPtrAs<$(Parent.Name)>();
	obj->$(Name) = value.GetAs<$(TypeName)>();
#endif
#endif
#endif
}

Prism::Holder $(Parent.Name)::VariableInfo_$(ReflectedName)::Get(Prism::Holder target) const
{
#if $(IsStatic)
	return $(Parent.Name)::$(Name);
#else
	$(Parent.Name)* obj = target.GetPtrAs<$(Parent.Name)>();
	return obj->$(Name);
#endif
}
#endif
");
			return builder;
		}
	}
}
