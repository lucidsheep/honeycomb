using System;
using System.Collections.Generic;

public abstract class LSRollingAverage<x> where x : IEquatable<x>
{
    protected LinkedList<x> list;
    protected x sum;
    protected int size;
    abstract public x average { get; } //{ get { return sum / (float)size; } }
    public x highest { get { return GetHighOrLow(true); } }
    public x lowest { get { return GetHighOrLow(false); } }

    protected abstract int CompareTo(x orig, x comparison);
    x GetHighOrLow(bool high)
    {
        var node = list.First;
        x best = node.Value;
        do
        {
            if ((high && CompareTo(node.Value, best) > 0) || (!high && CompareTo(node.Value, best) < 0)) best = node.Value;
            node = node.Next;
        } while (node != null);
        return best;
    }
    public LSRollingAverage()
    {
        InitList(1, default(x));
    }
    public LSRollingAverage(int s)
    {
        InitList(s, default(x));
    }
    public LSRollingAverage(int s, x defaultValue)
    {
        InitList(s, defaultValue);
    }

    void InitList(int s, x value)
    {
        if (s <= 0) s = 1;
        size = s;
        sum = default(x);
        list = new LinkedList<x>();
        for (int i = 0; i < size; i++)
        {
            list.AddLast(value);
            sum = AddToSum(sum, value);
        }
    }
    protected abstract x AddToSum(x lhs, x rhs);
    protected abstract x SubFromSum(x lhs, x rhs);
    public void AddValue(x val)
    {
        sum = AddToSum(SubFromSum(sum, list.First.Value), val); 
        list.RemoveFirst();
        list.AddLast(val);
    }
}