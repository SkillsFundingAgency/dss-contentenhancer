using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentEnhancer.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}