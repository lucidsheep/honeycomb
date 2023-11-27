using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/*
 *                 "type": "match",
                "match_type": "tournament",
                "scene_name": self.scene.name,
                "cabinet_name": cabinet.name,
                "cabinet_id": cabinet.id,
                "current_match": current_match,

                current_match = {
                    "id": match.id,
                    "blue_team": match.blue_team.name,
                    "blue_score": match.blue_score,
                    "gold_team": match.gold_team.name,
                    "gold_score": match.gold_score,
                    "is_warmup": match.is_warmup,
                    "rounds_per_match": match.bracket.rounds_per_match,
                    "wins_per_match": match.bracket.wins_per_match,
                }

                if match.round_name:
                    current_match["round_name"] = "{} - {}".format(match.bracket.name, match.round_name)
                else:
                    current_match["round_name"] = match.bracket.name
*/
[System.Serializable]
public class HMMatchState
{
    public string type;
    public string cabinet_id;
    //type = match
    public string scene_name;
    public string cabinet_name;
    public HMCurrentMatch current_match;
    public HMCurrentMatch on_deck;
    //type = matchend
    public int match_id;
    public string blue_team;
    public int blue_score;
    public string gold_team;
    public int gold_score;
}

/*
 *             "type": "matchend",
            "match_id": self.id,
            "cabinet_id": self.active_cabinet_id,
            "blue_team": self.blue_team.name,
            "blue_score": self.blue_score,
            "gold_team": self.gold_team.name,
            "gold_score": self.gold_score,
*/
[System.Serializable]
public class HMCurrentMatch
{
    public int id;
    public string blue_team;
    public int blue_score;
    public string gold_team;
    public int gold_score;
    public bool is_warmup;
    public int rounds_per_match;
    public int wins_per_match;
    public string round_name;
}
[System.Serializable]
public class HMTypeCheck
{
    public string type;
    public int cabinet_id;
    public string cabinet_name;
    public string scene_name;
}

[System.Serializable]
public class HMPlayerStat
{
    private Dictionary<string, int> Something = new Dictionary<string, int>();
    public int this[string i]
    {
        get { if (Something.ContainsKey(i)) return Something[i]; else return 0; }
        set { Something[i] = value; }
    }
    public void ConvertJson(string json)
    {
        //takes dumb json array and adds attributes
        string[] vals = json.Split(',');
        foreach(var parts in vals)
        {
            int key = 0;
            int value = 0;
            string[] colonParts = parts.Split(':');

            //left side of colon = key, might have some brackets or quotes in the way
            string[] part = colonParts[0].Split('"');
            foreach(var maybeNumber in part)
            {
                int tv = 0;
                int.TryParse(maybeNumber, out tv);
                if(tv != 0)
                {
                    //found something! set key
                    key = tv;
                }
            }
            //right side of colon = value
            int.TryParse(colonParts[1], out value);
            if(key != 0)
            {
                //set value
                Something.Add(key.ToString(), value);
            }
        }
    }

    public static HMPlayerStat CreateHMStatArray(string message, string arrayName)
    {
        var ret = new HMPlayerStat();
        int startIndex = message.IndexOf(arrayName) + arrayName.Length + 2;
        int endIndex = message.IndexOf('}', startIndex + 1);
        if (startIndex > -1 && endIndex - startIndex > 5) //watch for empty array case
        {
            var substring = message.Substring(startIndex, endIndex - startIndex);
            //do conversion thingy and pray
            ret.ConvertJson(substring);
        }
        return ret;
    }
}
[System.Serializable]
public class HMInGameStats
{
    public HMPlayerStat berries_deposited;
    public HMPlayerStat berries_kicked_in;
    public int berries_remaining;
    public HMPlayerStat deaths;
    public float game_time;
    public string[] have_berries;
    public int[] kills;
    public string map_name;
    public HMPlayerStat military_deaths;
    public HMPlayerStat military_kills;
    public HMPlayerStat queen_kills;
    public HMPlayerStat snail_distance;
    public HMPlayerStat total_berries;

}

[System.Serializable]
public class HMOverlaySettings
{
    public string type;
    public string overlay_id;
    public string ingame_theme;
    public string postgame_theme;
    public string match_theme;
    public string match_preview_theme;
    public string player_cams_theme;
    public string cabinet_name;
    public bool show_players;
    public string blue_team;
    public int blue_score;
    public string gold_team;
    public int gold_score;
    public bool show_score;
    public int match_win_max;
}

//{"type":"gameend","cabinet_id":"26","cabinet_name":"groundkontrol","scene_name":"kqpdx","game_id":"372764","winning_team":"gold","win_condition":"military"}

[System.Serializable]
public class HMGameEnd
{
    public string type;
    public string cabinet_id;
    public string cabinet_name;
    public string scene_name;
    public string game_id;
    public string winning_team;
    public string win_condition;
}

//{"type":"gamestart","cabinet_id":"26","cabinet_name":"groundkontrol","scene_name":"kqpdx","game_id":"627809"}
public class HMGameStart
{
    public string type;
    public string cabinet_id;
    public string cabinet_name;
    public string scene_name;
    public string game_id;
}
/*
 *             "id": 26,
            "name": "groundkontrol",
            "display_name": "Ground Kontrol",
            "scene": 13,
            "address": "",
            "description": "",
            "time_zone": "America/Los_Angeles",
            "allow_qr_signin": true,
            "allow_nfc_signin": false,
            "allow_tournament_player": false,
            "total_games": 1304,
            "last_game_time": "2022-05-23T05:43:32.785000Z",
            "client_status": "online"
*/
[System.Serializable]
public class HMCabinet
{
    public int id, scene, total_games;
    public bool allow_qr_signin, allow_nfc_signin, allow_tournament_player;
    public string name, display_name, address, description, time_zone, last_game_time, client_status;
}
[System.Serializable]
public class HMCabinetResponse
{
    public int count;
    public string next;
    public string previous;
    public HMCabinet[] results;
}

[System.Serializable]
public class HMGameResponse
{
    public int count;
    public string next;
    public string previous;
    public HMGame[] results;
}

[System.Serializable]
public class HMGame
{
    public int id, match, qp_match, tournament_match;
    public string map_name, start_time, end_time, status, win_condition, winning_team, player_count, cabinet_version;
    //cabinet{}
    //scene{}
    //users[{}]
}

//most stats need to be converted to HMPlayerStats
//https://kqhivemind.com/api/game/game/911259/stats/

[System.Serializable]
public class HMGameStats
{
    public HMTeamIntData berries;
    public string map, win_condition, winning_team;
    public float length_sec;
}

//Honeycomb converted aggregation of team stats for a game
public class TeamGameStats
{
    public int teamID, gameID, militaryKills, militaryDeaths, snailLengths, berries, totalSeconds;
    public bool didWin;
    public string mapName, winCondition;

}

public class TeamTournamentStats : TeamGameStats
{
    public int[] mapWins, mapLosses;
    public int totalGames;

    public TeamTournamentStats(int id)
    {
        teamID = id;
        mapWins = new int[] { 0, 0, 0, 0 };
        mapLosses = new int[] { 0, 0, 0, 0 };

        militaryKills = militaryDeaths = snailLengths = berries = totalGames = totalSeconds = 0;
    }

    public void AddGame(TeamGameStats game)
    {
        militaryKills += game.militaryKills;
        militaryDeaths += game.militaryDeaths;
        berries += game.berries;
        snailLengths += game.snailLengths;
        totalGames++;
        totalSeconds += game.totalSeconds;

        var ind = -1;
        if (game.mapName == "Day") ind = 0;
        else if (game.mapName == "Night") ind = 1;
        else if (game.mapName == "Dusk") ind = 2;
        else if (game.mapName == "Twilight") ind = 3;

        if(ind > -1)
        {
            if (game.didWin)
                mapWins[ind]++;
            else
                mapLosses[ind]++;
        }
    }
    public string GetMap(bool best)
    {
        if (totalGames == 0)
            return "???";
        int bestScore = 99999 * (best ? -1 : 1);
        int bestInd = -1;
        int numTies = 0;
        int tiedInd = -1;
        for(int i = 0; i < 4; i++)
        {
            int thisScore = mapWins[i] - mapLosses[i];
            if((best && thisScore > bestScore)
            || (!best && thisScore < bestScore))
            {
                bestScore = thisScore;
                bestInd = i;
                numTies = 0;
            } else if(thisScore == bestScore)
            {
                numTies++;
                tiedInd = i;
            }
        }
        if(numTies == 1) //pick randomly between two candidates
        {
            bestInd = Random.Range(0, 2) == 1 ? tiedInd : bestInd;
        } else if(numTies > 1) //too many candidates, answer is unknown
        {
            return "???";
        }
        switch(bestInd)
        {
            case 0: return "Day";
            case 1: return "Night";
            case 2: return "Dusk";
            case 3: return "Twilight";
            default: return "???";
        }
    }
}

[System.Serializable]
public class HMTeamIntData
{
    public int blue;
    public int gold;
}

[System.Serializable]
public class HMTeamStringData
{
    public string blue;
    public string gold;
}
/*
 *             "scene_name": "kqpdx",
            "cabinet_id": 26,
            "cabinet_name": "groundkontrol",
            "player_id": 1,
            "position_name": "Gold Queen",
            "action": "sign_in",
            "user_id": 237,
            "user_name": "Kevin J"
*/
[System.Serializable]
public class HMSignedInUser
{
    public string scene_name, cabinet_name, position_name, action, user_name;
    public int cabinet_id, player_id, user_id;
}
[System.Serializable]
public class HMSignedInResponse
{
    public int id;
    public HMSignedInUser[] signed_in;
}

[System.Serializable]
public class HMTournamentResponse
{
    public int count;
    public string next;
    public string previous;
    public HMTournamentMatch[] results;
}

/*{"id":1,"tournament":
 * {
 *  "id":1,"scene":1,"description":"","deleted":false,"deleted_at":null,
 *  "name":"Baltimore Brawl 4","date":"2019-09-07",
 *  "is_active":false,"location":"","link_type":"challonge","deleted_by":null
 * },
 * "linked_match_id":"173316050","blue_score":3,"gold_score":0,"stage_name":null,"round_name":"",
 * "is_flipped":false,"is_complete":true,"is_warmup":false,"video_link":null,
 * "bracket":1,"blue_team":15,"gold_team":1,"active_cabinet":null


*/
[System.Serializable]
public class HMTournamentMatch
{
    public HMTournament tournament;
    public string linked_match_id, stage_name, round_name, video_link;
    public int blue_score, gold_score, bracket, blue_team, gold_team, active_cabinet, id;
    public bool is_flipped, is_complete, is_warmup;
}

[System.Serializable]
public class HMTournament
{
    public int id, scene, deleted_by;
    public string description, deleted_at, name, date, location, link_type;
    public bool deleted, is_active;
}

//{ "id":171,"name":"Let's Talk Comp","linked_team_ids":["172021224"],"tournament":28}

[System.Serializable]
public class HMTournamentTeam
{
    public int id, tournament;
    public string name;
}

/*
{
    "id": 1,
    "name": "Pool A",
    "linked_bracket_id": "baltimorebrawlpoola",
    "link_token": "",
    "linked_org": null,
    "is_valid": true,
    "rounds_per_match": null,
    "wins_per_match": null,
    "report_as_sets": false,
    "tournament": 1
}
*/

[System.Serializable]
public class HMTournamentBracket
{
    public int id, rounds_per_match, wins_per_match, tournament;
    public string name, linked_bracket_id, link_token, linked_org;
    public bool is_valid, report_as_sets;
}

[System.Serializable]
public class HMTournamentPlayer
{
    public string name, scene, pronouns, tidbit, image;
    public int team, user, tournament;
    public bool do_not_display;
    public HMUserStat[] stats;
}

[System.Serializable]
public class HMTournamentPlayerList
{
    public int count;
    public string next;
    public string previous;
    public HMTournamentPlayer[] results;
}
[System.Serializable]
public class HMUserList
{
    public int count;
    public string next;
    public string previous;
    public HMUserData[] results;
}
[System.Serializable]
public class HMUserStat
{
    public string name;
    public string label;
    public int value;
}
[System.Serializable]
public class HMUserData
{
    public int id;
    public string name;
    public string detail; //if not null, profile is private
    //stats [{ name / label / value }]
    public bool is_profile_public;
    public string scene;
    public string image;
    public string pronouns;
    public HMUserStat[] stats;
}