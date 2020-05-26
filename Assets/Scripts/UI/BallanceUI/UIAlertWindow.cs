using Ballance2.UI.BallanceUI;
using Ballance2.UI.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAlertWindow : UIWindow
{
    private void Start()
    {
        SetWindowType(WindowType.GlobalAlert);
        EventTriggerListener.Get(UIButtonConfirm.gameObject).onClick = (g) => { Close(); };
    }

    public void Show(string text, string title, string okText)
    {
        Title = title;
        UIDialogText.text = text;
        UIButtonConfirmText.text = okText;
        Show();
    }

    public Button UIButtonConfirm;
    public Text UIButtonConfirmText;
    public Text UIDialogText;
}
