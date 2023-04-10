using Application.Models;

namespace Application.Common.Interfaces
{
    public interface IEncodingOrchestratorService
    {
        public void EncodeData(DCTData quantizedDCTData, BinaryWriter bw);
    }
}
