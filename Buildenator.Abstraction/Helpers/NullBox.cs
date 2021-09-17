namespace Buildenator.Abstraction.Helpers
{
    public readonly struct NullBox<T>
    {
        private readonly T _value;

        public NullBox(T value)
        {
            _value = value;
        }

        public static implicit operator T(NullBox<T> nullBox) => nullBox._value;
        public static implicit operator NullBox<T>(T nullBox) => new (nullBox);
    }
}
