namespace AdaptiveMeshes
{
    public static class LINQExtensions
    {
        public static void ThreadSafeSet(this double[] array, int index, double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Value is NAN.");

            double initialValue, computedValue;

            do
            {
                initialValue = array[index];
                computedValue = value;
            }
            while (initialValue != Interlocked.CompareExchange(ref array[index], computedValue, initialValue));
        }

        public static void ThreadSafeAdd(this double[] array, int index, double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentException("Value is NAN.");

            double initialValue, computedValue;

            do
            {
                initialValue = array[index];
                computedValue = initialValue + value;
            }
            while (initialValue != Interlocked.CompareExchange(ref array[index], computedValue, initialValue));
        }
    }
}
