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
#include "Holder.h"

namespace Prism 
{
	class TypeInfo;

	class PRISMCORE_API Property : public AttributeStore
	{
	private:
		const String m_Name;
		const String m_Documentation;
		const Accessor m_Accessor;

	protected:
		const bool m_IsPointer : 1;
		const bool m_IsStatic : 1;
		const bool m_IsConst : 1;

		Property(const String& name, const String& documentation, const std::vector<const Attribute*>& attributes, Accessor accessor, bool isPointer, bool isStatic, bool isConst);
	public:
		inline const String& GetName() const { return m_Name; }
		inline const String& GetDocumentation() const { return m_Documentation; }

		inline bool IsPointer() const { return m_IsPointer; }
		inline bool IsStatic() const { return m_IsStatic; }
		inline bool IsConst() const { return m_IsConst; }
		inline Accessor GetAccessor() const { return m_Accessor; }

		///
		/// Get the prism type info for the parent of this property
		/// @returns The Prism::TypeInfo, if found or nullptr
		///
		virtual TypeInfo GetParentInfo() const = 0;

		///
		/// Get the prism type info for this property
		/// @returns The Prism::TypeInfo, if found or nullptr
		///
		virtual TypeInfo GetTypeInfo() const = 0;

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