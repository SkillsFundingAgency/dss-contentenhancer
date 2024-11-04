using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentEnhancer.Cosmos.Client
{
    public class DocumentDBClient : IDocumentDBClient
    {
        private DocumentClient _documentClient;

        public DocumentClient CreateDocumentClient()
        {
            string serviceEndpoint = Environment.GetEnvironmentVariable("Endpoint");
            string authorisationKey = Environment.GetEnvironmentVariable("Key");

            return _documentClient != null ? _documentClient : new DocumentClient(new Uri(serviceEndpoint), authorisationKey);
        }
    }
}