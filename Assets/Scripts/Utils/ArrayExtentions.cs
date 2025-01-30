namespace KKL.Utils
{
    public static class ArrayExtentions
    {
        public static void Default<T>(this T[] array, T defaultValue)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = defaultValue;
            }
        }
    }
}