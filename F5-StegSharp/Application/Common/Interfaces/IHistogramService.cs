namespace StegSharp.Application.Common.Interfaces
{
    public interface IHistogramService
    {
        public Dictionary<float, int> GetHistogramFromImagePath(string imagePath);
    }
}
