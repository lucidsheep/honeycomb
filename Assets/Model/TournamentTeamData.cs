using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class TournamentTeamData : ScriptableObject
{
	public string teamName;
	public string hivemindID;
	public string discordID;
	public List<TournamentPlayer> players;
}

