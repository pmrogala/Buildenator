namespace Buildenator.Abstraction.Helpers
{
    public readonly struct Nullbox<T>
    {
        public T? Object { get; }

        public Nullbox(T? value)
        {
            Object = value;
        }

        public static implicit operator Nullbox<T?>(T? obj) => new (obj);
        public static explicit operator T?(Nullbox<T?> nullbox) => nullbox.Object;
    }
}
