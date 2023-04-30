using JpegLibrary;

namespace Application.Common.Interfaces
{
    public interface IF5ParameterCalculatorService
    {
        int CalculateK(JpegBlock8x8F[] mcus, string message);
        int CalculateN(int k);
    }
}
