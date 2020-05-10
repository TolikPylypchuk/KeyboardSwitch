using System;
using System.Collections.Generic;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Converters
{
    public sealed class NumberConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            var numberTypes = new HashSet<Type>
            {
                typeof(int?),
                typeof(uint?),
                typeof(short?),
                typeof(ushort?),
                typeof(long?),
                typeof(ulong?),
                typeof(byte?),
                typeof(sbyte?),
                typeof(float?),
                typeof(double?)
            };

            return fromType == typeof(string) && numberTypes.Contains(toType) ||
                   numberTypes.Contains(fromType) && toType == typeof(string)
                ? 10
                : 0;
        }

        public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
        {
            string fromAsString = from?.ToString() ?? String.Empty;

            result = toType == typeof(string)
                ? fromAsString
                : this.ConvertFromString(fromAsString, toType);

            return true;
        }

        private object? ConvertFromString(string from, Type toType)
            => toType switch
            {
                var t when t == typeof(int?) => Int32.TryParse(from, out int result) ? (object)result : null,
                var t when t == typeof(uint?) => UInt32.TryParse(from, out uint result) ? (object)result : null,
                var t when t == typeof(short?) => Int16.TryParse(from, out short result) ? (object)result : null,
                var t when t == typeof(ushort?) => UInt16.TryParse(from, out ushort result) ? (object)result : null,
                var t when t == typeof(long?) => Int64.TryParse(from, out long result) ? (object)result : null,
                var t when t == typeof(ulong?) => UInt64.TryParse(from, out ulong result) ? (object)result : null,
                var t when t == typeof(byte?) => Byte.TryParse(from, out byte result) ? (object)result : null,
                var t when t == typeof(sbyte?) => SByte.TryParse(from, out sbyte result) ? (object)result : null,
                var t when t == typeof(float?) => Single.TryParse(from, out float result) ? (object)result : null,
                var t when t == typeof(double?) => Double.TryParse(from, out double result) ? (object)result : null,
                _ => null
            };
    }
}
