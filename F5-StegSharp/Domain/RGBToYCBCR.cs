namespace StegSharp.Domain
{
    public static class RGBToYCBCR
    {
        public static float CalculateY(byte r, byte g, byte b)
        {
            return (float)(0.299 * r + 0.587 * g + 0.114 * b);
        }

        public static float CalculateCB(byte r, byte g, byte b)
        {
            return (float)(128 + (-0.16874 * r - 0.33126 * g + 0.5 * b));
        }

        public static float CalculateCR(byte r, byte g, byte b)
        {
            return (float)(128 + (0.5 * r - 0.41869 * g - 0.08131 * b));
        }
    }
}