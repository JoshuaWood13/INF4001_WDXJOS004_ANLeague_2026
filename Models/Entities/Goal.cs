using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class Goal
    {
        [FirestoreProperty("playerId")]
        public string PlayerId { get; set; } = string.Empty;

        [FirestoreProperty("playerName")]
        public string PlayerName { get; set; } = string.Empty;

        [FirestoreProperty("countryId")]
        public string CountryId { get; set; } = string.Empty;

        [FirestoreProperty("minute")]
        public int Minute { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//