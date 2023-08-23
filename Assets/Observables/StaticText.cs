using UnityEngine;
using System.Collections;
using TMPro;

public class StaticText : KQObserver
{
    public TextMeshPro txt;

    public override void Start()
    {
        base.Start();
    }

    public override void OnParameters()
    {
        base.OnParameters();
        txt.text = moduleParameters.ContainsKey("text") ? moduleParameters["text"] : "";
        txt.alignment = ViewModel.currentTheme.layout == ThemeData.LayoutStyle.TwoCol ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;
    }
}

