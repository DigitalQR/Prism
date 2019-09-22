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
		struct PRISMCORE_API Value
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
		/// Construct a new object of this type
		/// @param params		The params that should be used in the constructor for this type
		/// @returns The newly constructed object or an empty holder if a valid constructor couldn't be found
		///
		virtual Prism::Object CreateNew(const std::vector<Prism::Object>& params = {}) const override;

		///
		/// Get value at a specific index i.e. the order they appear in code, not the actual value itself
		///
		inline const Value& GetValueAtIndex(int i) const { return m_Values[i]; }

		///
		/// Get the number of values stored in this enum
		///
		inline size_t GetValueCount() const { return m_Values.size(); }

	protected:
		Enum(long uniqueId, const String& space, const String& name, const String& documentation, size_t size, const std::vector<const Attribute*>& attributes, const std::vector<Value>& values);

		///
		/// Convert this given value into a string
		/// (Object will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual Prism::String ToString(Prism::Object inStorage) const override;

		///
		/// Attempt to parse a string into an instance of this type 
		/// (Object will already be pre-checked to make sure it is valid, when this is called)
		///
		virtual bool ParseFromString(const String& str, Prism::Object outStorage) const override;
	};
}
