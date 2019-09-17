using Prism.Reflection.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Parser.Cpp.Token
{
	/// <summary>
	/// A specific behaviour which can be parsed into producing token info
	/// </summary>
	public interface ICppTokenType
	{
		ICppTokenType Parent { get; set; }
		string Identifier { get; }

		bool Accept(CppTokenReader reader, ref string input, out CppTokenInfo tokenInfo);
	}

	/// <summary>
	/// Parsed info to be processed via the CppTokenReader
	/// </summary>
	public class CppTokenInfo
	{
#if DEBUG
		public string m_Source;
#endif
		public long m_LineNumber;
		public ICppTokenType m_TokenType;
		public object m_ParsedData;

		public override string ToString()
		{
#if DEBUG
			return "(" + m_LineNumber + ") " + m_TokenType + ": " + m_Source;
#else
			return "(" + m_LineNumber + ") " + m_TokenType;
#endif
		}

		public ElementOrigin ToElementOrigin()
		{
			return new ElementOrigin((int)m_LineNumber);
		}
	}

	/// <summary>
	/// A generic handler for token info
	/// </summary>
	/// <typeparam name="DataType">Parsed data type to add to the token in</typeparam>
	public abstract class CppTokenType<DataType> : ICppTokenType
	{
		private ICppTokenType m_Parent;

		public string Identifier { get { return GetType().Name; } }
		public ICppTokenType Parent { get => m_Parent; set => m_Parent = value; }

		protected abstract bool Accept(CppTokenReader reader, ref string input, out DataType data);

		public virtual bool Accept(CppTokenReader reader, ref string input, out CppTokenInfo tokenInfo)
		{
			DataType data;
			bool result = Accept(reader, ref input, out data);

			if (result)
			{
				tokenInfo = new CppTokenInfo();
				tokenInfo.m_ParsedData = data;
				tokenInfo.m_TokenType = this;
#if DEBUG
				tokenInfo.m_Source = input;
#endif
				return true;
			}

			tokenInfo = null;
			return result;
		}

		public override string ToString()
		{
			return Identifier;
		}
	}

	/// <summary>
	/// Information for a block that has opened/finished
	/// </summary>
	public class CppBlockInfo
	{
		public object m_ParsedData;
		public bool m_IsBlockBegin;
	}

	/// <summary>
	/// A token type which can add special behaviour to only run when this block is active
	/// </summary>
	public class CppBlockToken<BaseType> : ICppTokenType where BaseType : ICppTokenType, new()
	{
		private BaseType m_BaseType;
		private IEnumerable<ICppTokenType> m_InternalTypes;
		private IEnumerable<ICppTokenType> m_ElseInternalTypes;
		private Stack<Tuple<int, object>> m_ActiveBlocks;

		public CppBlockToken(params ICppTokenType[] internalTypes)
		{
			m_BaseType = new BaseType();
			m_InternalTypes = internalTypes;
			m_ElseInternalTypes = new ICppTokenType[0];
			m_ActiveBlocks = new Stack<Tuple<int, object>>();

			foreach (ICppTokenType type in m_InternalTypes)
			{
				type.Parent = this;
			}
		}

		public string Identifier { get { return GetType().Name; } }
		public ICppTokenType Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public BaseType InternalType { get => m_BaseType; }
		public IEnumerable<Tuple<int, object>> ActiveBlocks { get => m_ActiveBlocks; }

		/// <summary>
		/// Types to only activate when a block isn't active
		/// </summary>
		public CppBlockToken<BaseType> WhenOutside(params ICppTokenType[] internalTypes)
		{
			m_ElseInternalTypes = m_ElseInternalTypes.Union(internalTypes);
			return this;
		}

		public bool Accept(CppTokenReader reader, ref string input, out CppTokenInfo tokenInfo)
		{
			// Check to see if a block is free
			if (m_ActiveBlocks.Any())
			{
				var info = m_ActiveBlocks.Peek();
				if (info.Item1 > reader.CurrentBlockDepth)
				{
					// Left block depth
					m_ActiveBlocks.Pop();

					tokenInfo = new CppTokenInfo();
					tokenInfo.m_ParsedData = new CppBlockInfo
					{
						m_IsBlockBegin = false,
						m_ParsedData = info.Item2
					};
					tokenInfo.m_TokenType = m_BaseType;
#if DEBUG
					tokenInfo.m_Source = "";
#endif
					input = "";
					return true;
				}

				// Check any internal behaviours
				foreach (ICppTokenType internalType in m_InternalTypes)
				{
					if (internalType.Accept(reader, ref input, out tokenInfo))
						return true;
				}
			}
			// Check to see if any behaviours that should be tested when block isn't active can be found
			else
			{
				foreach (ICppTokenType internalType in m_ElseInternalTypes)
				{
					if (internalType.Accept(reader, ref input, out tokenInfo))
						return true;
				}
			}

			// Check normal behaviour
			bool result = m_BaseType.Accept(reader, ref input, out tokenInfo);

			// Start of block
			if (result)
			{
				object data = tokenInfo.m_ParsedData;
				m_ActiveBlocks.Push(new Tuple<int, object>(reader.CurrentBlockDepth + 1, data));
				
				tokenInfo.m_ParsedData = new CppBlockInfo
				{
					m_IsBlockBegin = true,
					m_ParsedData = data
				};
				return true;
			}
			
			tokenInfo = null;
			return result;
		}
	}
}
