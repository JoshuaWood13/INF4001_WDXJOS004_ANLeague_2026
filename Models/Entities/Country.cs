using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class Country
    {
        public Country() { }

        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("federationRepresentativeId")]
        public string FederationRepresentativeId { get; set; }

        [FirestoreProperty("managerName")]
        public string? ManagerName { get; set; } 

        [FirestoreProperty("captainId")]
        public string? CaptainId { get; set; } 

        [FirestoreProperty("players")]
        public List<Player>? Players { get; set; } 

        [FirestoreProperty("rating")]
        public double Rating { get; set; } 

        [FirestoreProperty("isRegisteredForCurrentTournament")]
        public bool IsRegisteredForCurrentTournament { get; set; }

        [FirestoreProperty("isTeamComplete")]
        public bool IsTeamComplete { get; set; } 

        [FirestoreProperty("statistics")]
        public TeamStatistics Statistics { get; set; } = new TeamStatistics();

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//