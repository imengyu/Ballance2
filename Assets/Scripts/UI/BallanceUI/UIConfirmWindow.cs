using Ballance2.UI.BallanceUI;
using Ballance2.UI.Utils;
using UnityEngine.UI;

public class UIConfirmWindow : UIWindow
{
    public Button UIButtonConfirm;
    public Button UIButtonCancel;
    public Text UIButtonConfirmText;
    public Text UIButtonCancelText; 
    public Text UIConfirmText;

    public UIConfirmWindow()
    {
        SetWindowType(WindowType.GlobalAlert);
    }

    private void Start()
    {
        CanClose = false;
        EventTriggerListener.Get(UIButtonConfirm.gameObject).onClick = (g) => { Close(true); };
        EventTriggerListener.Get(UIButtonCancel.gameObject).onClick = (g) => { Close(false); };
    }

    public void Show(string text, string title, string okText, string cancelText)
    {
        Title = title;
        UIConfirmText.text = text;
        UIButtonConfirmText.text = okText;
        UIButtonCancelText.text = cancelText;
        Show();
    }
    public void Close(bool confirm)
    {
        IsConfirmed = confirm;
        Close();
    }

    public bool IsConfirmed { get; private set; }

}
