namespace Infrastructure.Util.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static int Read2Bytes(this BinaryReader reader)
        {
            var upper = reader.ReadByte();
            var lower = reader.ReadByte();

            var result = (upper << 8) | lower;

            return result;
        }
    }
}
