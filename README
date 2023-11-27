# Honeycomb

Honeycomb is a free overlay system for Killer Queen. It can be used in combination with OBS to broadcast many different aspects of the game, including team names, instant replays, and post-game stats.

There are three different styles for Honeycomb. The most flexible and feature-rich is a an app-based overlay that can leverage your streaming PC's GPU and CPU to the fullest to do cool things™. Here's a full breakdown of each version of Honeycomb:

|  | Web Scorebar | Web Fullscreen  | Honeycomb App  |
|---|---|---|---|
|HiveMind Team Names and Scores|✅|✅|✅|
|Center Score Area|✅|✅|✅|
|Kill Feed|✅|❌|❌|
|Team Rosters|❌|✅|✅|
|Post-game Stats|❌|✅|✅|
|Custom Themes|❌|Limited|✅|
|Instant Replay|❌|❌|✅|
|Image Recognition (beta)|❌|❌|✅|
|Win Prediction|❌|❌|✅|
|Leaderboards|❌|❌|✅|
|Tournament Match Preview|❌|❌|✅|
|BuzzBar bottom ticker|❌|❌|✅|
|Automatic Webcam Management|❌|❌|✅|

## Setup
First, ensure your scene and cabinet are properly integrated into HiveMind. See  [this guide](https://kqhivemind.com/wiki/Basic_Client_Setup)  for getting things set up! Make a note of your HiveMind scene and cabinet name for setting up Honeycomb.

The rest will depend on which version of Honeycomb you'd like to use.
### Web Versions
1.  In your OBS scene, add a new Browser source. Enter the following settings:
-   URL: replace sceneName with your scene name, and cabinetName with your cabinet name.
    - Scorebar: `https://kq.style/overlay/sceneName/cabinetName/`  
    - Full screen: `https://kq.style/bigoverlay/sceneName/cabinetName/`
-   Width and Height: For Scorebar, the recommended resolution is 1200 x 140 or wider. For the fullscreen overlay, enter 1920 x 1080.
2.  That's it! The overlay should load automatically and begin receiving game events. You can move and scale the scoreboard window to suit your scene's overlay, and add webcam sources for gameplay and player cameras.

### Honeycomb App
1. Download the client:
   -   [Windows](https://kq.style/honeycomb/windows/honeycomb.zip)
   -  MacOS (coming soon)
   - Linux (coming soon)
 2. Run the downloaded client. It will auto-update itself to the newest version and restart.
 3. In OBS add a Game Window source, and target honeycomb.exe. Scale the source to fill the entire area.

See the Configuration section for entering settings in the app.

## Setting Team Names and Scores

Team names and scores are automatically pulled from HiveMind and should not need to be manually adjusted. If needed, you can override the current names and score by clicking near the bottom of the screen. In the web version, you will need to click "Interact" in OBS first and then click inside the window.

[screenshot]

1.  Set team names for each team.
2.  Set number of games won for each team. This is automatically tallied by the scoreboard, but you can use this for manual adjustment when needed.
3.  Set maximum number of wins to show on each side (So for a Best of Three, enter 2).
4.  Click "New Set" when a new set is about to begin. This resets both team scores to 0 and resets all tracked data from the last series of games.
5.  When you are done with adjustments, click the "Done" button to save and exit the edit interface.

## Dev Console
Access the console by pressing tilde `~` and then entering a command. Use the `help` command to get a list of available commands. Press tilde again to close the console.

If something goes wrong in the overlay, a good first step is to check the console. Click inside the text to select the entire log; from here you can copy it and send to a Honeycomb developer.

Dev console is not available in the Scorebar web version.

# Honeycomb App
The rest of this documentation pertains to settings and features only available in the app version.

### Configuration Window
[window]
1. Hivemind / Local mode toggle. See Local Mode.
2. Theme: choose between multiple layouts for the overlay:
   - One Column: A single wide column on the right side of gameplay.
   - Two Column: Two smaller columns on either side of gameplay.
   - Two Columns + Ticker: Two columns, with added ticker at the bottom.
   - Game Only: Only shows gameplay (no columns or ticker). Will still show elements that overlap gameplay, such as Instant Replay, Postgame, and Match Preview.
   - Custom: Enter a name for a custom theme.
  3. Ticker Cabs: Enter cabinet names to track on the bottom ticker (optional)
  4. Camera Config: For each of the four cameras, click the button below to cycle through available cameras. You can also choose "Frame Only" to show a blank space (useful if you want to manage cameras in OBS) or "Off" to disable a camera entirely. For Blue and Gold cameras, you can select between Wide and Ultrawide; Ultrawide will crop the cameras at a wider aspect ratio.
  5. Low Resolution Cameras: Enable this to render non-gameplay cameras at a lower resolution and framerate. This is useful to improve performance.
  6.  Instant Replay: Enable and the overlay will replay the last 6 seconds of gameplay after the game ends, along with a transition video. Use of this requires more memory and GPU usage, and may cause issues on slower machines. Disable to improve performance.
  7. Image Recognition (beta): Enable an advanced player position tracking system that does fun things, including putting hats on players and putting players "on fire" when performing well. This is an advanced feature that utilizes machine learning models, and requires a dedicated GPU (at least a GTX 1070) to run decently.
  8. Project as Webcam: Using a third party driver, the overlay acts like a webcam and shows up as one in OBS. This is broken as of OBS 29.1 and should not be checked unless Kevin set it up for you :)

### Local Mode
When recording or projecting gameplay in non-networked environments, you can use Local Mode to get stats directly from a cabinet, instead of from HiveMind. 
1. Create a local network that connects your streaming PC and KQ cabinet. This can be done with a router or by sharing the computer's internet connection over Ethernet. 
2. In the Configuration Window, click the "HiveMind" button in the top left to switch into Local Mode.
3. Optionally, enter the cabinet's IP address in the text field. By default a cabinet will emit the custom domain `kq.local` so you should not need to, but this may be necessary if the custom domain fails or if there are multiple cabinets on the same network.
4. Finish configuration and click Done. Run a game on the cabinet to confirm if stats are coming through correctly.

Note that HiveMind features like logging in and running tournaments will not work in Local Mode.

## FAQ / Troubleshooting
#### The app is slow / laggy!
- Disable Instant Replay and Image Recognition
- Check "Low Resolution Cameras", or disable player cameras (set to Frame Only or Off). You can use OBS for player cameras, it has a more optimized process for rendering them.
- In Windows Task Manager, find the honeycomb.exe process and set its priority to "high".
- If you're a C# developer, [contribute](https://github.com/lucidsheep/honeycomb) to the project, it needs a lot of optimization :) 
#### A game started and the post-game stats are still up
- Ensure stats are still coming through (is the rest of the overlay still getting score updates?). If they are not, the internet may have broken, make sure to reconnect the cabinet. You may need to restart the overlay app to fix theconnection.
- If other stats are coming through, the overlay most likely missed the game start signal. Open the console (with `~`), enter `startGame` and hit enter to force the postgame screen to close. Hit `~` again to close the console.
#### The snail percentage is off / snail keeps going up after the game ends
- Due to a limitation in how game events work, snail progress is estimated and assumes the player is always moving forward. This can't be fixed unless more precise stats are implemented on the cabinet.

#### Who are all these people on the leaderboard / What the heck are Jason Points?
- The leaderboard system is currently global, an overhaul is in the works to make them per-scene and allow customization of which leaderboards show up.
- Jason Points are very important and unfortunately are exclusive to PDX.
#### Why an app? Aren't most overlays web-based?
- The Honeycomb project was originally web-only! The app version came about when I realized there were things that could only be done with an app that could up the production values of a KQ stream significantly, like instant replay and image recognition.
- Web overlays that are hosted on HiveMind have also come a long way, and Honeycomb made less sense as "just another web overlay" since it requires additional additional setup on a separate website from HiveMind.
- The ultimate end-game for Honeycomb is an all-in-one solution for streaming KQ, cutting OBS out of the equation. That would be a significant undertaking, though.
#### How do you determine which players show up on the post-game player cards?
- I have a proprietary algorithm that evaluates player actions and awards or takes away points for game actions. The top four scoring players are shown in the post-game.
- On the app version, [WPA](https://en.wikipedia.org/wiki/Win_probability_added) (win probability added) is also factored in using the Win Prediction feature. So an action that swings the prediction strongly for (or against) your team will factor into top player analysis.

#### Speaking of which, how does win prediction work?
- Win prediction (aka KQuity) is a game prediction model [created by Rob Neuhaus](https://github.com/rrenaud/KQuity). It combines multiple vectors from game data (such as berries deposited, snail position, queen lives, and warriors up) across thousands of recorded HiveMind games to come up with an estimate of which team is currently winning.
- In other words, it takes the current game state and compares it to similar situations within its library of millions of game states, and takes the average of which teams ended up winning in those similar games to create a prediction percentage.
#### What is a snail meter? What is a snail length? What is a snail?
- A Snail meter is the distance a snail moves when a normal drone moves it for one second (approximately `20.896215463` pixels). A snail length is the same thing as a snail meter, we just have inconsistent naming.
- The snail holds great power. It may or may not be the reincarnation of a vengeful god. For more info, please watch: [Killer Queen Lore](https://www.youtube.com/watch?v=dQw4w9WgXcQ).

#### I want to contribute! I want to send you money!
- [Please do contribute!](https://github.com/lucidsheep/honeycomb) The project is written in C# and uses Unity 2021.2 LTS. The code is poorly documented, so please contact me if you need any guidance or would like to submit a pull request.
- This is a passion project and I'm not taking donations. Please direct any of your money to [keeping the HiveMind servers running](https://www.patreon.com/kqhivemind/posts), this project would not be possible without the incredible work that goes into it and its very generous APIs.
- I will, however, always accept a drink. I'd love to chat with you at a KQ event soon!

#### Who are you, anyway?
- I'm Kevin! Find me on Discord as `lucidsheep`
- When I'm not making overlays with Unity, I [make games](https://lucidsheepgames.com) with Unity.

#### Did you just ask yourself who you are in your own FAQ?
- Yes.
#### Are you just aimlessly adding questions to procrastinate doing real work?
- Possibly.