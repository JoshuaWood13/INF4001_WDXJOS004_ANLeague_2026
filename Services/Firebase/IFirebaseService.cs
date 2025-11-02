namespace INF4001_WDXJOS004_ANLeague_2026.Services.Firebase
{
    public interface IFirebaseService
    {
        Task<T?> GetDocumentAsync<T>(string collection, string documentId) where T : class;
        Task<List<T>> GetCollectionAsync<T>(string collection) where T : class;
        Task<string> AddDocumentAsync<T>(string collection, T document) where T : class;
        Task UpdateDocumentAsync<T>(string collection, string documentId, T document) where T : class;
        Task DeleteDocumentAsync(string collection, string documentId);
        Task<List<T>> QueryCollectionAsync<T>(string collection, string field, object value) where T : class;
    }
}
