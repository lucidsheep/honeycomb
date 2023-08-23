using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LSWeightedItem<A>
{
	public A item;
	public int weight;
	public LSWeightedItem() { }
	public LSWeightedItem(A itm) { item = itm; }
	public LSWeightedItem(A itm, int w) { item = itm; weight = w; }
}

[System.Serializable]
public class LSWeightedList<A> : List<LSWeightedItem<A>>
{

	public int totalWeight { get { return GetWeight(); }}

	public LSWeightedList() : base() { }

	public void Add(A item)
	{
		base.Add(new LSWeightedItem<A>(item));
	}

	public void Add(A item, int weight)
	{
		base.Add(new LSWeightedItem<A>(item, weight));
	}

	public A GetRandomItem()
	{
		if(Count == 0) return new LSWeightedItem<A>().item;
		if(Count == 1) return this[0].item;

		int totalWeight = GetWeight();
		if(totalWeight == 0) return new LSWeightedItem<A>().item;

		int chosenIndex = Random.Range(0, totalWeight) + 1;
		foreach(var item in this)
		{
			chosenIndex -= item.weight;
			if(chosenIndex <= 0)
				return item.item;
		}
		return new LSWeightedItem<A>().item;
	}


	int GetWeight()
	{
		int totalWeight = 0;
		foreach(var item in this)
		{
			totalWeight += item.weight;
		}

		return totalWeight;
	}
}

