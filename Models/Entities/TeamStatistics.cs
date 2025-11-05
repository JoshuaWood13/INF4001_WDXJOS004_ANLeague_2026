using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class TeamStatistics
    {
        public TeamStatistics() { }  

        [FirestoreProperty]
        public int TournamentsWon { get; set; }

        [FirestoreProperty]
        public int MatchesPlayed { get; set; }

        [FirestoreProperty]
        public int Wins { get; set; }

        [FirestoreProperty]
        public int Losses { get; set; }

        [FirestoreProperty]
        public int Draws { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//