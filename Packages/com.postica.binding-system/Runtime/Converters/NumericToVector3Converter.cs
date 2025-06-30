using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Postica.BindingSystem.Converters
{
    /// <summary>
    /// Converts a number into a vector value, allows to select which component to apply to.
    /// </summary>
    internal sealed class NumericToVector3Converter : NumericToVectorConverter<Vector3>
    {
        [SerializeField]
        [Tooltip("The vector to apply the value to. The value will be multiplied by this vector.")]
        private ReadOnlyBind<Vector3> _multiplyBy = Vector3.one.Bind();

        public override Vector3 Convert(float value) => _multiplyBy.Value * value;
    }
    
    /// <summary>
    /// Converts a number into a vector value, allows to select which component to apply to.
    /// </summary>
    internal sealed class NumericToVector2Converter : NumericToVectorConverter<Vector2>
    {
        [SerializeField]
        [Tooltip("The vector to apply the value to. The value will be multiplied by this vector.")]
        private ReadOnlyBind<Vector2> _multiplyBy = Vector2.one.Bind();

        public override Vector2 Convert(float value) => _multiplyBy.Value * value;
    }
    
    /// <summary>
    /// Converts a number into a vector value, allows to select which component to apply to.
    /// </summary>
    internal sealed class NumericToVector4Converter : NumericToVectorConverter<Vector4>
    {
        [SerializeField]
        [Tooltip("The vector to apply the value to. The value will be multiplied by this vector.")]
        private ReadOnlyBind<Vector4> _multiplyBy = Vector4.one.Bind();

        public override Vector4 Convert(float value) => _multiplyBy.Value * value;
    }
    
    /// <summary>
    /// Converts a number into a vector value, allows to select which component to apply to.
    /// </summary>
    internal sealed class NumericToVector3IntConverter : NumericToVectorConverter<Vector3Int>
    {
        [SerializeField]
        [Tooltip("The vector to apply the value to. The value will be multiplied by this vector.")]
        private ReadOnlyBind<Vector3Int> _multiplyBy = Vector3Int.one.Bind();

        public override Vector3Int Convert(float value) => _multiplyBy.Value * (int)value;
    }
    
    /// <summary>
    /// Converts a number into a vector value, allows to select which component to apply to.
    /// </summary>
    internal sealed class NumericToVector2IntConverter : NumericToVectorConverter<Vector2Int>
    {
        [SerializeField]
        [Tooltip("The vector to apply the value to. The value will be multiplied by this vector.")]
        private ReadOnlyBind<Vector2Int> _multiplyBy = Vector2Int.one.Bind();

        public override Vector2Int Convert(float value) => _multiplyBy.Value * (int)value;
    }
    
    /// <summary>
    /// Converts a number into a vector value, allows to select which component to apply to.
    /// </summary>
    internal sealed class NumericToColorConverter : NumericToVectorConverter<Color>
    {
        [SerializeField]
        [Tooltip("The color to apply the value to. The value will be multiplied by this color.")]
        private ReadOnlyBind<Color> _multiplyBy = Color.white.Bind();
        [SerializeField]
        [Tooltip("The normalized range to apply the value to.")]
        private ReadOnlyBind<Vector2> _normalizeBy = Vector2.up.Bind();

        public override Color Convert(float value)
        {
            // First normalize the value
            var normalizedValue = Mathf.InverseLerp(_normalizeBy.Value.x, _normalizeBy.Value.y, value);
            return _multiplyBy.Value * normalizedValue;
        }
    }
    
    [HideMember]
    [Serializable]
    public abstract class NumericToVectorConverter<T> : 
        IConverter<float, T>,
        IConverter<int, T>,
        IConverter<double, T>,
        IConverter<long, T>,
        IConverter<short, T>,
        IConverter<byte, T>,
        IConverter<sbyte, T>,
        IConverter<uint, T>,
        IConverter<ulong, T>,
        IConverter<ushort, T>,
        IConverter<IValueProvider<float>, T>,
        IConverter<IValueProvider<int>, T>,
        IConverter<IValueProvider<double>, T>,
        IConverter<IValueProvider<long>, T>,
        IConverter<IValueProvider<short>, T>,
        IConverter<IValueProvider<byte>, T>,
        IConverter<IValueProvider<sbyte>, T>,
        IConverter<IValueProvider<uint>, T>,
        IConverter<IValueProvider<ulong>, T>,
        IConverter<IValueProvider<ushort>, T>
    {
        public string Id { get; } = $"Numeric to {typeof(T).Name} Converter";

        public string Description => "Converts a numeric value to a bool value, if the value is greater or equal to the threshold, it returns true";

        public bool IsSafe => true;

        public T Convert(int value) => Convert((float)value);
        public T Convert(double value) => Convert((float)value);
        public T Convert(long value) => Convert((float)value);
        public T Convert(short value) => Convert((float)value);
        public T Convert(byte value) => Convert((float)value);
        public T Convert(sbyte value) => Convert((float)value);
        public T Convert(uint value) => Convert((float)value);
        public T Convert(ulong value) => Convert((float)value);
        public T Convert(ushort value) => Convert((float)value);
        
        public T Convert(IValueProvider<float> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<int> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<double> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<long> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<short> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<byte> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<sbyte> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<uint> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<ulong> value) => Convert((float)value.Value);
        public T Convert(IValueProvider<ushort> value) => Convert((float)value.Value);

        public object Convert(object value)
        {
            return Convert((float)value);
        }

        public abstract T Convert(float value);
    }
}