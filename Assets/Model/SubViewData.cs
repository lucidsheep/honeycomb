using UnityEngine;

[CreateAssetMenu()]
public class SubViewData : ScriptableObject
{
    public string viewName;
    public int streamDeckOrder;
    public string streamDeckIcon;

    public SubView viewObject;

}