using UnityEngine;

public class SubView : MonoBehaviour
{

    protected class CamData
    {
        public Vector2 pos;
        public float scale;
    }
    public SpriteRenderer bgSprite;

    virtual public void OnSubViewStarted(params string[] args)
    {

    }

    virtual public void OnSubViewClosed(params string[] args)
    {

    }
} 