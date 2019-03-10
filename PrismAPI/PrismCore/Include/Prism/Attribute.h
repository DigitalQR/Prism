///
/// TODO - Write
///
/// MIT License
/// Copyright (c) 2018 Sam Potter
///
#pragma once
#include "Definitions.h"

namespace Prism
{
	///
	/// Base class for Attributes which can be attached during reflection then queried
	/// (Child classes are expected to append Attribute to the class name e.g. IsTestableAttribute)
	///
	class PRISMCORE_API Attribute
	{
	public:
		enum class Usage : unsigned char
		{
			Nothing		= 0,
			Structs		= 1,
			Classes		= 2,
			Enums		= 4,
			Methods		= 8,
			Properties	= 16,

			DataStructures  = Structs | Classes,
			DataMembers		= Methods | Properties,

			Anything	= 255
		};

		enum class Behaviour : unsigned char
		{
			None			= 0,
			Inherited		= 1,
			MultipleAllowed = 2,
		};

	protected:
		const Usage m_Usage;
		const Behaviour m_Behaviour;

		Attribute(Usage usage = Usage::Anything, Behaviour behaviour = Behaviour::None);
	public:
		///
		/// Does this attribute support a given usage
		///
		inline bool SupportsUsage(Usage usage) const 
		{
			return (static_cast<unsigned char>(m_Usage) & static_cast<unsigned char>(usage)) != 0;
		}

		///
		/// What is the acceptable usage for this
		///
		inline Usage GetUsageFlags() const { return m_Usage; }

		///
		/// What is the behaviour of this attribute
		///
		inline Behaviour GetBehaviourFlags() const { return m_Behaviour; }

		///
		/// Can this attribute be inherited from a parent store
		///
		inline bool CanInherit() const;

		///
		/// Are multiple of these attributes allowed
		///
		inline bool IsMultipleAllowed() const;
	};

	///
	/// Bitwise support
	///
	inline Attribute::Usage operator|(Attribute::Usage lhs, Attribute::Usage rhs)
	{
		using T = std::underlying_type<Attribute::Usage>::type;
		return static_cast<Attribute::Usage>(static_cast<T>(lhs) | static_cast<T>(rhs));
	}

	inline Attribute::Usage operator&(Attribute::Usage lhs, Attribute::Usage rhs)
	{
		using T = std::underlying_type<Attribute::Usage>::type;
		return static_cast<Attribute::Usage>(static_cast<T>(lhs) & static_cast<T>(rhs));
	}

	inline Attribute::Usage operator^(Attribute::Usage lhs, Attribute::Usage rhs)
	{
		using T = std::underlying_type<Attribute::Usage>::type;
		return static_cast<Attribute::Usage>(static_cast<T>(lhs) ^ static_cast<T>(rhs));
	}

	///
	/// Bitwise support
	///
	inline Attribute::Behaviour operator|(Attribute::Behaviour lhs, Attribute::Behaviour rhs)
	{
		using T = std::underlying_type<Attribute::Behaviour>::type;
		return static_cast<Attribute::Behaviour>(static_cast<T>(lhs) | static_cast<T>(rhs));
	}

	inline Attribute::Behaviour operator&(Attribute::Behaviour lhs, Attribute::Behaviour rhs)
	{
		using T = std::underlying_type<Attribute::Behaviour>::type;
		return static_cast<Attribute::Behaviour>(static_cast<T>(lhs) & static_cast<T>(rhs));
	}

	inline Attribute::Behaviour operator^(Attribute::Behaviour lhs, Attribute::Behaviour rhs)
	{
		using T = std::underlying_type<Attribute::Behaviour>::type;
		return static_cast<Attribute::Behaviour>(static_cast<T>(lhs) ^ static_cast<T>(rhs));
	}


	bool Attribute::CanInherit() const 
	{ 
		return (m_Behaviour & Behaviour::Inherited) != (Behaviour)0;
	}

	bool Attribute::IsMultipleAllowed() const 
	{ 
		return (m_Behaviour & Behaviour::MultipleAllowed) != (Behaviour)0;
	}
}