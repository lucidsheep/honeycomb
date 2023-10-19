using UnityEngine;
using System.Collections;
using TMPro;

public class PostgameStatTable : MonoBehaviour
{
	public TextMeshPro[] cols;
	public PlayerModel.StatValueType[] stats = new PlayerModel.StatValueType[] { PlayerModel.StatValueType.KD, PlayerModel.StatValueType.Pinces, PlayerModel.StatValueType.ObjGuards, PlayerModel.StatValueType.FormGuards, PlayerModel.StatValueType.UpTime, PlayerModel.StatValueType.Berries, PlayerModel.StatValueType.BerryKicks, PlayerModel.StatValueType.Snail, PlayerModel.StatValueType.BumpAssists, PlayerModel.StatValueType.SnailKills };
	public PostgamePlayerLine[] playerLines;
	public bool miniTable = false;
	public SpriteRenderer themeBG;

    private void Awake()
    {
		GameModel.onGameModelComplete.AddListener((_, __) => OnPostgame());
    }

    public void OnPostgame()
	{
		if(themeBG != null)
			themeBG.sprite = AppLoader.GetStreamingSprite("postgameStatTable");
		if (miniTable)
		{
			for (int i = 0; i < cols.Length; i++)
			{
				string txt = "";
				for (int j = 0; j < stats.Length; j++)
				{
					//force queen uptime to be gate control
					if (i % 5 == 2 && stats[j] == PlayerModel.StatValueType.UpTime)
						txt += GameModel.GetPlayer(i / 5, i % 5).curGameDerivedStats[PlayerModel.StatValueType.Gates].fullNumber + "\n";
					else if (stats[j] == PlayerModel.StatValueType.KDA)
						txt += GameModel.GetPlayer(i / 5, i % 5).GetKDA() + "\n";
					else
						txt += GameModel.GetPlayer(i / 5, i % 5).curGameDerivedStats[stats[j]].fullNumber + "\n";
				}
				cols[i].text = txt;
			}
		}
		else
		{
			for (int i = 0; i < stats.Length; i++)
			{
				string txt = "";
				for (int j = 0; j < 10; j++)
					txt += GameModel.GetPlayer(j / 5, j % 5).curGameDerivedStats[stats[i]].fullNumber + "\n";
				cols[i].text = txt;
			}
		}
		for(int i =0; i < 10; i++)
        {
			playerLines[i].OnPostgame(GameModel.GetPlayer(i / 5, i % 5));

		}
	}
}

