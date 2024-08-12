using Project.Helpers.Attributes;
using Project.Managers.Store;
using Project.Systems.Ads;
using Project.Systems.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Project.Extensions
{
    public static class EnumExtensions
    {
        public static T Parse<T>(string value, T defaultValue = default(T), bool ignoreCase = true) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("Type T must be enum");

            if (string.IsNullOrEmpty(value))
            {
                LogSystem.Trace($"Can't parse [{typeof(T).Name}] type, value is empty");
                return defaultValue;
            }

            if (Enum.TryParse<T>(value, ignoreCase, out var result))
            {
                return result;
            }

            LogSystem.Error($"Can't parse [{value}] to type [{typeof(T).Name}]");
            return defaultValue;
        }

        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }

        public static bool HasAnyFlag(this Enum e)
        {
            var intValue = Convert.ToInt32(e);

            return intValue != 0;
        }

        public static T[] ToArray<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static Enum[] ToArray(this Enum en)
        {
            return (Enum[])Enum.GetValues(en.GetType());
        }

        public static bool IsVictory(this Enum en)
        {
            return en.GetAttributeOfType<VictoryAttribute>() != null;
        }

        public static bool IsGameplay(this Enum en)
        {
            return en.GetAttributeOfType<GameplayAttribute>() != null;
        }

        public static bool IsLoading(this Enum en)
        {
            return en.GetAttributeOfType<LoadingAttribute>() != null;
        }

        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static T GetIteratedValue<T>(int newIndex)
        {
            var enumLength = Enum.GetNames(typeof(T)).Length;

            if (newIndex < 0)
            {
                return (T)Enum.Parse(typeof(T), $"{enumLength - 1}");
            }

            //
            if (newIndex >= enumLength)
            {
                return (T)Enum.Parse(typeof(T), "0");
            }

            //
            return (T)Enum.Parse(typeof(T), $"{newIndex}");
        }

        public static T Prev<T>(this T e) where T : Enum
        {
            var enumValues = (T[])Enum.GetValues(typeof(T));

            var nextIndex = Array.IndexOf(enumValues, e) - 1;

            return (nextIndex < 0) ? enumValues.Last() : enumValues[nextIndex];
        }

        public static T Next<T>(this T e) where T : Enum
        {
            var enumValues = (T[])Enum.GetValues(typeof(T));

            var nextIndex = Array.IndexOf(enumValues, e) + 1;

            return (enumValues.Length == nextIndex) ? enumValues.First() : enumValues[nextIndex];
        }
    }
}