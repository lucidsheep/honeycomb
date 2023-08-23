public class LSRollingAverageFloat : LSRollingAverage<float>
{
    public override float average => sum / (float)size;

    protected override float AddToSum(float lhs, float rhs)
    {
        return lhs + rhs;
    }

    protected override float SubFromSum(float lhs, float rhs)
    {
        return lhs - rhs;
    }
    protected override int CompareTo(float orig, float comparison)
    {
        return orig.CompareTo(comparison);
    }
    public LSRollingAverageFloat(int count, float val) : base(count, val) { }
}