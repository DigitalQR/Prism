///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "Holder.h"

#include <vector>

namespace Prism
{
	class TypeInfo;

	struct PRISMCORE_API ParamInfo
	{
		const String Name;
		const TypeInfo* TypeInfo;
		const bool IsPointer;
	};

	class PRISMCORE_API Method
	{
	private:
		const String m_Name;
		const Type* m_OwningType;
		const ParamInfo m_ReturnType;
		const std::vector<ParamInfo> m_ParamTypes;
	protected:
		const bool m_IsStatic;

		Method(const String& name, const Type* owningType, const ParamInfo& returnType, const std::vector<ParamInfo>& paramTypes);

	public:
		inline const String& GetName() const { return m_Name; }

		inline const ParamInfo& GetReturnInfo() const { return m_ReturnType; }
		inline const ParamInfo& GetParamInfo(int index) const { return m_ParamTypes[index]; }
		inline size_t GetParamCount() const { return m_ParamTypes.size(); }
		
		inline bool IsStatic() const { return m_IsStatic; }

		///
		/// Attempt to call this method with these params
		/// Warning: This may crash if params are invalid
		/// @param target		The class instance which this method will be called on (Will be ignored for static/global functions)
		/// @param params		The parms to pass into the method call
		/// @returns The result for this call
		///
		virtual Prism::Holder Call(Prism::Holder target, const std::vector<Prism::Holder>& params) const = 0;

		///
		/// Check that these params will result in a valid call
		/// @param target		The class instance which this method will be called on (Will be ignored for static/global functions)
		/// @param params		The parms to pass into the method call
		/// @returns If these params will result in a valid call
		///
		bool AreValidParams(Prism::Holder target, const std::vector<Prism::Holder>& params) const;
	};
}