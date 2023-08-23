using UnityEngine;
using System.Collections.Generic;

public class LSTimer
{

    public delegate void TimerDelegate();
    public float duration;
    public TimerDelegate onComplete;
    public int loop;
    public float startTime;
    public int id;
    public int tickDuration;
    public int ticksLeft;

    public float progress
    {
        get
        {
            if (mode == Mode.Time)
                return (startTime - duration) / startTime;
            else
                return (tickDuration - ticksLeft) / tickDuration; 
        }
    }
    public enum Mode {Time, Ticks}
    public Mode mode;
    public LSTimer(float time, TimerDelegate complete, int timerID = 0, int numLoops = 1)
    {
        duration = startTime = time;
        loop = numLoops;
        onComplete = complete;
        id = timerID;
        mode = Mode.Time;
        LSTimeManager.timers.Add(this);
    }

    public LSTimer(int ticks, TimerDelegate complete, int timerID = 0, int numLoops = 1)
    {
        tickDuration = ticksLeft = ticks;
        loop = numLoops;
        onComplete = complete;
        id = timerID;
        mode = Mode.Ticks;
        LSTimeManager.timers.Add(this);
    }

    public void CancelTimer(bool invokeCompletion = false)
    {
        LSTimeManager.timers.Remove(this);
        if (invokeCompletion)
            onComplete.Invoke();
    }

    public static void CancelTimer(LSTimer timer, bool invokeCompletion = false)
    {
        var found = LSTimeManager.timers.Find(x => x == timer);
        if(found != null)
        {
            found.CancelTimer(invokeCompletion);
        }
    }
}

public class LSTimeManager : MonoBehaviour
{
    public static LSTimeManager instance;
    public static List<LSTimer> timers = new List<LSTimer>();
    public static List<LSTimer> toRemove = new List<LSTimer>();

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {

        for (int i = 0; i < timers.Count; i++)
        {
            LSTimer t = timers[i];
            if (t.mode == LSTimer.Mode.Ticks)
                continue;
            t.duration -= Time.deltaTime;
            if (t.duration <= 0f)
            {
                t.onComplete.Invoke();
                t.loop -= 1;
                if (t.loop != 0) t.duration += t.startTime;
                else
                    toRemove.Add(t);
            }
        }
        if (toRemove.Count > 0)
        {
            foreach (LSTimer t in toRemove)
                timers.Remove(t);
            toRemove = new List<LSTimer>();
        }
        //SetTimeMultiplier(2f);
    }
    void FixedUpdate()
    {
        for (int i = 0; i < timers.Count; i++)
        {
            LSTimer t = timers[i];
            if (t.mode == LSTimer.Mode.Time)
                continue;
            t.ticksLeft--;
            if (t.ticksLeft <= 0)
            {
                t.onComplete.Invoke();
                t.loop -= 1;
                if (t.loop != 0) t.ticksLeft += t.tickDuration;
                else
                    toRemove.Add(t);
            }
        }
        if (toRemove.Count > 0)
        {
            foreach (LSTimer t in toRemove)
                timers.Remove(t);
            toRemove = new List<LSTimer>();
        }
    }
}