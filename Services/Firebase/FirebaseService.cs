namespace INF4001_WDXJOS004_ANLeague_2026.Services.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        // TODO: Implement Firebase CRUD operations
        public Task<T?> GetDocumentAsync<T>(string collection, string documentId) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> GetCollectionAsync<T>(string collection) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<string> AddDocumentAsync<T>(string collection, T document) where T : class
        {
            throw new NotImplementedException();
        }

        public Task UpdateDocumentAsync<T>(string collection, string documentId, T document) where T : class
        {
            throw new NotImplementedException();
        }

        public Task DeleteDocumentAsync(string collection, string documentId)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> QueryCollectionAsync<T>(string collection, string field, object value) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
