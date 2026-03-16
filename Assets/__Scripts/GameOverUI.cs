using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    private bool shown = false;

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        shown = false;
    }

    void Update()
    {
        if (shown) return;
        if (gameOverPanel == null) return;

        if (Hero.S == null)
        {
            gameOverPanel.SetActive(true);
            shown = true;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Main.GAME_PAUSED = false;

        if (Main.S != null)
        {
            Main.S.Restart();
        }
    }
}
