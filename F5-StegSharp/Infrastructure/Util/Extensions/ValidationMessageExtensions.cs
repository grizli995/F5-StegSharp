namespace StegSharp.Infrastructure.Util.Extensions
{
    public static class ValidationMessageExtensions
    {
        public static string ToArgumentNullExceptionMessage(this string input)
        {
            return $"{input} cannot be null.";
        }

        public static string ToArgumentEqualsZeroExceptionMessage(this string input)
        {
            return $"{input} must be greater than 0.";
        }
    }
}
