using System.Text;

namespace StegSharp.Infrastructure.Util.Extensions
{
    public static class StringExtensions
    {
        public static int GetBitLength(this string str)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(str);

            return messageBytes.Length * 8;
        }
    }
}
