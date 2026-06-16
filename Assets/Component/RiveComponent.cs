using UnityEngine;
using Rive.Components;
using Rive;
using Unity.Properties;
using ZXing.OneD;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
public class RiveProperty<T>
{
    public string key;
    public ViewModelInstance refViewModel; 
    virtual public T property { get {return default(T);} set {}}

}
/*
public class RiveEmojiString : RiveProperty<string>
{
    
}
*/
public class RiveString : RiveProperty<string>
{
    override public string property { get { return refViewModel.GetStringProperty(key).Value; } set { refViewModel.GetStringProperty(key).Value = value; } } 
    public RiveString(ViewModelInstance vm, string k)
    {
        refViewModel = vm;
        key = k;
    }
    public RiveString(ViewModelInstance vm, string k, string initialValue)
    {
        refViewModel = vm;
        key = k;
        property = initialValue;
    }
}

public class RiveStringRun : RiveProperty<string>
{
    public string actualString;
    public int numberOfPairs = 3;

        public RiveStringRun(ViewModelInstance vm, string k)
    {
        refViewModel = vm;
        key = k;
    }
    public RiveStringRun(ViewModelInstance vm, string k, string initialValue)
    {
        refViewModel = vm;
        key = k;
        property = initialValue;
    }
    public override string property { get => actualString; set {
            actualString = value;
            bool curEmoji = true;
            for(int i = 0; i < numberOfPairs * 2; i++)
            {
                refViewModel.GetStringProperty(ss(i, curEmoji)).Value = "";
                curEmoji = !curEmoji;
            }
            if(actualString == "") return;
            curEmoji = !Char.IsLetterOrDigit(actualString[0]);
            int curIndex = curEmoji ? 0 : 1;
            int curCharIndex = 0;
            string curSubString = "";
            bool isFinal = false;
            while(curIndex < numberOfPairs * 2 && curCharIndex < actualString.Length)
            {
                curSubString += actualString[curCharIndex];
                curCharIndex++;
                if(curCharIndex >= actualString.Length)
                {
                    refViewModel.GetStringProperty(ss(curIndex, curEmoji)).Value = curSubString;
                    break;
                }
                if(!isFinal)
                {
                    if((curEmoji && (Char.IsLetterOrDigit(actualString[curCharIndex]) || actualString[curCharIndex] == ' ')) || ((!curEmoji) && !(Char.IsLetterOrDigit(actualString[curCharIndex]) || actualString[curCharIndex] == ' ')))
                    {
                        refViewModel.GetStringProperty(ss(curIndex, curEmoji)).Value = curSubString;
                        curSubString = "";
                        curEmoji = !curEmoji;
                        curIndex++;
                        if(curIndex + 1 >= numberOfPairs * 2)
                            isFinal = true;
                    }
                }
            }
        }
    }

    string ss(int num, bool emoji)
    {
        return key + num.ToString() + (emoji ? "Emoji" : "Text");
    }
}
public class RiveFloat : RiveProperty<float>
{
    override public float property { get { return refViewModel.GetNumberProperty(key).Value; } set { refViewModel.GetNumberProperty(key).Value = value; } } 
    public RiveFloat(ViewModelInstance vm, string k)
    {
        refViewModel = vm;
        key = k;
    }
    public RiveFloat(ViewModelInstance vm, string k, float initialValue)
    {
        refViewModel = vm;
        key = k;
        property = initialValue;
    }
}

public class RiveInt : RiveProperty<int>
{
    override public int property { get { return (int)refViewModel.GetNumberProperty(key).Value; } set { refViewModel.GetNumberProperty(key).Value = value; } } 
        public RiveInt(ViewModelInstance vm, string k)
    {
        refViewModel = vm;
        key = k;
    }
    public RiveInt(ViewModelInstance vm, string k, int initialValue)
    {
        refViewModel = vm;
        key = k;
        property = initialValue;
    }
}

public class RiveColor : RiveProperty<UnityEngine.Color>
{
    override public UnityEngine.Color property { get { return refViewModel.GetColorProperty(key).Value; } set { refViewModel.GetColorProperty(key).Value = value; } } 
    public RiveColor(ViewModelInstance vm, string k)
    {
        refViewModel = vm;
        key = k;
    }
    public RiveColor(ViewModelInstance vm, string k, UnityEngine.Color initialValue)
    {
        refViewModel = vm;
        key = k;
        property = initialValue;
    }
}

public class RiveBool : RiveProperty<bool>
{
    override public bool property { get { return refViewModel.GetBooleanProperty(key).Value; } set { refViewModel.GetBooleanProperty(key).Value = value; } } 
    public RiveBool(ViewModelInstance vm, string k)
    {
        refViewModel = vm;
        key = k;
    }
    public RiveBool(ViewModelInstance vm, string k, bool initialValue)
    {
        refViewModel = vm;
        key = k;
        property = initialValue;
    }
}

public class RiveTrigger : RiveProperty<bool>
{
    override public bool property { get { return false; } set { if(value) refViewModel.GetTriggerProperty(key).Trigger(); } } 
    public RiveTrigger(ViewModelInstance vm, string k)
    {
        refViewModel = vm;
        key = k;
    }
    public RiveTrigger(ViewModelInstance vm, string k, bool initialValue)
    {
        refViewModel = vm;
        key = k;
        property = initialValue;
    }
}

public class RiveImage : RiveProperty<string>
{
    public string url;
    bool requestedImageLoad = false;
    ImageOutOfBandAsset curImage;
    ImageEmbeddedAssetReference assetReference;
    override public string property { get { return url; } set { url = value; SetImage(); } } 
    public RiveImage(ImageEmbeddedAssetReference assetRef)
    {
        assetReference = assetRef;
    }
    public RiveImage(ImageEmbeddedAssetReference assetRef, string initialValue)
    {
        assetReference = assetRef;
        property = initialValue;
    }

    void SetImage()
    {
        if(NetworkManager.RiveImageCache.ContainsKey(url))
        {
            if(curImage != null)
            {
                curImage.Unload();
            }
            curImage = NetworkManager.RiveImageCache[url];
            curImage.Load();
            assetReference.SetImage(curImage);
        } else if(!requestedImageLoad)
        {
            requestedImageLoad = true;
            NetworkManager.GetDynamicRiveImage(url);
            NetworkManager.instance.onRiveImageLoaded.AddListener(loadedURL =>
            {
               if(loadedURL == url)
                {
                    SetImage();
                } 
            });
        }
    }
}
public class RiveComponent : MonoBehaviour
{
    public RiveWidget widget;
    public Asset riveAsset;
    public ViewModelInstance viewModel;

    protected bool isLoaded = false;

    virtual protected bool OobAssetLoaderDelegate(EmbeddedAssetReference assetReference)
    {
        Debug.Log("Override this");
        return false;
    }

    virtual protected void Start()
    {
        if(riveAsset != null)
        {
            var file = Rive.File.Load(riveAsset, OobAssetLoaderDelegate);
            widget.Load(file);
        }
    }
    virtual protected void Update()
    {
        if(!isLoaded && widget.Status == WidgetStatus.Loaded)
            {
                isLoaded = true;
                viewModel = widget.StateMachine.ViewModelInstance;
                OnLoaded();
            }
    }

    virtual protected void OnLoaded()
    {

    }
}

public class RiveSubComponent
{
    public ViewModelInstance viewModel;
}