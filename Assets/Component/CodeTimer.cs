using UnityEngine;
using System.Collections.Generic;
using System;

public class CodeTimer : MonoBehaviour
{
	static LSRollingAverageDouble avg;
	static List<LSRollingAverageDouble> milestones;
	static DateTime marker;
	static DateTime lastMilestone;

	CodeTimer instance;

	static int numSamples;
	static int samplesLeft;
	static bool init = false;
	static int curMilestone = 0;

	//overhead for using codeTimer, run start() and stop() in an update loop for your system to calibrate
	static double calibratedOffset = 0.003d;

	// Use this for initialization
	void Awake()
	{
		instance = this;
	}

	public static void SetSamples(int num)
    {
		numSamples = samplesLeft = num;
		avg = new LSRollingAverageDouble(num, 0d);
		init = true;
    }

	public static void SetMilestones(int numMilestones)
    {
		milestones = new List<LSRollingAverageDouble>();
		if (!init)
			SetSamples(100);
		while(numMilestones > 0)
        {
			numMilestones--;
			milestones.Add(new LSRollingAverageDouble(numSamples, 0d));
        }
    }
	public static void StartTimer()
    {
		if(!init)
        {
			SetSamples(100);
        }
		marker = lastMilestone = DateTime.Now;
		curMilestone = 0;
    }
	public static void Milestone()
    {
		var thisMSTime = (DateTime.Now - lastMilestone).TotalMilliseconds;
		milestones[curMilestone].AddValue(thisMSTime);
		lastMilestone = DateTime.Now;
		curMilestone++;
    }
	public static void EndTimer()
    {
		avg.AddValue((DateTime.Now - marker).TotalMilliseconds - calibratedOffset);
		samplesLeft--;
		if(samplesLeft <= 0)
        {
			Debug.Log("average time: " + avg.average + "ms worst: " + avg.highest + "ms");
			if(milestones != null)
            {
				string msString = "Milestones:\n";
				for(var i = 0; i < milestones.Count; i++)
                {
					msString += i + ": " + "average time: " + milestones[i].average + "ms worst: " + milestones[i].highest + "ms\n";
				}
				Debug.Log(msString);
            }
			samplesLeft = numSamples;
        }

    }
}

