using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Postica.BindingSystem.Converters
{
    /// <summary>
    /// This class contains all <see cref="FormatStringConverter{T}"/> converters for default types.
    /// </summary>
    public static class FormatStringConverters
    {
        private sealed class FormatStringConverter_Byte     : FormatStringConverter<byte> { }
        private sealed class FormatStringConverter_Ushort   : FormatStringConverter<ushort> { }
        private sealed class FormatStringConverter_Short    : FormatStringConverter<short> { }
        private sealed class FormatStringConverter_Uint     : FormatStringConverter<uint> { }
        private sealed class FormatStringConverter_Int      : FormatStringConverter<int> { }
        private sealed class FormatStringConverter_Ulong    : FormatStringConverter<ulong> { }
        private sealed class FormatStringConverter_Long     : FormatStringConverter<long> { }
        private sealed class FormatStringConverter_Float    : FormatStringConverter<float> { }
        private sealed class FormatStringConverter_Double   : FormatStringConverter<double> { }
        private sealed class FormatStringConverter_Vector2   : FormatStringConverter<Vector2> { }
        private sealed class FormatStringConverter_Vector2I   : FormatStringConverter<Vector2Int> { }
        private sealed class FormatStringConverter_Vector3   : FormatStringConverter<Vector3> { }
        private sealed class FormatStringConverter_Vector3I   : FormatStringConverter<Vector3Int> { }
        private sealed class FormatStringConverter_Vector4   : FormatStringConverter<Vector4> { }
        private sealed class FormatStringConverter_Color        : FormatStringConverter<Color> { }
        private sealed class FormatStringConverter_Color32   : FormatStringConverter<Color32> { }
        private sealed class FormatStringConverter_Rect         : FormatStringConverter<Rect> { }
        private sealed class FormatStringConverter_RectInt   : FormatStringConverter<RectInt> { }
        private sealed class FormatStringConverter_Bounds       : FormatStringConverter<Bounds> { }
        private sealed class FormatStringConverter_BoundsInt   : FormatStringConverter<BoundsInt> { }
        private sealed class FormatStringConverter_DateTime   : FormatStringConverter<DateTime> { }
        private sealed class FormatStringConverter_TimeSpan   : FormatStringConverter<TimeSpan> { }

        /// <summary>
        /// Registers all defined converters
        /// </summary>
        public static void RegisterDefaultTypes()
        {
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Byte>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Ushort>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Short>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Uint>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Int>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Ulong>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Long>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Float>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Double>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Vector2>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Vector2I>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Vector3>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Vector3I>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Vector4>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Color>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Color32>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Rect>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_RectInt>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_Bounds>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_BoundsInt>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_DateTime>();
            ConvertersFactory.RegisterTemplate<FormatStringConverter_TimeSpan>();
        }
    }

    /// <summary>
    /// A converter which transforms a given <typeparamref name="T"/> value into a string using the specified format.
    /// </summary>
    /// <typeparam name="T">The type of the value to be formatted</typeparam>
    [Serializable]
    [HideMember]
    public class FormatStringConverter<T> : IConverter<T, string> where T : IFormattable
    {
        [FormerlySerializedAs("_format")] [SerializeField]
        protected string _valueFormat;

        /// <summary>
        /// The format to use for this converter.
        /// </summary>
        /// <remarks>The format should be compatible with <see cref="IFormattable"/> <typeparamref name="T"/> type</remarks>
        public string Format { get => _valueFormat; set => _valueFormat = value; }

        /// <inheritdoc/>
        public string Id { get; } = "Format String";

        /// <inheritdoc/>
        public string Description { get; } = "Formats a value into a string with specified format." +
            "\nThe format is passed as a parameter to the 'ToString()' method, and not as a format to string.Format() method";
        
        /// <inheritdoc/>
        public bool IsSafe => true;

        /// <inheritdoc/>
        public string Convert(T value) => string.IsNullOrEmpty(_valueFormat) ? value.ToString() : value.ToString(_valueFormat, NumberFormatInfo.CurrentInfo);

        /// <inheritdoc/>
        public object Convert(object value)
        {
            if (!string.IsNullOrEmpty(_valueFormat) && value is IFormattable formattable)
            {
                return formattable.ToString(_valueFormat, NumberFormatInfo.CurrentInfo);
            }
            return value?.ToString();
        }
    }

    /// <summary>
    /// A converter which transforms any given value into a string using the specified format.
    /// </summary>
    [Serializable]
    [HideMember]
    public class FormatStringConverter : IConverter<object, string>
    {
        [SerializeField]
        protected string _format;

        /// <summary>
        /// The format to use for this converter in case the value is of <see cref="IFormattable"/> type.
        /// </summary>
        /// <remarks>The format should be compatible with <see cref="IFormattable"/> type</remarks>
        public string Format { get => _format; set => _format = value; }

        /// <inheritdoc/>
        public string Id { get; } = "Format String";

        /// <inheritdoc/>
        public string Description { get; } = "Formats a value into a string with specified format." +
            "\nThe format is passed as a parameter to the 'ToString()' method, and not as a format to string.Format() method";

        /// <inheritdoc/>
        public bool IsSafe => true;

        /// <inheritdoc/>
        public object Convert(object value)
        {
            if(!string.IsNullOrEmpty(_format) && value is IFormattable formattable)
            {
                return formattable.ToString(_format, NumberFormatInfo.CurrentInfo);
            }
            return value?.ToString();
        }

        /// <inheritdoc/>
        string IConverter<object, string>.Convert(object value)
        {
            if (!string.IsNullOrEmpty(_format) && value is System.IFormattable formattable)
            {
                return formattable.ToString(_format, NumberFormatInfo.CurrentInfo);
            }
            return value?.ToString();
        }
    }
}
