using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class Player
    {
        public Player() { }

        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("name")]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty("naturalPosition")]
        public string NaturalPosition { get; set; } = string.Empty;

        [FirestoreProperty("ratings")]
        public PlayerRatings Ratings { get; set; } = new PlayerRatings();

        [FirestoreProperty("isCaptain")]
        public bool IsCaptain { get; set; }

        [FirestoreProperty("goalsScored")]
        public int goalsScored { get; set; } = 0;
        }
}
