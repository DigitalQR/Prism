#include "Include\Prism\CommonTypes.h"

#define IMPLEMENT_PRISM_SOURCE(Namespace,StructName,TypeName) \
TypeInfo_ ## StructName::TypeInfo_ ## StructName() \
	: Prism::Type(Prism::TypeId::Get<TypeName>(), PRISM_STR(#Namespace), PRISM_STR(#StructName), PRISM_DEVSTR("Internally reflected type for '" #TypeName "'"), sizeof(TypeName), false, false) \
{} \
Prism::Holder TypeInfo_ ## StructName::CreateNew(const std::vector<Prism::Holder>& params) const \
{ \
	if(params.size() == 0) return new TypeName; \
	else if(params.size() == 1 && params[0].GetTypeInfo() == m_AssociatedInfo) return new TypeName(params[0].GetAs<TypeName>()); \
	else return nullptr; \
} \
TypeInfo_ ## StructName TypeInfo_ ## StructName::s_AssemblyInstance;

namespace Prism
{
	namespace Common
	{
		TypeInfo_nullptr::TypeInfo_nullptr() 
			: Prism::Type(
				0, 
				PRISM_STR(""), PRISM_STR("null"), PRISM_DEVSTR("null type info"), 
				0, false, false
			) 
		{}
		
		Prism::Holder TypeInfo_nullptr::CreateNew(const std::vector<Prism::Holder>& params) const
		{ 
			return nullptr; 
		}

		TypeInfo_nullptr TypeInfo_nullptr::s_AssemblyInstance;


		FOREACH_PRISM_COMMONTYPE(IMPLEMENT_PRISM_SOURCE)
#undef IMPLEMENT_PRISM_SOURCE

		Prism::String TypeInfo_char::ToString(Prism::Holder inStorage) const 
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<char>());
#else
			return std::to_string(inStorage.GetAs<char>());
#endif
		}
		bool TypeInfo_char::ParseFromString(const String& str, Prism::Holder outStorage) const 
		{
			char& outValue = *outStorage.GetPtrAs<char>();
			outValue = (char)str[0];
			return true;
		}

		Prism::String TypeInfo_uchar::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<unsigned char>());
#else
			return std::to_string(inStorage.GetAs<unsigned char>());
#endif
		}
		bool TypeInfo_uchar::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			unsigned char& outValue = *outStorage.GetPtrAs<unsigned char>();
			outValue = (unsigned char)str[0];
			return true;
		}

		Prism::String TypeInfo_short::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<short>());
#else
			return std::to_string(inStorage.GetAs<short>());
#endif
		}
		bool TypeInfo_short::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			short& outValue = *outStorage.GetPtrAs<short>();
			outValue = (short)std::stoi(str);
			return true;
		}

		Prism::String TypeInfo_ushort::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<unsigned short>());
#else
			return std::to_string(inStorage.GetAs<unsigned short>());
#endif
		}
		bool TypeInfo_ushort::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			unsigned short& outValue = *outStorage.GetPtrAs<unsigned short>();
			outValue = (unsigned short)std::stoi(str);
			return true;
		}

		Prism::String TypeInfo_int::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<int>());
#else
			return std::to_string(inStorage.GetAs<int>());
#endif
		}
		bool TypeInfo_int::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			int& outValue = *outStorage.GetPtrAs<int>();
			outValue = std::stoi(str);
			return true;
		}

		Prism::String TypeInfo_uint::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<unsigned int>());
#else
			return std::to_string(inStorage.GetAs<unsigned int>());
#endif
		}
		bool TypeInfo_uint::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			unsigned int& outValue = *outStorage.GetPtrAs<unsigned int>();
			outValue = (unsigned int)std::stol(str);
			return true;
		}


		Prism::String TypeInfo_long::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<long>());
#else
			return std::to_string(inStorage.GetAs<long>());
#endif
		}
		bool TypeInfo_long::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			long& outValue = *outStorage.GetPtrAs<long>();
			outValue = std::stol(str);
			return true;
		}

		Prism::String TypeInfo_ulong::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<unsigned long>());
#else
			return std::to_string(inStorage.GetAs<unsigned long>());
#endif
		}
		bool TypeInfo_ulong::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			unsigned long& outValue = *outStorage.GetPtrAs<unsigned long>();
			outValue = std::stoul(str);
			return true;
		}

		Prism::String TypeInfo_float::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<float>());
#else
			return std::to_string(inStorage.GetAs<float>());
#endif
		}
		bool TypeInfo_float::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			float& outValue = *outStorage.GetPtrAs<float>();
			outValue = std::stof(str);
			return true;
		}

		Prism::String TypeInfo_double::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return std::to_wstring(inStorage.GetAs<double>());
#else
			return std::to_string(inStorage.GetAs<double>());
#endif
		}
		bool TypeInfo_double::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			double& outValue = *outStorage.GetPtrAs<double>();
			outValue = std::stod(str);
			return true;
		}

		Prism::String TypeInfo_string::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			const std::string& str = *inStorage.GetPtrAs<std::string>();
			return std::wstring(str.begin(), str.end());
#else
			return inStorage.GetAs<std::string>();
#endif
		}
		bool TypeInfo_string::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			std::string& outValue = *outStorage.GetPtrAs<std::string>();
			outValue = std::string(str.begin(), str.end());
			return true;
		}

		Prism::String TypeInfo_wstring::ToString(Prism::Holder inStorage) const
		{
#if PRISM_STR_WIDE
			return inStorage.GetAs<std::wstring>();
#else
			const std::wstring& str = *inStorage.GetPtrAs<std::wstring>();
			return std::string(str.begin(), str.end());
#endif
		}
		bool TypeInfo_wstring::ParseFromString(const String& str, Prism::Holder outStorage) const
		{
			std::wstring& outValue = *outStorage.GetPtrAs<std::wstring>();
			outValue = std::wstring(str.begin(), str.end());
			return true;
		}

	}
}
