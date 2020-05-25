using Ballance2;
using Ballance2.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

public class GameGlobalErrorUI : MonoBehaviour
{
    public Button GlobalGameErrorButtonQuitGame;
    public Text GlobalGameErrorContent;

    private void Start()
    {
        EventTriggerListener.Get(GlobalGameErrorButtonQuitGame.gameObject).onClick =
            (g) => GameManager.QuitGame();
    }

    public void ShowErrorUI(string errContent)
    {
        GlobalGameErrorContent.text = errContent;
        gameObject.SetActive(true);
    }
}
