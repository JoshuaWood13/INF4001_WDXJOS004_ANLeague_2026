using Google.Cloud.Firestore;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class CommentaryMoment
    {
        [FirestoreProperty("minute")]
        public int Minute { get; set; }

        [FirestoreProperty("type")]
        public CommentaryType Type { get; set; }

        [FirestoreProperty("description")]
        public string Description { get; set; } = string.Empty; 

        [FirestoreProperty("homeScore")]
        public int HomeScore { get; set; }

        [FirestoreProperty("awayScore")]
        public int AwayScore { get; set; }

        [FirestoreProperty("playerId")]
        public string? PlayerId { get; set; } 

        [FirestoreProperty("playerName")]
        public string? PlayerName { get; set; } 
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//