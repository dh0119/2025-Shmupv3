using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private Slider bossSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;

    void Update()
    {
        if (Main.S == null || bossPanel == null || bossSlider == null) return;

        BossEnemy boss = Main.S.currentBoss;

        if (boss == null)
        {
            bossPanel.SetActive(false);
            return;
        }

        if (!bossPanel.activeSelf)
        {
            bossPanel.SetActive(true);
        }

        if (bossNameText != null)
        {
            bossNameText.text = "Boss";
        }

        bossSlider.maxValue = boss.MaxHealth;
        bossSlider.value = Mathf.Max(0f, boss.health);
    }
}
