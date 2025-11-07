using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class TournamentBracket
    {
        [FirestoreProperty("quarterFinals")]
        public List<string> QuarterFinals { get; set; } = new List<string>(); 

        [FirestoreProperty("semiFinals")]
        public List<string> SemiFinals { get; set; } = new List<string>(); 

        [FirestoreProperty("final")]
        public string Final { get; set; } = string.Empty; 
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//