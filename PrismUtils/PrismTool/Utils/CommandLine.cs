using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Prism.Utils
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class CmdArgAttribute : Attribute
	{
		/// <summary>
		/// Name that is checked for in the form --my-name (Only my-name should be provided)
		/// </summary>
		public string Arg { get; set; }

		/// <summary>
		/// Name that is checked for in the form -myname (Only myname should be provided, no spaces '-' allowed)
		/// </summary>
		public string ShortArg { get; set; }

		/// <summary>
		/// Usage for this argument (Output when parse fails)
		/// </summary>
		public string Usage { get; set; }

		/// <summary>
		/// Is this value required to exist
		/// </summary>
		public bool MustExist { get; set; }
	}

	public class CmdArgParseException : Exception
	{
		public string Usage;

		public CmdArgParseException(CmdArgAttribute arg, string message) : base(message)
		{
			if (string.IsNullOrEmpty(arg.Arg))
			{
				Usage = "-" + arg.ShortArg;
			}
			else if (string.IsNullOrEmpty(arg.ShortArg))
			{
				Usage = "--" + arg.Arg;
			}
			else
			{
				Usage = "--" + arg.Arg + "/-" + arg.ShortArg;
			}

			Usage = "(" + Usage + "): " + arg.Usage;
		}
	}

	public class CmdArgs
	{
		/// <summary>
		/// Parse command arguments and set the appropriate values in obj's properties
		/// Expected to be in the format: -k=value or --my-key=value (With multiple of these existing for entering array information)
		/// </summary>
		/// <param name="obj">The target obj where CommandArugmentAttribute should be applied to properties</param>
		/// <param name="args">The raw cmd line arguments</param>
		public static void Parse(object obj, string[] args) 
		{
			// Create dicitonary of -v/--value-key and values
			Dictionary<string, List<string>> argTable = new Dictionary<string, List<string>>();

			foreach (string arg in args)
			{
				if (arg.StartsWith("-") && arg.Contains('='))
				{
					string key = arg.Split('=')[0].ToLower();
					string value = arg.Substring(key.Length + 1);

					if (argTable.ContainsKey(key))
					{
						argTable[key].Add(value);
					}
					else
					{
						List<string> values = new List<string>();
						values.Add(value);
						argTable.Add(key, values);
					}
				}
			}

			// Apply the correct values
			foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(prop => prop.IsDefined(typeof(CmdArgAttribute), true)))
			{
				CmdArgAttribute arg = (CmdArgAttribute)field.GetCustomAttributes(typeof(CmdArgAttribute), true)[0];
				bool isArray = field.FieldType.IsArray;
				bool found = false;

				// Look for full name
				if (!string.IsNullOrEmpty(arg.Arg))
				{
					string key = "--" + arg.Arg.ToLower().Replace(' ', '-');
					if (argTable.ContainsKey(key))
					{
						found = true;
						ParseField(obj, arg, field, argTable[key]);
					}
				}

				// Look for short name
				if (!string.IsNullOrEmpty(arg.ShortArg))
				{
					string key = "-" + arg.ShortArg.ToLower().Replace(" ", "");
					if (argTable.ContainsKey(key))
					{
						if (found)
						{
							throw new CmdArgParseException(arg, "Both full and short arg form found (Do not mix)");
						}

						found = true;
						ParseField(obj, arg, field, argTable[key]);
					}
				}

				if (!found && arg.MustExist)
				{
					throw new CmdArgParseException(arg, "Required argument is not present");
				}
			}
		}

		/// <summary>
		/// Attempt to parse value(s) for a single field of an object
		/// </summary>
		private static void ParseField(object obj, CmdArgAttribute arg, FieldInfo field, List<string> rawValues)
		{
			bool isArray = field.FieldType.IsArray;

			// Do not allow multiple values if the target is not an array
			if (!isArray && rawValues.Count != 1)
			{
				throw new CmdArgParseException(arg, "Found multiple entries for '" + field.Name + "' (Only single values allowed for non-arrays)");
			}

			// Try to parse as array
			if (isArray)
			{
				Type elementType = field.FieldType.GetElementType();
				Array values = Array.CreateInstance(elementType, rawValues.Count);

				int i = 0;
				foreach (string rawValue in rawValues)
				{
					try
					{
						object value = Convert.ChangeType(rawValue, elementType);
						values.SetValue(value, i++);
					}
					catch (FormatException)
					{
						throw new CmdArgParseException(arg, "Invalid entry format '" + rawValue + "' (Expected type: " + elementType.Name + ")");
					}
				}

				field.SetValue(obj, values);
			}

			// Try to parse as single value
			else
			{
				try
				{
					field.SetValue(obj, Convert.ChangeType(rawValues[0], field.FieldType));
				}
				catch (FormatException)
				{
					throw new CmdArgParseException(arg, "Invalid format (Expected type: " + field.FieldType.Name + ")");
				}
			}
		}
	}
}
