using UnityEngine;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;

    void Start()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Main.GAME_PAUSED = true;
    }

    public void StartGame()
{
    Debug.Log("StartGame clicked");

    if (startPanel != null)
    {
        startPanel.SetActive(false);
    }

    Time.timeScale = 1f;
    Main.GAME_PAUSED = false;
}
}
