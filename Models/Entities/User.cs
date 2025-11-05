using Google.Cloud.Firestore;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("role")]
        public string Role { get; set; } = string.Empty;

        [FirestoreProperty("countryId")]
        public string? CountryId { get; set; } // nullable, for Representatives

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
