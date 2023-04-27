using JpegLibrary;

namespace Application.Common.Interfaces
{
    public interface IPermutationService
    {
        JpegBlock8x8F[] PermutateArray(string password, JpegBlock8x8F[] inputArray, bool reverse);
    }
}
