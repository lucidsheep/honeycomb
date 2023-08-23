using UnityEngine;
using System.Collections;

public class Util
{
    public static string FormatTime(float totalSeconds)
    {
        bool isNegative = totalSeconds < 0f;
        if (isNegative)
        {
            totalSeconds = Mathf.Abs(totalSeconds);
            totalSeconds += 1f;
        }
        bool hasHours = totalSeconds >= 3600f;
        string hours = (hasHours ? Mathf.FloorToInt(totalSeconds / 3600f).ToString() + ":" : "");
        totalSeconds = Mathf.FloorToInt(totalSeconds) % 3600;
        string minutes = Mathf.FloorToInt(totalSeconds / 60f).ToString() + ":";
        minutes = (hasHours && minutes.Length == 2 ? "0" : "") + minutes;
        totalSeconds = Mathf.FloorToInt(totalSeconds) % 60;
        string seconds = (totalSeconds < 10 ? "0" : "") + totalSeconds.ToString();
        return (isNegative ? "-" : "") + hours + minutes + seconds;
    }

    public static string FormatTime(int milliseconds)
    {
        return FormatTime(milliseconds / 1000f);
    }

    public static string AddZeroes(int input, int length)
    {
        string ret = input.ToString();
        while (ret.Length < length)
            ret = "0" + ret;
        return ret;
    }
    public static float AdjustRange(float val, Vector2 oldRange, Vector2 newRange)
    {
        return (((val - oldRange.x) / (oldRange.y - oldRange.x)) * (newRange.y - newRange.x)) + newRange.x;
    }

    public static string SmartTruncate(string input, int numChars)
    {
        if (input.Length <= numChars) return input;

        string[] spaced = input.Split(' ');
        if (spaced[0].Length + 2 > numChars) //first name is too big, gotta use elipsis
            return spaced[0].Substring(0, numChars - 2) + "...";
        int count = 0;
        int i = 0;
        string ret = "";
        do
        {
            ret += spaced[i] + " ";
            count += spaced[i].Length + 1;
            i++;
        } while (i < spaced.Length && count + spaced[i].Length + 2 < numChars);
        //do final initial
        ret += spaced[spaced.Length - 1][0];
        return ret;
    }

}

