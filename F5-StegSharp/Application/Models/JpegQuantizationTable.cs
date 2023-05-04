namespace Application.Models
{
    public class JpegQuantizationTable
    {
        public int Id { get; set; }

        public byte[] Values { get; set; }

        public JpegQuantizationTable()
        {
            Values = new byte[64];
        }

        public JpegQuantizationTable(int id)
        {
            Id = id;
            Values = new byte[64];
        }
    }
}
