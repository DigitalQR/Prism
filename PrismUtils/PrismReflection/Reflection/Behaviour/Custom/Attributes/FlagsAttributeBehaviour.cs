using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Reflection.Tokens;

namespace Prism.Reflection.Behaviour.Custom.Attributes
{
	/// <summary>
	/// Generates boiler plate for enum flags
	/// </summary>
	public class FlagsAttributeBehaviour : AttributeReflectionBehaviour
	{
		public FlagsAttributeBehaviour()
			: base("Flags", BehaviourTarget.Enumurator, -1)
		{
		}

		public override void RunBehaviour(IReflectableToken target, AttributeData data)
		{
			EnumToken token = target as EnumToken;

			if (token == null)
			{
				throw new Exception("Invalid state reached. FlagsAttributeBehaviour expected a EnumToken target");
			}

			token.AppendIncludeContent(@"
#if $(PreProcessorCondition)
#if $(HasInternalType)
enum $(StructureType) $(Name) : $(InternalType);
#else
enum $(StructureType) $(Name);
#endif

inline $(Name) operator|($(Name) lhs, $(Name) rhs)
{
	using T = std::underlying_type<$(Name)>::type;
	return static_cast<$(Name)>(static_cast<T>(lhs) | static_cast<T>(rhs));
}
inline $(Name) operator|=($(Name)& lhs, $(Name) rhs)
{
	using T = std::underlying_type<$(Name)>::type;
	return (lhs = static_cast<$(Name)>(static_cast<T>(lhs) | static_cast<T>(rhs)));
}

inline $(Name) operator&($(Name) lhs, $(Name) rhs)
{
	using T = std::underlying_type<$(Name)>::type;
	return static_cast<$(Name)>(static_cast<T>(lhs) & static_cast<T>(rhs));
}
inline $(Name) operator&=($(Name)& lhs, $(Name) rhs)
{
	using T = std::underlying_type<$(Name)>::type;
	return (lhs = static_cast<$(Name)>(static_cast<T>(lhs) & static_cast<T>(rhs)));
}

inline $(Name) operator^($(Name) lhs, $(Name) rhs)
{
	using T = std::underlying_type<$(Name)>::type;
	return static_cast<$(Name)>(static_cast<T>(lhs) ^ static_cast<T>(rhs));
}
inline $(Name) operator^=($(Name)& lhs, $(Name) rhs)
{
	using T = std::underlying_type<$(Name)>::type;
	return (lhs = static_cast<$(Name)>(static_cast<T>(lhs) ^ static_cast<T>(rhs)));
}

namespace Prism
{
	namespace Flags
	{
		inline bool IsEmpty($(Name) lhs)
		{
			using T = std::underlying_type<$(Name)>::type;
			return static_cast<T>(lhs) == static_cast<T>(0);
		}
	}
}
#endif
");
		}
	}
}
