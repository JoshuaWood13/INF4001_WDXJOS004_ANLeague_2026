using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class Match
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("tournamentId")]
        public string TournamentId { get; set; } = string.Empty;

        [FirestoreProperty("round")]
        public string Round { get; set; } = string.Empty;

        [FirestoreProperty("homeCountryId")]
        public string HomeCountryId { get; set; } = string.Empty;

        [FirestoreProperty("awayCountryId")]
        public string AwayCountryId { get; set; } = string.Empty;

        [FirestoreProperty("homeScore")]
        public int HomeScore { get; set; }

        [FirestoreProperty("awayScore")]
        public int AwayScore { get; set; }

        [FirestoreProperty("status")]
        public string Status { get; set; } = string.Empty;

        [FirestoreProperty("matchType")]
        public string MatchType { get; set; } = string.Empty;

        [FirestoreProperty("goalScorers")]
        public List<Goal> GoalScorers { get; set; } = new List<Goal>();

        [FirestoreProperty("commentary")]
        public List<CommentaryMoment> Commentary { get; set; } = new List<CommentaryMoment>(); 

        [FirestoreProperty("matchDate")]
        public DateTime MatchDate { get; set; }

        [FirestoreProperty("winnerId")]
        public string? WinnerId { get; set; } 
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//