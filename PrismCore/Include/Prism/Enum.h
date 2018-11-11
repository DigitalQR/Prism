///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"
#include "Type.h"

#include <vector>

namespace Prism 
{
	class PRISMCORE_API Enum : public Type
	{
	public:
		struct Value
		{
			String Name;
			size_t NumberValue;

			Value();
			Value(const String& name, size_t value);
		};

	private:
		const std::vector<Value> m_Values;

	public:
		///
		/// Get value at a specific index i.e. the order they appear in code, not the actual value itself
		///
		inline const Value& GetValueAtIndex(int i) const { return m_Values[i]; }

		///
		/// Get the number of values stored in this enum
		///
		inline size_t GetValueCount() const { return m_Values.size(); }

	protected:
		Enum(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, const std::vector<Value>& values);

		///
		/// Convert this given value into a string
		/// (Holder will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual Prism::String ToString(Prism::Holder inStorage) const override;

		///
		/// Attempt to parse a string into an instance of this type 
		/// (Holder will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual bool ParseFromString(const String& str, Prism::Holder outStorage) const override;
	};
}
