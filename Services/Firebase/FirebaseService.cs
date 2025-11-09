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

        // Update an existing document in Firestore
        public async Task UpdateDocumentAsync<T>(string collection, string documentId, T document) where T : class
        {
            try
            {
                var docRef = _firestoreDb.Collection(collection).Document(documentId);
                await docRef.SetAsync(document, SetOptions.MergeAll);

                _logger.LogInformation($"Updated document {documentId} in collection {collection}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document {documentId} in collection {collection}");
                throw;
            }
        }

        public Task DeleteDocumentAsync(string collection, string documentId)
        {
            throw new NotImplementedException();
        }

        // Query a collection by a specific field value and return document ID with entity
        public async Task<List<(T entity, string documentId)>> QueryCollectionWithIdsAsync<T>(string collection, string field, object value) where T : class
        {
            try
            {
                var query = _firestoreDb.Collection(collection).WhereEqualTo(field, value);
                var snapshot = await query.GetSnapshotAsync();

                return snapshot.Documents.Select(doc => (doc.ConvertTo<T>(), doc.Id)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error querying collection {collection} by field {field}");
                throw;
            }
        }

        // Query a collection by a specific field value
        public async Task<List<T>> QueryCollectionAsync<T>(string collection, string field, object value) where T : class
        {
            try
            {
                var query = _firestoreDb.Collection(collection).WhereEqualTo(field, value);
                var snapshot = await query.GetSnapshotAsync();

                return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error querying collection {collection} by field {field}");
                throw;
            }
        }

        // Get all documents in a collection with their document IDs
        public async Task<List<(T entity, string documentId)>> GetCollectionWithIdsAsync<T>(string collection) where T : class
        {
            try
            {
                var snapshot = await _firestoreDb.Collection(collection).GetSnapshotAsync();

                return snapshot.Documents.Select(doc => (doc.ConvertTo<T>(), doc.Id)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting collection {collection} with IDs");
                throw;
            }
        }

        // Get the most recent document from a collection ordered by a field
        public async Task<T?> GetMostRecentDocumentAsync<T>(string collection, string orderByField, int limit = 1) where T : class
        {
            try
            {
                var query = _firestoreDb.Collection(collection)
                    .OrderByDescending(orderByField)
                    .Limit(limit);

                var snapshot = await query.GetSnapshotAsync();
                var document = snapshot.Documents.FirstOrDefault();

                if (document != null)
                {
                    return document.ConvertTo<T>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting most recent document from {collection} ordered by {orderByField}");
                throw;
            }
        }

        // Get the most recent document from a collection ordered by a field, with document ID
        public async Task<(T? entity, string documentId)?> GetMostRecentDocumentWithIdAsync<T>(string collection, string orderByField, int limit = 1) where T : class
        {
            try
            {
                var query = _firestoreDb.Collection(collection)
                    .OrderByDescending(orderByField)
                    .Limit(limit);

                var snapshot = await query.GetSnapshotAsync();
                var document = snapshot.Documents.FirstOrDefault();

                if (document != null && document.Exists)
                {
                    return (document.ConvertTo<T>(), document.Id);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting most recent document with ID from {collection} ordered by {orderByField}");
                throw;
            }
        }

        // Get multiple documents by their IDs
        public async Task<Dictionary<string, T>> GetDocumentsByIdsAsync<T>(string collection, List<string> documentIds) where T : class
        {
            try
            {
                if (documentIds == null || !documentIds.Any())
                {
                    return new Dictionary<string, T>();
                }

                var uniqueIds = documentIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

                if (!uniqueIds.Any())
                {
                    return new Dictionary<string, T>();
                }

                var docRefs = uniqueIds.Select(id => _firestoreDb.Collection(collection).Document(id)).ToList();
                var snapshots = await _firestoreDb.GetAllSnapshotsAsync(docRefs);

                var result = new Dictionary<string, T>();
                foreach (var snapshot in snapshots)
                {
                    if (snapshot.Exists)
                    {
                        result[snapshot.Id] = snapshot.ConvertTo<T>();
                    }
                }

                _logger.LogInformation($"Retrieved {result.Count} documents out of {uniqueIds.Count} requested from {collection}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error batch getting documents from {collection}");
                throw;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//