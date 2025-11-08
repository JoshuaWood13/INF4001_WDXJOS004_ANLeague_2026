using GenerativeAI;
using INF4001_WDXJOS004_ANLeague_2026.Data;
using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary
{
    public class AICommentaryService : IAICommentaryService
    {
        private readonly ILogger<AICommentaryService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly GoogleAi _googleAI;
        private readonly GenerativeModel _model;

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public AICommentaryService(ILogger<AICommentaryService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiKey = _configuration["AI:ApiKey"] ?? throw new InvalidOperationException("AI API Key not configured");
            _googleAI = new GoogleAi(_apiKey);
            _model = _googleAI.CreateGenerativeModel("models/gemini-2.5-flash");
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Get a detailed play-by-play commentary for a played match and return a compiled result 
        public async Task<MatchCommentaryResult> GetPlayMatchCommentaryAsync(string homeCountryId, string homeCountryName, List<Player> homePlayers, string awayCountryId, string awayCountryName, List<Player> awayPlayers)
        {
            _logger.LogInformation($"Generating play commentary for {homeCountryName} vs {awayCountryName}");

            // Create player dictionaries
            var homePlayerDict = homePlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);
            var awayPlayerDict = awayPlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);

            // Generate AI commentary (all events)
            var matchData = await GeneratePlayMatchCommentary(homeCountryName, homePlayers, awayCountryName, awayPlayers);

            // Compile result
            var result = CompileMatchResult(matchData, homeCountryId, awayCountryId, homePlayerDict, awayPlayerDict);

            _logger.LogInformation($"Generated {result.Commentary.Count} commentary moments. Final: {result.HomeScore}-{result.AwayScore}");

            return result;
        }

        // Get simple commentary for a simulated match and return a compiled result
        public async Task<MatchCommentaryResult> GetSimulateMatchCommentaryAsync(string homeCountryId, string homeCountryName, List<Player> homePlayers, string awayCountryId, string awayCountryName, List<Player> awayPlayers) 
        {
            _logger.LogInformation($"Generating simulate commentary for {homeCountryName} vs {awayCountryName}");

            // Create player dictionaries
            var homePlayerDict = homePlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);
            var awayPlayerDict = awayPlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);

            // Generate AI commentary (goals only)
            var matchData = await GenerateSimulateMatchCommentary(homeCountryName, homePlayers, awayCountryName, awayPlayers);

            // Compile result 
            var result = CompileMatchResult(matchData, homeCountryId, awayCountryId, homePlayerDict, awayPlayerDict);

            _logger.LogInformation($"Simulated match: {result.HomeScore}-{result.AwayScore}");

            return result;
        }

        // Generate AI play-by-play commentary for a match 
        private async Task<MatchDataResponse> GeneratePlayMatchCommentary(string homeTeam, List<Player> homePlayers, string awayTeam, List<Player> awayPlayers)
        {
            // Get all players by position
            var homeAttackers = GetPlayerNamesByPosition(homePlayers, "AT");
            var homeDefenders = GetPlayerNamesByPosition(homePlayers, "DF");
            var homeGoalkeepers = GetPlayerNamesByPosition(homePlayers, "GK");

            var awayAttackers = GetPlayerNamesByPosition(awayPlayers, "AT");
            var awayDefenders = GetPlayerNamesByPosition(awayPlayers, "DF");
            var awayGoalkeepers = GetPlayerNamesByPosition(awayPlayers, "GK");

            // Get prompt
            var prompt = AiPrompts.GetPlayMatchCommentaryPrompt(homeTeam, homeAttackers, homeDefenders, homeGoalkeepers, awayTeam, awayAttackers, awayDefenders, awayGoalkeepers);

            // Generate commentary
            var response = await _model.GenerateContentAsync(prompt);

            // Clean response
            var jsonText = CleanJsonResponse(response.Text);
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var result = JsonSerializer.Deserialize<MatchDataResponse>(jsonText, options);
                if (result == null)
                {
                    _logger.LogError("Failed to parse AI response - deserialization returned null");
                    throw new Exception("Failed to parse AI response - deserialization returned null");
                }
                
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON parsing error: {ex.Message}");
                throw new Exception($"Failed to parse AI response: {ex.Message}", ex);
            }
        }

        // Generate AI goal commentary for a simulated match
        private async Task<MatchDataResponse> GenerateSimulateMatchCommentary(string homeTeam, List<Player> homePlayers, string awayTeam, List<Player> awayPlayers)
        {
            // Get attackers only
            var homeAttackers = GetPlayerNamesByPosition(homePlayers, "AT");
            var awayAttackers = GetPlayerNamesByPosition(awayPlayers, "AT");

            // Get prompt
            var prompt = AiPrompts.GetSimulateMatchCommentaryPrompt(homeTeam, homeAttackers, awayTeam, awayAttackers);

            // Generate commentary
            var response = await _model.GenerateContentAsync(prompt);

            // Clean response
            var jsonText = CleanJsonResponse(response.Text);
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var result = JsonSerializer.Deserialize<MatchDataResponse>(jsonText, options);
                if (result == null)
                {
                    _logger.LogError("Failed to parse AI simulation response - deserialization returned null");
                    throw new Exception("Failed to parse AI simulation response - deserialization returned null");
                }
                
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON parsing error: {ex.Message}");
                throw new Exception($"Failed to parse AI simulation response: {ex.Message}", ex);
            }
        }

        // Compile match result data into commentary models and return it for easy display and Firestore updates 
        private MatchCommentaryResult CompileMatchResult(MatchDataResponse matchData, string homeCountryId, string awayCountryId, Dictionary<string, string> homePlayerDict, Dictionary<string, string> awayPlayerDict)
        {
            var result = new MatchCommentaryResult
            {
                HomeScore = matchData.HomeScore,
                AwayScore = matchData.AwayScore,
                WinnerId = matchData.HomeScore > matchData.AwayScore ? homeCountryId : awayCountryId
            };

            // Add each event to commentary
            foreach (var evt in matchData.Events.OrderBy(e => e.Minute))
            {
                var moment = new CommentaryMoment
                {
                    Minute = evt.Minute,
                    Type = ParseEventType(evt.EventType),
                    Description = evt.Description,
                    HomeScore = evt.HomeScore,
                    AwayScore = evt.AwayScore,
                    PlayerName = evt.PlayerName
                };

                if (!string.IsNullOrEmpty(evt.PlayerName))
                {
                    if (homePlayerDict.TryGetValue(evt.PlayerName, out var playerId))
                    {
                        moment.PlayerId = playerId;
                    }
                    else if (awayPlayerDict.TryGetValue(evt.PlayerName, out var playerId2))
                    {
                        moment.PlayerId = playerId2;
                    }
                }

                result.Commentary.Add(moment);

                // Find goals
                if (evt.EventType == "Goal" && !string.IsNullOrEmpty(evt.PlayerName))
                {
                    string? playerId = null;
                    string? countryId = null;

                    if (homePlayerDict.TryGetValue(evt.PlayerName, out var homePlayerId))
                    {
                        playerId = homePlayerId;
                        countryId = homeCountryId;
                    }

                    else if (awayPlayerDict.TryGetValue(evt.PlayerName, out var awayPlayerId))
                    {
                        playerId = awayPlayerId;
                        countryId = awayCountryId;
                    }
                    else
                    {
                        _logger.LogWarning($"Could not find player ID for goal scorer: {evt.PlayerName}");
                        continue;
                    }

                    // Add to goal scorers list
                    result.GoalScorers.Add(new Goal
                    {
                        PlayerId = playerId,
                        PlayerName = evt.PlayerName,
                        CountryId = countryId,
                        Minute = evt.Minute
                    });

                    // Track player goal counts for goal updates
                    if (!result.PlayerGoalCounts.ContainsKey(playerId))
                    {
                        result.PlayerGoalCounts[playerId] = 0;
                    }

                    result.PlayerGoalCounts[playerId]++;
                }
            }

            return result;
        }

        private string GetPlayerNamesByPosition(List<Player> players, string position)
        {
            var names = players
                .Where(p => p.NaturalPosition == position)
                .Select(p => p.Name)
                .ToList();

            return names.Any() ? string.Join(", ", names) : "None";
        }

        // Remove markdown and clean AI response
        private string CleanJsonResponse(string responseText)
        {
            responseText = responseText.Trim();

            if (responseText.StartsWith("```json"))
                responseText = responseText.Substring(7);
            if (responseText.StartsWith("```"))
                responseText = responseText.Substring(3);
            if (responseText.EndsWith("```"))
                responseText = responseText.Substring(0, responseText.Length - 3);

            return responseText.Trim();
        }

        private CommentaryType ParseEventType(string eventType)
        {
            return eventType switch
            {
                "KickOff" => CommentaryType.KickOff,
                "Goal" => CommentaryType.Goal,
                "Save" => CommentaryType.Save,
                "NearMiss" => CommentaryType.NearMiss,
                "Foul" => CommentaryType.Foul,
                "YellowCard" => CommentaryType.YellowCard,
                "RedCard" => CommentaryType.RedCard,
                "HalfTime" => CommentaryType.HalfTime,
                "FullTime" => CommentaryType.FullTime,
                "ExtraTimeStart" => CommentaryType.ExtraTimeStart,
                "Penalties" => CommentaryType.Penalties,
                _ => CommentaryType.KickOff
            };
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------//
    // Service response models
    public class MatchDataResponse
    {
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public List<MatchEvent> Events { get; set; } = new List<MatchEvent>();
    }

    public class MatchEvent
    {
        public int Minute { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string? PlayerName { get; set; }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------//
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//