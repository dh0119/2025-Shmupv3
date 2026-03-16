using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    void Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (scoreText == null) return;
        if (Main.S == null) return;

        scoreText.text = "Score: " + Main.S.score + "\nKills: " + Main.S.totalKills;
    }
}
