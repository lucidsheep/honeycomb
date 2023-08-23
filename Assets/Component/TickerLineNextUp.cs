using UnityEngine;
using System.Collections;
using TMPro;

public class TickerLineNextUp : TickerLineItem
{
	public TextMeshPro text;
	public Color blueColor;
	public Color goldColor;

    public void Init(TickerNetworkManager.TickerCabinetState state)
    {
        text.text = "<color=#6295CA>" + state.nextUpBlueTeam + "</color> vs <color=#DBAD56>" + state.nextUpGoldTeam + "</color>"; 
    }
}

