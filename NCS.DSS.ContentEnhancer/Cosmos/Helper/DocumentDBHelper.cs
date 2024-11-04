using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentEnhancer.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private Uri _documentCollectionUri;
        private Uri _documentUri;
        private readonly string _databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        private readonly string _collectionId = Environment.GetEnvironmentVariable("CollectionId");

        public Uri CreateDocumentCollectionUri()
        {
            return _documentCollectionUri != null ? _documentCollectionUri : UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        }

        public Uri CreateDocumentUri(Guid? customerId)
        {
            return _documentUri != null ? _documentUri : UriFactory.CreateDocumentUri(_databaseId, _collectionId, customerId.ToString());
        }
    }
}