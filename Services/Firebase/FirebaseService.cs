using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<FirebaseService> _logger;

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public FirebaseService(FirestoreDb firestoreDb, ILogger<FirebaseService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Get a single document by ID
        public async Task<T?> GetDocumentAsync<T>(string collection, string documentId) where T : class
        {
            try
            {
                var doc = await _firestoreDb.Collection(collection).Document(documentId).GetSnapshotAsync();

                if (doc.Exists)
                {
                    return doc.ConvertTo<T>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting document {documentId} from collection {collection}");
                throw;
            }
        }

        // Get all documents in a collection
        public async Task<List<T>> GetCollectionAsync<T>(string collection) where T : class
        {
            try
            {
                var col = await _firestoreDb.Collection(collection).GetSnapshotAsync();

                return col.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting collection {collection}");
                throw;
            }
        }

        // Add a new document to Firestore with auto-generated ID
        public async Task<string> AddDocumentAsync<T>(string collection, T document) where T : class
        {
            try
            {
                var col = _firestoreDb.Collection(collection);
                var newDoc = await col.AddAsync(document);

                _logger.LogInformation($"Added document to collection {collection} with ID {newDoc.Id}");
                return newDoc.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding document to collection {collection}");
                throw;
            }
        }

        // Add a new document to Firestore with specific ID
        public async Task AddDocumentWithIdAsync<T>(string collection, string documentId, T document) where T : class
        {
            try
            {
                var newDoc = _firestoreDb.Collection(collection).Document(documentId);
                await newDoc.SetAsync(document);

                _logger.LogInformation($"Added document to collection {collection} with ID {documentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding document with ID {documentId} to collection {collection}");
                throw;
            }
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
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//