using System;

namespace NCS.DSS.ContentEnhancer.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid? customerId);

    }
}