namespace Postica.Common
{
    /// <summary>
    /// Wrapper class which keeps an internal struct value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Ref<T> where T : struct
    {
        public T Value;
        
        public Ref(T value)
        {
            Value = value;
        }
        
        public static implicit operator T(Ref<T> reference)
        {
            return reference.Value;
        }
        
        public static implicit operator Ref<T>(T value)
        {
            return new Ref<T>(value);
        }
    }
}
