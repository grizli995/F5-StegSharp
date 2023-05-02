using Application.Models;

namespace Application.Common.Interfaces
{
    public interface IF5ExtractingService
    {
        string Extract(DCTData dctData, string password);
    }
}
