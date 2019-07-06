using System;
using System.Collections.Generic;
using System.Text;
using Prism.Reflection.Tokens;

namespace Prism.Reflection.Behaviour.Custom
{
	public class MethodReflectBehaviour : GlobalReflectionBehaviour
	{
		public MethodReflectBehaviour()
			: base(BehaviourTarget.Function, 0)
		{

		}

		public override void RunBehaviour(IReflectableToken target)
		{
			FunctionToken token = target as FunctionToken;

			if (token == null)
			{
				throw new Exception("Invalid state reached. Internal_ReflectMethod expected a FunctionToken target");
			}

			// Don't ever reflect destructor info
			if (!token.IsDestructor)
			{
				token.AppendIncludeContent(GenerateIncludeContent(token));
				token.AppendDeclarationContent(GenerateDeclarationContent(token));
				token.AppendImplementationContent(GenerateImplementationContent(token));
			}
		}

		private StringBuilder GenerateIncludeContent(FunctionToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(@"
#if $(PreProcessorCondition)
#define FUNC_$(ReflectedName)_DECL() \
private: \
class MethodInfo_$(ReflectedName) : public Prism::Method \
{ \
public: \
	MethodInfo_$(ReflectedName)(); \
\
	virtual Prism::TypeInfo GetParentInfo() const override; \
	virtual const Prism::ParamInfo* GetReturnInfo() const override; \
	virtual const Prism::ParamInfo* GetParamInfo(size_t index) const override; \
	virtual size_t GetParamCount() const override; \
	virtual Prism::Holder Call(Prism::Holder target, const std::vector<Prism::Holder>& params) const override; \
};
#else
#define FUNC_$(ReflectedName)_DECL()
#endif
");

			return builder;
		}

		private StringBuilder GenerateDeclarationContent(FunctionToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("FUNC_$(ReflectedName)_DECL() ");
			return builder;
		}

		private StringBuilder GenerateImplementationContent(FunctionToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(@"
#if $(PreProcessorCondition)
$(Parent.Name)::MethodInfo_$(ReflectedName)::MethodInfo_$(ReflectedName)()
	: Prism::Method(
		PRISM_STR(""$(Name)""), PRISM_DEVSTR(R""($(Documentation))""),
		{ $(AttributesInstances) },
		Prism::Accessor::$(AccessorPretty),
		$(IsStatic), $(IsConst), $(IsVirtual)
	)
{
}

const Prism::ParamInfo* $(Parent.Name)::MethodInfo_$(ReflectedName)::GetReturnInfo() const
{
#if $(IsConstructor)
	static Prism::ParamInfo info = {
		PRISM_STR(""ReturnValue""),
		Prism::Assembly::Get().FindTypeOf<$(Parent.Name)>(),
		1, 0
	};
	return &info;
#else
#if $(ReturnType.IsVoid)
	return nullptr;
#else
	static Prism::ParamInfo info = {
		PRISM_STR(""ReturnValue""),
		Prism::Assembly::Get().FindTypeOf<$(ReturnType.TypeName)>(),
		$(ReturnType.IsPointer), $(ReturnType.IsConst)
	};
	return &info;
#endif
#endif
}

Prism::TypeInfo $(Parent.Name)::MethodInfo_$(ReflectedName)::GetParentInfo() const
{
	return Prism::Assembly::Get().FindTypeOf<$(Parent.Name)>();
}

const Prism::ParamInfo* $(Parent.Name)::MethodInfo_$(ReflectedName)::GetParamInfo(size_t index) const
{
#if $(ParamCount)
	switch (index)
	{
		$(SwitchBody_GetParamInfo)
	}
#endif
	return nullptr;
}

size_t $(Parent.Name)::MethodInfo_$(ReflectedName)::GetParamCount() const
{
	return $(ParamCount);
}

template<typename T>
struct SafeConstruct$(ReflectedName)
{
private:
	template<typename A, typename = std::enable_if_t<std::is_abstract<A>::value>>
	static std::true_type AbstractCheck(...)
	{
		return std::true_type;
	}

	template<typename A, typename = std::enable_if_t<!std::is_abstract<A>::value>>
	static std::false_type AbstractCheck(...)
	{
		return std::false_type;
	}

	typedef decltype(AbstractCheck<T>(0)) IsAbstract;

	static T* ConstructInternal(Prism::Holder& target, std::vector<Prism::Holder>& params, std::true_type)
	{
		return nullptr;
	}

	static T* ConstructInternal(Prism::Holder& target, std::vector<Prism::Holder>& params, std::false_type)
	{
		return new $(Parent.Name)($(CallParams_Call));
	}

public:
	static T* Construct(Prism::Holder& target, std::vector<Prism::Holder>& params)
	{
		return ConstructInternal(target, params, IsAbstract());
	}
};

Prism::Holder $(Parent.Name)::MethodInfo_$(ReflectedName)::Call(Prism::Holder target, const std::vector<Prism::Holder>& params) const
{
	std::vector<Prism::Holder>& safeParams = *const_cast<std::vector<Prism::Holder>*>(&params);
#if $(IsConstructor)
	return SafeConstruct$(ReflectedName)<$(Parent.Name)>::Construct(target, safeParams);
#else
#if $(IsStatic)
#if $(ReturnType.IsVoid)
	$(Parent.Name)::$(Name)($(CallParams_Call));
	return nullptr;
#else
	return $(Parent.Name)::$(Name)($(CallParams_Call));
#endif
#else
	$(Parent.Name)* obj = target.GetPtrAs<$(Parent.Name)>();
#if $(ReturnType.IsVoid)
	obj->$(Name)($(CallParams_Call));
	return nullptr;
#else
	return obj->$(Name)($(CallParams_Call));
#endif
#endif
#endif
}

#endif
");
			string template_AttributesInstances = "";
			StringBuilder paramSelect = new StringBuilder();
			StringBuilder callParams = new StringBuilder();

			int a = 0;
			foreach (AttributeData attrib in target.DataAttributes)
			{
				string current = "new $(Attribute[%i].Name)Attribute($(Attribute[%i].Params)),\n";
				template_AttributesInstances += current.Replace("%i", a.ToString());
				++a;
			}

			for (int i = 0; i < target.ParamTypes.Length; ++i)
			{
				string switchStr = @"
case %i:
{
	static Prism::ParamInfo info = {
		PRISM_STR(""$(Param[%i].Name)""),
		Prism::Assembly::Get().FindTypeOf<$(Param[%i].TypeName)>(),
		$(Param[%i].IsPointer), $(Param[%i].IsConst)
	};
	return &info;
}
";
				string callStr;
				if (target.ParamTypes[i].IsReference)
					callStr = "*safeParams[%i].GetPtrAs<$(Param[%i].TypeName)>()";
				else if (target.ParamTypes[i].IsPointer)
					callStr = "safeParams[%i].GetPtrAs<$(Param[%i].TypeName)>()";
				else
					callStr = "safeParams[%i].GetAs<$(Param[%i].TypeName)>()";

				paramSelect.Append(switchStr.Replace("%i", i.ToString()));

				if (i != 0)
					callParams.Append(", ");
				callParams.Append(callStr.Replace("%i", i.ToString()));
			}


			builder.Replace("$(AttributesInstances)", template_AttributesInstances);
			builder.Replace("$(SwitchBody_GetParamInfo)", paramSelect.ToString());
			builder.Replace("$(CallParams_Call)", callParams.ToString());
			return builder;
		}
	}
}
