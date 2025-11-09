using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class TournamentBracketViewModel
    {
        // Tournament Info
        public string TournamentId { get; set; } = string.Empty;
        public int TournamentNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public int RegisteredTeamsCount { get; set; }
        public bool CanStart { get; set; } // 8 teams + registration status
        public bool IsStarted { get; set; } // In progress or completed
        public bool CanRestart { get; set; } // Must be started or in progress
        public bool ShowRemoveButtons { get; set; } // Admin + registration status
        
        // User Info
        public bool IsAuthenticated { get; set; }
        public bool IsRepresentative { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsUserTeamRegistered { get; set; }
        public string? UserCountryId { get; set; }
        public string? UserCountryName { get; set; }

        // Matches
        public MatchViewModel QF1 { get; set; } = new MatchViewModel { Id = "QF1" };
        public MatchViewModel QF2 { get; set; } = new MatchViewModel { Id = "QF2" };
        public MatchViewModel QF3 { get; set; } = new MatchViewModel { Id = "QF3" };
        public MatchViewModel QF4 { get; set; } = new MatchViewModel { Id = "QF4" };
        public MatchViewModel SF1 { get; set; } = new MatchViewModel { Id = "SF1" };
        public MatchViewModel SF2 { get; set; } = new MatchViewModel { Id = "SF2" };
        public MatchViewModel Final { get; set; } = new MatchViewModel { Id = "Final" };
        
        public int SeedCounter { get; set; } = 1;
    }

    public class MatchViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public CountrySlotInfo? HomeCountry { get; set; }
        public CountrySlotInfo? AwayCountry { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string? WinnerId { get; set; }
        public bool IsHomeSlotAvailable { get; set; }
        public bool IsAwaySlotAvailable { get; set; }
        public bool IsMatchCompleted => Status == "Completed" || Status == "Finished" || !string.IsNullOrEmpty(WinnerId);
        public bool CanBePlayed { get; set; }
    }

    // Country Info
    public class CountrySlotInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ManagerName { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//