namespace INF4001_WDXJOS004_ANLeague_2026.Data
{
    public static class AiPrompts
    {
        // Prompt for detailed AI play-by-play match commentary
        public static string GetPlayMatchCommentaryPrompt(string homeTeam, string homeAttackers, string homeDefenders, string homeGoalkeepers, string awayTeam, string awayAttackers, string awayDefenders, string awayGoalkeepers)
        {
            return $@"Generate a knockout stage African Nations League match between {homeTeam} and {awayTeam}.

CRITICAL RULES:
- Match CANNOT end in a draw (knockout tournament)
- FIRST: Randomly decide final scores (0-5 goals for each team, but scores MUST be different)
- THEN: Create goal events and other match moments that lead to those final scores
- MOST matches should have a winner after 90 minutes
- ONLY if scores are level at 90 minutes: extra time (91-120)
- ONLY if still level after extra time: penalties
- Generate 15-20 key moments
- Use ONLY player names from the lists below
- Times format: ""15"" not ""15:00""
- Either team can win - no home advantage
- Ensure variety: don't favor any particular scoreline

{homeTeam} Players:
Attackers: {homeAttackers}
Defenders: {homeDefenders}
Goalkeeper: {homeGoalkeepers}

{awayTeam} Players:
Attackers: {awayAttackers}
Defenders: {awayDefenders}
Goalkeeper: {awayGoalkeepers}

DESCRIPTION STYLE (keep concise, 1-2 sentences, be creative and varied):
- Goal: Exciting commentary with player name and scoring action
- Save: Highlight goalkeeper's reflexes and skill
- NearMiss: Build tension, describe how close the chance was
- Foul: Describe the challenge and impact
- YellowCard/RedCard: State the card and reason
- KickOff/HalfTime/FullTime: Brief match status update

JSON FORMAT (respond with ONLY this, no markdown):
{{
  ""homeScore"": X,
  ""awayScore"": Y,
  ""events"": [
    {{
      ""minute"": 1,
      ""eventType"": ""KickOff"",
      ""description"": ""We're underway in this thrilling encounter!"",
      ""homeScore"": [cumulative],
      ""awayScore"": [cumulative],
      ""playerName"": null
    }},
    {{
      ""minute"": [varies],
      ""eventType"": ""Goal"",
      ""description"": ""GOAL! A stunning strike finds the net!"",
      ""homeScore"": [cumulative],
      ""awayScore"": [cumulative],
      ""playerName"": ""[exact name from attacker list]""
    }},
    {{
      ""minute"": 45,
      ""eventType"": ""HalfTime"",
      ""description"": ""Half-time whistle blows."",
      ""homeScore"": [cumulative],
      ""awayScore"": [cumulative],
      ""playerName"": null
    }},
    {{
      ""minute"": 90,
      ""eventType"": ""FullTime"",
      ""description"": ""Full-time! What a match!"",
      ""homeScore"": X,
      ""awayScore"": Y,
      ""playerName"": null
    }}
  ]
}}

EVENT TYPES: KickOff, Goal, Save, NearMiss, Foul, YellowCard, RedCard, HalfTime, FullTime, ExtraTime Start, Penalties

Generate exciting, realistic match!";
        }

        // Ai prompt for simple goal-only match commentary
        public static string GetSimulateMatchCommentaryPrompt(string homeTeam, string homeAttackers, string awayTeam, string awayAttackers)
        {
            return $@"Simulate a knockout stage African Nations League match between {homeTeam} and {awayTeam}.

CRITICAL RULES:
- Match CANNOT end in a draw (knockout tournament)
- FIRST: Randomly decide final scores (0-5 goals for each team, but scores MUST be different)
- THEN: Create goal events chronologically that lead to those final scores
- MOST matches should have a winner after 90 minutes
- ONLY if scores are level at 90 minutes: extra time (91-120 minutes)
- ONLY if still level after extra time: penalties (use minute 121+)
- Generate ONLY goal events (no other event types)
- Use ONLY exact attacker names from the squad lists below
- Time format: INTEGER only (15, not ""15"")
- Either team can win - no home advantage
- Ensure variety: don't favor any particular scoreline

{homeTeam} SQUAD:
Attackers: {homeAttackers}

{awayTeam} SQUAD:
Attackers: {awayAttackers}

PLAYER USAGE:
- Goal: Use attacker names in description (ALWAYS include the scorer's name)

DESCRIPTION STYLE (be brief but ALWAYS include player name):
- Goal: Simple goal announcement with player name (e.g., ""[Player Name] scores!"")

JSON FORMAT REQUIREMENTS:
- Respond with ONLY valid JSON (no markdown, no code blocks, no explanations)
- ""minute"" field: INTEGER only (e.g., 23)
- ""eventType"" field: Must be ""Goal""
- ""description"" field: STRING with brief commentary including player name
- ""homeScore"" and ""awayScore"" fields: INTEGERS showing cumulative score at that moment
- ""playerName"" field: Exact attacker name from squad lists as STRING
- Events must be in chronological order by minute

RESPONSE FORMAT EXAMPLE (DO NOT copy these exact scores - vary them!):
{{
  ""homeScore"": X,
  ""awayScore"": Y,
  ""events"": [
    {{
      ""minute"": [varies],
      ""eventType"": ""Goal"",
      ""description"": ""[Player Name] scores!"",
      ""homeScore"": [cumulative],
      ""awayScore"": [cumulative],
      ""playerName"": ""[exact attacker name]""
    }},
    {{
      ""minute"": [varies],
      ""eventType"": ""Goal"",
      ""description"": ""[Player Name] finds the net!"",
      ""homeScore"": [cumulative],
      ""awayScore"": [cumulative],
      ""playerName"": ""[exact attacker name]""
    }}
  ]
}}

Generate realistic match with varied outcomes!";
        }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//