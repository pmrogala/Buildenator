namespace Buildenator.Abstraction.Helpers;

public readonly struct NullBox<T>
{
    public T? Object { get; }

    public NullBox(T? value)
    {
            Object = value;
        }

    public static implicit operator NullBox<T?>(T? obj) => new (obj);
    public static explicit operator T?(NullBox<T?> nullBox) => nullBox.Object;
}