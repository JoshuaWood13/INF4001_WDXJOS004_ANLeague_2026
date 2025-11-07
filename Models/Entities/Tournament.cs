using Google.Cloud.Firestore;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class Tournament
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty; 

        [FirestoreProperty("tournamentNumber")]
        public int TournamentNumber { get; set; } 

        [FirestoreProperty("status")]
        public string Status { get; set; } = string.Empty;

        [FirestoreProperty("startDate")] 
        public DateTime StartDate { get; set; }

        [FirestoreProperty("endDate")]
        public DateTime? EndDate { get; set; } 

        [FirestoreProperty("registeredCountries")]
        public List<string> RegisteredCountries { get; set; } = new List<string>(); // Country IDs

        [FirestoreProperty("originalQuarterFinalCountries")]
        public List<string> OriginalQuarterFinalCountries { get; set; } = new List<string>(); // For easier restarts

        [FirestoreProperty("bracket")]
        public TournamentBracket Bracket { get; set; } = new TournamentBracket();

        [FirestoreProperty("matches")]
        public Dictionary<string, Match> Matches { get; set; } = new Dictionary<string, Match>();
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//