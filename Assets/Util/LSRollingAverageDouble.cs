public class LSRollingAverageDouble : LSRollingAverage<double>
{
    public override double average => sum / (double)size;

    protected override double AddToSum(double lhs, double rhs)
    {
        return lhs + rhs;
    }

    protected override double SubFromSum(double lhs, double rhs)
    {
        return lhs - rhs;
    }
    protected override int CompareTo(double orig, double comparison)
    {
        return orig.CompareTo(comparison);
    }
    public LSRollingAverageDouble(int count, double val) : base(count, val) { }
}