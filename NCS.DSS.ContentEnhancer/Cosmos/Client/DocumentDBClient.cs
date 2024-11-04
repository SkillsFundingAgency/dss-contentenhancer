using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentEnhancer.Cosmos.Client
{
    public class DocumentDBClient : IDocumentDBClient
    {
        private DocumentClient _documentClient;
        private readonly string _serviceEndpoint = Environment.GetEnvironmentVariable("Endpoint");
        private readonly string _authorisationKey = Environment.GetEnvironmentVariable("Key");

        public DocumentClient CreateDocumentClient()
        {
            return _documentClient != null ? _documentClient : new DocumentClient(new Uri(_serviceEndpoint), _authorisationKey);
        }
    }
}