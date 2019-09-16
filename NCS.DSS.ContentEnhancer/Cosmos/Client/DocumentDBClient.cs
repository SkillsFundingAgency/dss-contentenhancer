using System;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentEnhancer.Cosmos.Client
{
    public class DocumentDBClient : IDocumentDBClient
    {
        private DocumentClient _documentClient;

        public DocumentClient CreateDocumentClient()
        {
            if (_documentClient != null)
                return _documentClient;

            _documentClient = new DocumentClient(new Uri(
                Environment.GetEnvironmentVariable("Endpoint")),
            Environment.GetEnvironmentVariable("Key"));

            return _documentClient;
        }

    }
}
