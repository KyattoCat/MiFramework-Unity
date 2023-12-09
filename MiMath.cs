namespace MiFramework.Math
{
    public static class MiMath
    {
        public static bool InRange(int i, int min, int max, bool includeCeil = false)
        {
            return i >= min && (includeCeil ? i <= max : i < max);
        }
        public static bool InRangeFromZero(int i, int max, bool includeCeil = false)
        {
            return i >= 0 && (includeCeil ? i <= max : i < max);
        }
    }
}