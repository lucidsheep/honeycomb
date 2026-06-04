# Leaderboard Integration

Any overlay that can utilize Websockets can integrate with kq.style hosted leaderboards for collecting and showing off various stats in your community.

## <a name="connecting"></a>Connecting to the Leaderboard
Open a secure websocket and connect to `wss://kq.style/beehive` on port `8080`.

## <a name="sending"></a>Sending Stats to the Leaderboard
After a game ends, create a JSON string in the following format:
```

{"scene":"kqpdx","type":"gameEnd","leaderboard":"",
"players":
  [{"id":237,"name":"Kevin J","kills_military":0,"kills_queen":0,"kills_queen_aswarrior":0,"berries":0,"snail":0,"berries_kicked":0,"deaths":0,"warrior_uptime":0,"kills_all":0,"warrior_ratio":0.0,"warrior_deaths":0,"snail_deaths":0,"jason_points":0,"kills_queen_asqueen":0,"warrior_life":0,"bump_assists":0,"drone_kills_withberry":0},
  {"id":238,"name":"Jason G","kills_military":0,"kills_queen":0,"kills_queen_aswarrior":0,"berries":0,"snail":0,"berries_kicked":0,"deaths":0,"warrior_uptime":0,"kills_all":0,"warrior_ratio":0.0,"warrior_deaths":0,"snail_deaths":0,"jason_points":0,"kills_queen_asqueen":0,"warrior_life":0,"bump_assists":0,"drone_kills_withberry":0}]}

```
Explanations of each item
- `scene`: The name of the scene sending the data. The database filters by scene when generating leaderboards to display on your overlay.
- `type`: Message type, `gameEnd` tells the database that it is receiving new leaderboard data
- `leaderboard`: The type of leaderboard to update. Since there is currently only one leaderboard type we always send a blank string `""`
- `players`: An array of each logged in player and their associated stats for the game that ended. For each players, ALL of the following fields need to be sent:
  - `id`: The player's HiveMind ID, used as a unique identifier for them
  - `name`: The player's name to display on the overlay
  - `kills_military`: How many kills the player got on military units, excluding the queen
  - `kills_queen`: How many kills the player got on the enemy queen (as a warrior or a queen)
  - `kills_queen_aswarrior`: How many kills the player got on the enemy queen **as a warrior**
  - `berries`: How many berries the player deposited, **excluding berry kicks**
  - `snail`: How many pixels the snail was moved by the player
  - `berries_kicked`: How many berries were kicked into the hive by the player, **not including kicks to the enemy hive**
  - `deaths`: How many total deaths the player had
  - `warrior_uptime`: How many seconds of the game the player was a warrior for (send 0 for queens)
  - `kills_all`: How many total kills the player had
  - `warrior_ratio`: Legacy stat, always send 0.0
  - `warrior_deaths`: How many deaths the player had as a warrior
  - `snail_deaths`: How many times the player died by feeding the snail
  - `jason_points`: A meme leaderboard for kills on a specific player. You can implement it by having a specific HiveMind ID you're looking for and tracking kills on that player (their name doesn't actually have to be Jason). You can just send 0 if not using this feature
  - `kills_queen_asqueen`: How many kills the player got on the enemy queen **as a queen**
  - `warrior_life`: The longest length of time the player was up as a warrior consequtively, in seconds. Send 0 for queens
  - `bump_assists`: The number of times a player bumped an enemy warrior or queen **as a drone** and that player was killed within the next 2 seconds
  - `drone_kills_withberry`: How many kils the player got on drones that are carrying a berry

After building the JSON string, send it over the webSocket as a byte array and leaderboards will be updated automatically.

## <a name="receiving"></a>Requesting Leaderboard Data
You can request a leaderboard by sending the following JSON:
```

{"scene":"kqpdx","type":"getLeaderboard","leaderboard":"kills_queen_aswarrior"}

```
- `scene`: The scene you are requesting the leaderboard for
- `type`: Message type, `getLeaderboard` tells the database that a leaderboard generation is being requested
- `leaderboard`: The leaderboard that is being requested. This can be any of the data types listed in the "Sending Stats to the Leaderboard" section

After sending this message, the following data will be received:

```
{"leaderboardName":"kills_queen_asqueen","players":
[{"id":156,"scene":"kqpdx","name":"Dylan Lee ⭐️","kills_queen_asqueen":123},
{"id":552,"scene":"kqpdx","name":"Lex 🤘💀","kills_queen_asqueen":42},
{"id":265,"scene":"kqpdx","name":"Mel B","kills_queen_asqueen":40},
{"id":277,"scene":"kqpdx","name":"Bueno","kills_queen_asqueen":38},
{"id":390,"scene":"kqpdx","name":"Chacko","kills_queen_asqueen":38}]}
```

This data was truncated but there will be up to 15 players listed.

## <a name="delete"></a>Resetting Leaderboards
Leaderboards should be reset periodically to prevent the same players from dominating them too long. There's currently no API for requesting a reset, so contact @lucidsheep to request one for your scene. If you have a specific interval in mind (say, the first of every month) I can also add a cron job for that reset to occur automatically.