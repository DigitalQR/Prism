///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "Accessor.h"
#include "AttributeStore.h"
#include "TypeInfo.h"
#include "Object.h"

#include <vector>

namespace Prism
{
	struct PRISMCORE_API ParamInfo
	{
		String Name;
		TypeInfo Type;
		bool IsPointer : 1;
		bool IsConst : 1;
	};

	class PRISMCORE_API Method : public AttributeStore
	{
	private:
		const String m_Name;
		const String m_Documentation;
		const Accessor m_Accessor;

	protected:
		const bool m_IsStatic : 1;
		const bool m_IsConst : 1;
		const bool m_IsVirtual : 1;

		Method(const String& name, const String& documentation, const std::vector<const Attribute*>& attributes, Accessor accessor, bool isStatic, bool isConst, bool isVirtual);

	public:
		inline const String& GetName() const { return m_Name; }
		inline const String& GetDocumentation() const { return m_Documentation; }

		inline bool IsStatic() const { return m_IsStatic; }
		inline bool IsConst() const { return m_IsConst; }
		inline bool IsVirtual() const { return m_IsVirtual; }
		inline Accessor GetAccessor() const { return m_Accessor; }

		///
		/// Get the prism type info for the parent of this property
		/// @returns The Prism::TypeInfo, if found or nullptr
		///
		virtual TypeInfo GetParentInfo() const = 0;

		///
		/// Get the return type info for this method
		/// @returns The param info for the return type
		///
		virtual const Prism::ParamInfo* GetReturnInfo() const = 0;

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
		virtual Prism::Object Call(Prism::Object target = nullptr, const std::vector<Prism::Object>& params = {}) const = 0;

		///
		/// Check that these params will result in a valid call
		/// @param target		The class instance which this method will be called on (Will be ignored for static/global functions)
		/// @param params		The parms to pass into the method call
		/// @returns If these params will result in a valid call
		///
		bool AreValidParams(Prism::Object target = nullptr, const std::vector<Prism::Object>& params = {}) const;
	};
}