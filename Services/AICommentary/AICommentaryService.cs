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
        // Stream play-by-play commentary for a played match as events arrive from AI
        public async IAsyncEnumerable<CommentaryMoment> StreamPlayMatchCommentaryAsync(string homeCountryId, string homeCountryName, List<Player> homePlayers, string awayCountryId, string awayCountryName, List<Player> awayPlayers)
        {
            _logger.LogInformation($"Streaming play commentary for {homeCountryName} vs {awayCountryName}");

            // Create player dictionaries
            var homePlayerDict = homePlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);
            var awayPlayerDict = awayPlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);

            // Get all players by position for prompt
            var homeAttackers = GetPlayerNamesByPosition(homePlayers, "AT");
            var homeDefenders = GetPlayerNamesByPosition(homePlayers, "DF");
            var homeGoalkeepers = GetPlayerNamesByPosition(homePlayers, "GK");
            var awayAttackers = GetPlayerNamesByPosition(awayPlayers, "AT");
            var awayDefenders = GetPlayerNamesByPosition(awayPlayers, "DF");
            var awayGoalkeepers = GetPlayerNamesByPosition(awayPlayers, "GK");

            // Generate prompt and stream commentary from AI
            var prompt = AiPrompts.GetPlayMatchCommentaryPrompt(homeCountryName, homeAttackers, homeDefenders, homeGoalkeepers, awayCountryName, awayAttackers, awayDefenders, awayGoalkeepers);

            await foreach (var moment in StreamAICommentaryEvents(prompt, homeCountryId, awayCountryId, homePlayerDict, awayPlayerDict))
            {
                yield return moment;
            }

            _logger.LogInformation($"Completed streaming play commentary for {homeCountryName} vs {awayCountryName}");
        }

        // Stream goal commentary for a simulated match as events arrive from AI
        public async IAsyncEnumerable<CommentaryMoment> StreamSimulateMatchCommentaryAsync(string homeCountryId, string homeCountryName, List<Player> homePlayers, string awayCountryId, string awayCountryName, List<Player> awayPlayers)
        {
            _logger.LogInformation($"Streaming simulate commentary for {homeCountryName} vs {awayCountryName}");

            // Create player dictionaries
            var homePlayerDict = homePlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);
            var awayPlayerDict = awayPlayers.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);

            // Get attackers only for prompt
            var homeAttackers = GetPlayerNamesByPosition(homePlayers, "AT");
            var awayAttackers = GetPlayerNamesByPosition(awayPlayers, "AT");

            // Generate prompt and stream commentary from AI
            var prompt = AiPrompts.GetSimulateMatchCommentaryPrompt(homeCountryName, homeAttackers, awayCountryName, awayAttackers);

            await foreach (var moment in StreamAICommentaryEvents(prompt, homeCountryId, awayCountryId, homePlayerDict, awayPlayerDict))
            {
                yield return moment;
            }

            _logger.LogInformation($"Completed streaming simulate commentary for {homeCountryName} vs {awayCountryName}");
        }

        // Stream AI commentary events and parse NDJSON lines into CommentaryMoment objects
        private async IAsyncEnumerable<CommentaryMoment> StreamAICommentaryEvents(string prompt, string homeCountryId, string awayCountryId, Dictionary<string, string> homePlayerDict, Dictionary<string, string> awayPlayerDict)
        {
            var buffer = new System.Text.StringBuilder();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            await foreach (var chunk in _model.StreamContentAsync(prompt))
            {
                if (string.IsNullOrEmpty(chunk.Text))
                    continue;

                buffer.Append(chunk.Text);

                // Process complete lines
                var text = buffer.ToString();
                var lines = text.Split('\n');

                // Process all complete lines 
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    // Clean markdown if present
                    line = CleanJsonResponse(line);
                    
                    if (string.IsNullOrEmpty(line))
                        continue;

                    // Parse line
                    var moment = ParseNDJSONEventLine(line, homeCountryId, awayCountryId, homePlayerDict, awayPlayerDict, options);
                    
                    if (moment != null)
                    {
                        yield return moment;
                    }
                }

                // Keep the last incomplete line in buffer
                buffer.Clear();
                buffer.Append(lines[lines.Length - 1]);
            }

            // Process any remaining content in buffer
            var finalLine = buffer.ToString().Trim();
            if (!string.IsNullOrEmpty(finalLine))
            {
                finalLine = CleanJsonResponse(finalLine);
                
                if (!string.IsNullOrEmpty(finalLine))
                {
                    var moment = ParseNDJSONEventLine(finalLine, homeCountryId, awayCountryId, homePlayerDict, awayPlayerDict, options);
                    
                    if (moment != null)
                    {
                        yield return moment;
                    }
                }
            }
        }

        // Parse an NDJSON line into a commentary moment
        private CommentaryMoment? ParseNDJSONEventLine(string line, string homeCountryId, string awayCountryId, Dictionary<string, string> homePlayerDict, Dictionary<string, string> awayPlayerDict, JsonSerializerOptions options)
        {
            try
            {
                var eventData = JsonSerializer.Deserialize<JsonElement>(line, options);
                return ConvertJsonEventToCommentaryMoment(eventData, homeCountryId, awayCountryId, homePlayerDict, awayPlayerDict);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning($"Failed to parse NDJSON line: {line}. Error: {ex.Message}");
                return null;
            }
        }

        // Convert a JSON event element into a commentary moment object
        private CommentaryMoment? ConvertJsonEventToCommentaryMoment(JsonElement eventData, string homeCountryId, string awayCountryId, Dictionary<string, string> homePlayerDict, Dictionary<string, string> awayPlayerDict)
        {
            try
            {
                var moment = new CommentaryMoment
                {
                    Minute = eventData.GetProperty("minute").GetInt32(),
                    Type = ParseEventType(eventData.GetProperty("eventType").GetString() ?? "KickOff"),
                    Description = eventData.GetProperty("description").GetString() ?? "",
                    HomeScore = eventData.GetProperty("homeScore").GetInt32(),
                    AwayScore = eventData.GetProperty("awayScore").GetInt32()
                };

                // Get player name if present
                if (eventData.TryGetProperty("playerName", out var playerNameElement))
                {
                    var playerName = playerNameElement.GetString();
                    if (!string.IsNullOrEmpty(playerName))
                    {
                        moment.PlayerName = playerName;
                        
                        if (homePlayerDict.TryGetValue(playerName, out var homePlayerId))
                        {
                            moment.PlayerId = homePlayerId;
                        }
                        else if (awayPlayerDict.TryGetValue(playerName, out var awayPlayerId))
                        {
                            moment.PlayerId = awayPlayerId;
                        }
                        else
                        {
                            var cleanedName = playerName;
                            
                            // Remove text in parentheses
                            var parenIndex = cleanedName.IndexOf('(');
                            if (parenIndex > 0)
                            {
                                cleanedName = cleanedName.Substring(0, parenIndex).Trim();
                            }
                            
                            // Try matching with cleaned name
                            if (!string.IsNullOrEmpty(cleanedName) && cleanedName != playerName)
                            {
                                if (homePlayerDict.TryGetValue(cleanedName, out var homePlayerId2))
                                {
                                    moment.PlayerId = homePlayerId2;
                                }
                                else if (awayPlayerDict.TryGetValue(cleanedName, out var awayPlayerId2))
                                {
                                    moment.PlayerId = awayPlayerId2;
                                }
                            }
                            
                            // Try partial matching 
                            if (string.IsNullOrEmpty(moment.PlayerId))
                            {
                                foreach (var kvp in homePlayerDict)
                                {
                                    if (kvp.Key.Equals(cleanedName, StringComparison.OrdinalIgnoreCase) ||
                                        kvp.Key.Contains(cleanedName, StringComparison.OrdinalIgnoreCase) ||
                                        cleanedName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                                    {
                                        moment.PlayerId = kvp.Value;
                                        break;
                                    }
                                }
                                
                                if (string.IsNullOrEmpty(moment.PlayerId))
                                {
                                    foreach (var kvp in awayPlayerDict)
                                    {
                                        if (kvp.Key.Equals(cleanedName, StringComparison.OrdinalIgnoreCase) ||
                                            kvp.Key.Contains(cleanedName, StringComparison.OrdinalIgnoreCase) ||
                                            cleanedName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                                        {
                                            moment.PlayerId = kvp.Value;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return moment;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error parsing event to CommentaryMoment: {ex.Message}");
                return null;
            }
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
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//