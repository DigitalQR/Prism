using Prism.CodeParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public enum ReflectionErrorCode
	{
		GenericError				= 100,

		TokenError					= 200,
		TokenMissuse				= 201,

		ParseError					= 300,
		ParseFormatError			= 301,
		ParseUnexpectedSignature	= 302,
		ParseExpectedInclude		= 303,
	}

	public class ReflectionException : Exception
	{
		public ReflectionErrorCode ErrorCode { get; private set; }
		public SignatureInfo Signature { get; private set; }


		public ReflectionException(ReflectionErrorCode errorCode, SignatureInfo signature, string message)
			: base(message)
		{
			ErrorCode = errorCode;
			Signature = signature;
		}
	}

	public class HeaderReflectionException : Exception
	{
		public ReflectionErrorCode ErrorCode { get; private set; }
		public SignatureInfo Signature { get; private set; }
		public string FilePath { get; private set; }

		public HeaderReflectionException(string filePath, ReflectionErrorCode errorCode, SignatureInfo signature, Exception exception)
			: base(exception.Message, exception)
		{
			FilePath = filePath;
			ErrorCode = errorCode;
			Signature = signature;
		}
	}
}
