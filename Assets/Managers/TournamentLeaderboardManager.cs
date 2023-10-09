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

		bool sendAllStats = GameModel.currentTournamentID == activeTournament || activeTournament == 0;

		var data = new TournamentLeaderboardSubmission();
		data.tournamentName = "campkq";
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
			
	}
}

