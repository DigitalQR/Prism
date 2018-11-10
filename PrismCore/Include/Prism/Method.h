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
		String Name;
		const TypeInfo* TypeInfo;
		bool IsPointer : 1;
		bool IsConst : 1;
	};

	class PRISMCORE_API Method
	{
	private:
		const String m_Name;

	protected:
		const bool m_IsStatic : 1;
		const bool m_IsConst : 1;
		const bool m_IsVirtual : 1;

		Method(const String& name, bool isStatic, bool isConst, bool isVirtual);

	public:
		inline const String& GetName() const { return m_Name; }

		inline bool IsStatic() const { return m_IsStatic; }
		inline bool IsConst() const { return m_IsConst; }
		inline bool IsVirtual() const { return m_IsVirtual; }


		///
		/// Get the return type info for this method
		/// @returns The param info for the return type
		///
		virtual const ParamInfo* GetReturnInfo() const = 0;

		///
		/// Get the type info for a specific param
		/// @returns The param info for the param
		///
		virtual const ParamInfo* GetParamInfo(size_t index) const = 0;

		///
		/// Get the number of params that this method has
		///
		virtual size_t GetParamCount() const = 0;

		///
		/// Attempt to call this method with these params
		/// Warning: This may crash if params are invalid
		/// @param target		The class instance which this method will be called on (Will be ignored for static/global functions)
		/// @param params		The parms to pass into the method call
		/// @returns The result for this call
		///
		virtual Prism::Holder Call(Prism::Holder target = nullptr, const std::vector<Prism::Holder>& params = {}) const = 0;

		///
		/// Check that these params will result in a valid call
		/// @param target		The class instance which this method will be called on (Will be ignored for static/global functions)
		/// @param params		The parms to pass into the method call
		/// @returns If these params will result in a valid call
		///
		bool AreValidParams(Prism::Holder target = nullptr, const std::vector<Prism::Holder>& params = {}) const;
	};
}