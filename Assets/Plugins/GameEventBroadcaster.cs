using System.Runtime.InteropServices;

public class GameEventBroadcaster
{
    [DllImport("__Internal")]
    public static extern void JSGameEvent(string text);
}