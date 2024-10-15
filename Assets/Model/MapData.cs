using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "MapData")]
public class MapData : ScriptableObject
{
	public string name;
	public string display_name;
	public bool is_bonus;
	public int total_berries;
	public int snail_track_width;
	public int beta_of;
	public int snail_Y;
	public int queen_lives;
	public Sprite thumbnail;
	public MapFeature[] features;

	public (MapFeature, float) GetNearestFeature(Vector2Int targetPos, MapFeature.Type filter = MapFeature.Type.Any)
	{
		if (features.Length == 0) return (default(MapFeature), 0f);
		MapFeature closest = features[0];
		float dist = 999f;
		for(int i = 0; i < features.Length; i++)
        {
			if (filter != MapFeature.Type.Any && filter != features[i].type)
				continue;
			var thisDist = Vector2Int.Distance(targetPos, features[i].coords);
			if(thisDist < dist)
            {
				dist = thisDist;
				closest = features[i];
            }
		}
		return (closest, dist);
	}
}
[System.Serializable]
public struct MapFeature
{
	public enum Type {SpeedGate, SwordGate, BlueHive, GoldHive, Any };
	public Type type;
	public Vector2Int coords;
}

