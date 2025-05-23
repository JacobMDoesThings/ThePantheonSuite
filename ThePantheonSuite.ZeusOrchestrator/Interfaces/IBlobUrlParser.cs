using ThePantheonSuite.ZeusOrchestrator.Data;

namespace ThePantheonSuite.ZeusOrchestrator.Interfaces;

public interface IBlobUrlParser
{
    public BlobData ParseBlob(Uri blobUrl);
}