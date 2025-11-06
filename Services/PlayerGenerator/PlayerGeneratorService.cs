using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using INF4001_WDXJOS004_ANLeague_2026.Data;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.PlayerGenerator
{
    public class PlayerGeneratorService : IPlayerGeneratorService
    {
        private readonly ILogger<PlayerGeneratorService> _logger;
        private readonly Random _random = new Random();

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public PlayerGeneratorService(ILogger<PlayerGeneratorService> logger)
        {
            _logger = logger;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Generate players with names, positions, and ratings
        public async Task<List<Player>> GeneratePlayersAsync(int count = 23)
        {
            try
            {
                var players = new List<Player>();

                // Define position distribution
                var positions = new List<Position>();
                positions.AddRange(Enumerable.Repeat(Position.GK, 2));
                positions.AddRange(Enumerable.Repeat(Position.DF, 8));
                positions.AddRange(Enumerable.Repeat(Position.MD, 8));
                positions.AddRange(Enumerable.Repeat(Position.AT, 5));

                // Shuffle positions
                var shuffledPositions = positions.OrderBy(x => _random.Next()).ToList();

                for (int i = 0; i < count; i++)
                {
                    var naturalPosEnum = shuffledPositions[i];
                    var player = new Player
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = GenerateRandomName(),
                        NaturalPosition = naturalPosEnum.ToString(),
                        IsCaptain = false,
                        Ratings = GenerateRatings(naturalPosEnum)
                    };

                    players.Add(player);
                }

                _logger.LogInformation($"Generated {players.Count} players");

                return await Task.FromResult(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating players");
                throw;
            }
        }

        // Generate a random player for a position
        private Player CreateRandomPlayer(Position naturalPos)
        {
            return new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = GenerateRandomName(),
                NaturalPosition = naturalPos.ToString(),
                IsCaptain = false,
                Ratings = GenerateRatings(naturalPos)
            };
        }

        // Generate players for specific positions
        public async Task<List<Player>> GeneratePlayersForPositionsAsync(IEnumerable<string> naturalPositions)
        {
            try
            {
                var list = new List<Player>();

                foreach (var posStr in naturalPositions)
                {
                    if (!Enum.TryParse<Position>(posStr, out var posEnum))
                    {
                        // default
                        posEnum = Position.DF;
                    }

                    list.Add(CreateRandomPlayer(posEnum));
                }

                _logger.LogInformation($"Generated {list.Count} players for specific positions");

                return await Task.FromResult(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating players for positions");
                throw;
            }
        }

        // Generate a random player name
        private string GenerateRandomName()
        {
            var firstName = PlayerNames.FirstNames[_random.Next(PlayerNames.FirstNames.Length)];
            var lastName = PlayerNames.LastNames[_random.Next(PlayerNames.LastNames.Length)];
            return $"{firstName} {lastName}";
        }

        // Generate ratings for a player
        private PlayerRatings GenerateRatings(Position naturalPosition)
        {
            var ratings = new PlayerRatings();

            ratings.GK = _random.Next(0, 51);
            ratings.DF = _random.Next(0, 51);
            ratings.MD = _random.Next(0, 51);
            ratings.AT = _random.Next(0, 51);

            switch (naturalPosition)
            {
                case Position.GK:
                    ratings.GK = _random.Next(50, 101);
                    break;

                case Position.DF:
                    ratings.DF = _random.Next(50, 101);
                    break;

                case Position.MD:
                    ratings.MD = _random.Next(50, 101);
                    break;

                case Position.AT:
                    ratings.AT = _random.Next(50, 101);
                    break;
            }

            return ratings;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//