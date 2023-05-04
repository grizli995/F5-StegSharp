namespace Application.Models
{
    public class YCBCRData
    {
        public float[,] YData { get; set; }
        public float[,] CRData { get; set; }
        public float[,] CBData { get; set; }

        public YCBCRData(int width, int height)
        {
            YData = new float[height, width];
            CRData = new float[height, width];
            CBData = new float[height, width];
        }
    }
}
