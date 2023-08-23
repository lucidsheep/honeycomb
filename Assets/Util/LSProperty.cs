
using System;
using UnityEngine.Events;

public class LSProperty<T>
{
    public delegate T SetDelegate(T before, T value);
    protected T t;
    protected SetDelegate d;
    public UnityEvent<T, T> onChange = new UnityEvent<T,T>();
    public T property { get { return t; } set { var before = t; t = d(t, value); onChange.Invoke(before, t); }}

    public static implicit operator T(LSProperty<T> p) => p.property;

    public LSProperty()
    {
        t = default(T);
        d = (before, value) => { return value; };
    }
    public LSProperty(T initialValue)
    {
        t = initialValue;
        d = (before, value) => {return value; };
    }

    public LSProperty(T initialValue, SetDelegate dg)
    {
        t = initialValue;
        d = dg;
    }

    public override string ToString()
    {
        return t.ToString();
    }
}