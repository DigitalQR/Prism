using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Prism.Utils
{
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class CommandLineArgumentAttribute : Attribute
	{
		/// <summary>
		/// Name that is checked for in the form --my-name (Only my-name should be provided)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Name that is checked for in the form -myname (Only myname should be provided, no spaces '-' allowed)
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Usage for this argument (Output when parse fails)
		/// </summary>
		public string Usage { get; set; }

		/// <summary>
		/// Is this value required to exist
		/// </summary>
		public bool MustExist { get; set; }
	}

	public class CommandLineArgumentParseException : Exception
	{
		public string Usage;

		public CommandLineArgumentParseException(CommandLineArgumentAttribute arg, string message) : base(message)
		{
			if (string.IsNullOrEmpty(arg.Name))
			{
				Usage = "-" + arg.ShortName;
			}
			else if (string.IsNullOrEmpty(arg.ShortName))
			{
				Usage = "--" + arg.Name;
			}
			else
			{
				Usage = "--" + arg.Name + "/-" + arg.ShortName;
			}

			Usage = "(" + Usage + "): " + arg.Usage;
		}
	}

	public static class CommandLineArguments
	{
		private static string[] s_RawArgs;
		private static Dictionary<string, List<string>> s_ArgTable = new Dictionary<string, List<string>>();

		/// <summary>
		/// To be called once on the programs start up with any arguments passed in
		/// Expected to be in the format: -k=value or --my-key=value (With multiple of these existing for entering array information)
		/// </summary>
		public static void ProvideArguments(string[] args)
		{
			s_RawArgs = args;

			// Create dicitonary of -v/--value-key and values
			foreach (string arg in args)
			{
				if (arg.StartsWith("-") && arg.Contains('='))
				{
					string key = arg.Split('=')[0].ToLower();
					string value = arg.Substring(key.Length + 1);

					if (s_ArgTable.ContainsKey(key))
					{
						s_ArgTable[key].Add(value);
					}
					else
					{
						List<string> values = new List<string>();
						values.Add(value);
						s_ArgTable.Add(key, values);
					}
				}
			}
		}

		/// <summary>
		/// Set all properties of a given object which has a CommandLineArgumentAttribute
		/// </summary>
		/// <param name="obj">The target obj where CommandLineArgumentAttribute should be applied to properties</param>
		public static void FillValues(object obj) 
		{
			// Apply the correct values
			foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(prop => prop.IsDefined(typeof(CommandLineArgumentAttribute), true)))
			{
				CommandLineArgumentAttribute arg = (CommandLineArgumentAttribute)field.GetCustomAttributes(typeof(CommandLineArgumentAttribute), true)[0];
				bool isArray = field.FieldType.IsArray;
				bool found = false;

				// Look for full name
				if (!string.IsNullOrEmpty(arg.Name))
				{
					string key = "--" + arg.Name.ToLower().Replace(' ', '-');
					if (s_ArgTable.ContainsKey(key))
					{
						found = true;
						ParseField(obj, arg, field, s_ArgTable[key]);
					}
				}

				// Look for short name
				if (!string.IsNullOrEmpty(arg.ShortName))
				{
					string key = "-" + arg.ShortName.ToLower().Replace(" ", "");
					if (s_ArgTable.ContainsKey(key))
					{
						if (found)
						{
							throw new CommandLineArgumentParseException(arg, "Both full and short arg form found (Do not mix)");
						}

						found = true;
						ParseField(obj, arg, field, s_ArgTable[key]);
					}
				}

				if (!found && arg.MustExist)
				{
					throw new CommandLineArgumentParseException(arg, "Required argument is not present");
				}
			}
		}

		/// <summary>
		/// Attempt to parse value(s) for a single field of an object
		/// </summary>
		private static void ParseField(object obj, CommandLineArgumentAttribute arg, FieldInfo field, List<string> rawValues)
		{
			bool isArray = field.FieldType.IsArray;

			// Do not allow multiple values if the target is not an array
			if (!isArray && rawValues.Count != 1)
			{
				throw new CommandLineArgumentParseException(arg, "Found multiple entries for '" + field.Name + "' (Only single values allowed for non-arrays)");
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
						throw new CommandLineArgumentParseException(arg, "Invalid entry format '" + rawValue + "' (Expected type: " + elementType.Name + ")");
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
					throw new CommandLineArgumentParseException(arg, "Invalid format (Expected type: " + field.FieldType.Name + ")");
				}
			}
		}
	}
}
