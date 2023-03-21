namespace Domain
{
    public static class RGBToYCBCR
    {
        public static byte CalculateY(byte r, byte g, byte b)
        {
            return (byte)(0.299 * r + 0.587 * g + 0.114 * b);
        }

        public static byte CalculateCB(byte r, byte g, byte b)
        {
            return (byte)(128 + (-0.16874 * r - 0.33126 * g + 0.5 * b));
        }

        public static byte CalculateCR(byte r, byte g, byte b)
        {
            return (byte)(128 + (0.5 * r - 0.41869 * g - 0.08131 * b));
        }
    }
}