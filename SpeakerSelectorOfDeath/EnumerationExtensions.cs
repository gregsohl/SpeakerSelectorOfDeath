using System;

namespace SpeakerSelectorOfDeath
{
	/*	public static class EnumerationExtensions
		{
			public static bool Has<T>(this System.Enum type, T value)
			{
				try
				{
					return (((int)(object)type & (int)(object)value) == (int)(object)value);
				}
				catch
				{
					return false;
				}
			}

			public static bool Is<T>(this System.Enum type, T value)
			{
				try
				{
					return (int)(object)type == (int)(object)value;
				}
				catch
				{
					return false;
				}
			}

			public static T Add<T>(this System.Enum type, T value)
			{
				try
				{
					var newValue = (T)(object)(((int)(object)type | (int)(object)value));

					return newValue;
				}
				catch (Exception ex)
				{
					throw new ArgumentException(
						string.Format(
							"Could not append value from enumerated type '{0}'.",
							typeof(T).Name
						), ex);
				}
			}


			public static T Remove<T>(this System.Enum type, T value)
			{
				try
				{
					return (T)(object)(((int)(object)type & ~(int)(object)value));
				}
				catch (Exception ex)
				{
					throw new ArgumentException(
						string.Format(
							"Could not remove value from enumerated type '{0}'.",
							typeof(T).Name
						), ex);
				}
			}

		}*/

	public static class EnumerationExtensions
	{
		/// <summary>
		/// Checks if an enumerated type contains a value
		/// </summary>
		public static bool Has<T>(this Enum value, T check)
		{
			Type type = value.GetType();

			// determine the values
			EnumValue parsed = new EnumValue(check, type);

			if (parsed.Signed.HasValue)
			{
				return (Convert.ToInt64(value) &
						(long)parsed.Signed) == (long)parsed.Signed;
			}

			if (parsed.Unsigned.HasValue)
			{
				return (Convert.ToUInt64(value) &
						(ulong)parsed.Unsigned) == (ulong)parsed.Unsigned;
			}

			return false;
		}

		/// <summary>
		/// Includes an enumerated type and returns the new value
		/// </summary>
		public static T Include<T>(this Enum value, T append)
		{
			Type type = value.GetType();

			// determine the values
			object result = value;
			EnumValue parsed = new EnumValue(append, type);
			
			if (parsed.Signed is long)
			{
				result = Convert.ToInt64(value) | (long)parsed.Signed;
			}
			else if (parsed.Unsigned is ulong)
			{
				result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;
			}

			// return the final value
			return (T)Enum.Parse(type, result.ToString());
		}

		/// <summary>
		/// Checks if an enumerated type is missing a value
		/// </summary>
		public static bool Missing<T>(this Enum obj, T value)
		{
			return !Has(obj, value);
		}

		/// <summary>
		/// Removes an enumerated type and returns the new value
		/// </summary>
		public static T Remove<T>(this Enum value, T remove)
		{
			Type type = value.GetType();

			// determine the values
			object result = value;
			EnumValue parsed = new EnumValue(remove, type);
			
			if (parsed.Signed.HasValue)
			{
				result = Convert.ToInt64(value) & ~(long)parsed.Signed;
			}
			else if (parsed.Unsigned.HasValue)
			{
				result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;
			}

			// return the final value
			return (T)Enum.Parse(type, result.ToString());
		}

		#region Helper Classes

		// class to simplfy narrowing values between 
		// a ulong and long since either value should
		// cover any lesser value
		private class EnumValue
		{
			public EnumValue(object value, Type type)
			{
				// make sure it is even an enum to work with
				if (!type.IsEnum)
				{
					throw new
						ArgumentException("Value provided is not an enumerated type!");
				}

				// then check for the enumerated value
				Type compare = Enum.GetUnderlyingType(type);

				// if this is an unsigned long then the only
				// value that can hold it would be a ulong
				if (compare.Equals(_UInt32) || compare.Equals(_UInt64))
				{
					Unsigned = Convert.ToUInt64(value);
				}
				else
				{
					// otherwise, a long should cover anything else
					Signed = Convert.ToInt64(value);
				}

			}

			public long? Signed;

			public ulong? Unsigned;

			// cached comparisons for type comparison
			private static readonly Type _UInt32 = typeof(long);
			private static readonly Type _UInt64 = typeof(ulong);
		}

		#endregion

	}

}