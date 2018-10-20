///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "Holder.h"

namespace Prism 
{
	class TypeInfo;

	class PRISMCORE_API Property
	{
	private:
		const String m_Name;
		const TypeInfo* m_TypeInfo;
	protected:
		const Type* m_OwningType;
		const bool m_IsPointer : 1;
		const bool m_IsStatic : 1;
		const bool m_IsConst : 1;

		Property(const String& name, const Type* owningType, const TypeInfo* type, bool isPointer, bool isStatic, bool isConst);
	public:
		inline const String& GetName() const { return m_Name; }
		inline const TypeInfo* GetTypeInfo() const { return m_TypeInfo; }
		inline bool IsPointer() const { return m_IsPointer; }
		inline bool IsStatic() const { return m_IsStatic; }
		inline bool IsConst() const { return m_IsConst; }

		///
		/// Attempt to set this property's value for the target
		/// Warning: This may crash if params are invalid
		/// @param target		The class instance which this property will be set (Will be ignored for static/global functions)
		/// @param value		The value it should be set to
		/// @returns The result for this call
		///
		virtual void Set(Prism::Holder target, Prism::Holder value) const = 0;

		///
		/// Attempt to get this property's value for the target
		/// Warning: This may crash if params are invalid
		/// @param target		The class instance which this property will be retrieved (Will be ignored for static/global functions)
		/// @returns The value of the property
		///
		virtual Prism::Holder Get(Prism::Holder target) const = 0;
	};
}