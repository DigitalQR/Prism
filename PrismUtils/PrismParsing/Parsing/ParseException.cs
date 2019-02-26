using Prism.Parsing.Code;
using Prism.Parsing.Code.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parsing
{
	public enum ParseErrorCode
	{
		GenericError				= 100,

		TokenError					= 200,
		TokenMissuse				= 201,
		TokenUnsupportedUsage		= 202,

		ParseError					= 300,
		ParseFormatError			= 301,
		ParseUnexpectedSignature	= 302,
		ParseExpectedInclude		= 303,
	}

	public class ParseException : Exception
	{
		public ParseErrorCode ErrorCode { get; private set; }
		public SignatureInfo Signature { get; private set; }


		public ParseException(ParseErrorCode errorCode, SignatureInfo signature, string message)
			: base(message)
		{
			ErrorCode = errorCode;
			Signature = signature;
		}
	}

	public class HeaderParseException : Exception
	{
		public ParseErrorCode ErrorCode { get; private set; }
		public SignatureInfo Signature { get; private set; }
		public string FilePath { get; private set; }

		public HeaderParseException(string filePath, ParseErrorCode errorCode, SignatureInfo signature, Exception exception)
			: base(exception.Message, exception)
		{
			FilePath = filePath;
			ErrorCode = errorCode;
			Signature = signature;
		}
	}
}
