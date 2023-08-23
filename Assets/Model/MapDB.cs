using UnityEngine;
using System.Collections.Generic;

public class MapDB : MonoBehaviour
{
	public static MapDB instance;

	public MapData[] allMaps;

	public static Dictionary<string, MapData> maps;
	public static LSProperty<MapData> currentMap = new LSProperty<MapData>();

	void Awake()
    {
		instance = this;
		maps = new Dictionary<string, MapData>();
		foreach (var m in allMaps) maps.Add(m.name, m);
		currentMap.property = allMaps[0];
	}

	public static void SetMap(string mapName)
    {
		if (MapDB.maps.ContainsKey(mapName))
		{
			Debug.Log("setting map to " + mapName);
			currentMap.property = maps[mapName];
		}
	}
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
			
	}
}

