using UnityEngine;
using ConnectFour;

public class ModeSelector : MonoBehaviour
{
    public GameObject ModePanel;

    public void SetMode(string mode)
    {
        ModePanel.SetActive(true);
        // Debug.Log("Selected difficulty: " + mode);

        // PlayerPrefs.SetString("GameMode", mode);
        FindFirstObjectByType<GameController>().SetGameMode(mode);

        ModePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
