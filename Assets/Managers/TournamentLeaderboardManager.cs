using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TournamentLeaderboardManager : MonoBehaviour
{
	NetworkConnection connection;

	static TournamentLeaderboardManager instance;

	public bool managerEnabled = true;
	public string serverAddress;
	public int serverPort;
	public bool isSecureConnection;
	public int activeTournament = -1;
	public string leaderboardScene = "";
	public float leaderboardTimer = 10f;

	float nextRefresh = 10f;
	string[] leaderboardList = new string[] { "deaths", "kills_queen_aswarrior", "berries_kicked", "warrior_ratio", "berries", "warrior_deaths", "snail", "snail_deaths", "jason_points" };
	int curLeaderboard = 0;

	public static UnityEvent<TournamentLeaderboard> OnLeaderboardReceived = new UnityEvent<TournamentLeaderboard>();
    private void Awake()
    {
		instance = this;
    }
    // Use this for initialization
    void Start()
	{
		if(managerEnabled)
        {
			connection = new NetworkConnection(serverAddress, serverPort, isSecureConnection);
			connection.OnNetworkEvent.AddListener(OnLeaderboardData);
			connection.OnConnectionEvent.AddListener(OnLeaderboardConnectionState);
			connection.StartConnection();

			GameModel.onGameModelComplete.AddListener(OnGameComplete);
        }
	}

	void OnLeaderboardConnectionState(bool state)
    {
		Debug.Log("leaderboard DB " + (!state ? "dis" : "") + "connected");
		if (connection.isConnected)
			nextRefresh = 0.01f;
    }
	void OnLeaderboardData(string jsonData)
    {
		var data = JsonUtility.FromJson<TournamentLeaderboard>(jsonData);
		OnLeaderboardReceived.Invoke(data);
    }

	//leaderboardID: -1 = don't send anything except jason points, 0 = send everything always, >0 = send if tournament ID matches
	void OnGameComplete(int winningTeam, string winType)
    {

		if (!connection.isConnected) return;
		if (GameModel.instance.isWarmup) return; //warmups don't count
		if (NetworkManager.instance.sceneName == "") return;

		if (leaderboardScene != "" && leaderboardScene != NetworkManager.instance.sceneName) return;

		bool sendAllStats = GameModel.currentTournamentID == activeTournament || activeTournament == 0;

		var data = new TournamentLeaderboardSubmission();
		data.scene = NetworkManager.instance.sceneName;
		data.type = "gameEnd";
		var loggedInPlayers = new List<TournamentLeaderboardPlayer>();
		foreach(var t in GameModel.instance.teams) { foreach(var p in t.players)
        {
			if(p.hivemindID > 0 && p.hivemindID < 1000000) //over 1000000 is a tournament-only id, not eligible for leaderboards
            {
				loggedInPlayers.Add(TournamentLeaderboardPlayer.PlayerModelToLeaderboardRow(p, null, sendAllStats));
            }
        }}
		if(loggedInPlayers.Count > 0)
        {
			data.players = loggedInPlayers.ToArray();
			connection.SendMessageToServer(JsonUtility.ToJson(data));
        }
    }
	// Update is called once per frame
	void Update()
	{
		if (!connection.isConnected) return;

		nextRefresh -= Time.deltaTime;
		if(nextRefresh <= 0f)
        {
			nextRefresh = leaderboardTimer;
			if (NetworkManager.instance.sceneName == "") return;
			var message = new TournamentLeaderboardSubmission();
			message.scene = NetworkManager.instance.sceneName;
			message.type = "getLeaderboard";
			message.leaderboard = leaderboardList[curLeaderboard];
			curLeaderboard++;
			if (curLeaderboard >= leaderboardList.Length)
				curLeaderboard = 0;
			//special case to exclude jason points from non-pdx scenes
			if (message.scene != "kqpdx" && curLeaderboard + 1 >= leaderboardList.Length)
				curLeaderboard = 0;
			connection.SendMessageToServer(JsonUtility.ToJson(message));
        }
	}
}

