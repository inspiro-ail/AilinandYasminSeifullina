using UnityEngine;
using TMPro;

public class BoardUI : MonoBehaviour
{
    public TMP_Text winText;

    private void Awake()
    {
        if (winText) winText.gameObject.SetActive(false);
    }

    public void ShowWin(string message = "You did it!")
    {
        Debug.Log("[BoardUI] ShowWin");
        if (winText)
        {
            winText.text = message;
            winText.gameObject.SetActive(true);
            winText.enabled = true;
        }
    }

    public void Hide()
    {
        if (winText)
        {
            winText.gameObject.SetActive(false);
            winText.enabled = false;
        }
    }
}