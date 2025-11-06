using Google.Cloud.Firestore;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    [FirestoreData]
    public class PlayerRatings
    {
        public PlayerRatings() { }

        [FirestoreProperty]
        public int GK { get; set; }

        [FirestoreProperty]
        public int DF { get; set; } 

        [FirestoreProperty]
        public int MD { get; set; } 

        [FirestoreProperty]
        public int AT { get; set; } 
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//