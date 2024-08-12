using Project.Systems.FeatureFlag;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Extensions
{
	public static class FeatureFlagExtensions
	{
		public static string ToFeatureFlagKey(this Enum @enum)
		{
			return @enum.ToString().ToSnakeCase();
		}

		public static T FromFeatureFlagKey<T>(this string str) where T : struct, Enum
		{
			return EnumExtensions.Parse<T>(str.ToCamelCase());
		}

		public static void Join(this FeatureFlagConfig config, FeatureFlagConfig other)
		{
			if (other == null || other.Flags == null || other.Flags.Length == 0)
			{
				return;
			}

			for (var i = 0; i < config.Flags.Length; i++)
			{
				var flag = config.Flags[i];
				var otherFlag = other.Flags.FirstOrDefault(x => flag.Key.Equals(x.Key));
				if (otherFlag != null)
				{
					config.Flags[i] = otherFlag;
				}
				else if (otherFlag != null && flag != otherFlag)
				{
					EnumerableExtensions.Add(ref config.Flags, otherFlag);
				}
			}

			foreach (var flag in other.Flags)
			{
				if (!config.Flags.Contains(flag))
				{
					EnumerableExtensions.Add(ref config.Flags, flag);
				}
			}
		} 
	}
}
