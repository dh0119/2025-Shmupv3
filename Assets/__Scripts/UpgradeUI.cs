using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI S;

    [Header("UI References")]
    public GameObject rootPanel;

    public Button button1;
    public Button button2;
    public Button button3;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timerText;

    public TextMeshProUGUI button1Text;
    public TextMeshProUGUI button2Text;
    public TextMeshProUGUI button3Text;

    [Header("Settings")]
    public float selectionDuration = 10f;

    private List<string> currentOptions = new List<string>();
    private float timeRemaining = 0f;
    private bool isShowing = false;

    void Awake()
    {
        S = this;
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }

        button1.onClick.AddListener(() => ChooseOption(0));
        button2.onClick.AddListener(() => ChooseOption(1));
        button3.onClick.AddListener(() => ChooseOption(2));
    }

    void Update()
    {
        if (!isShowing) return;

        timeRemaining -= Time.unscaledDeltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }

        if (timeRemaining <= 0f)
        {
            AutoPickUpgrade();
        }
    }

    public void ShowOptions(List<string> options)
    {
        if (options == null || options.Count < 3) return;

        currentOptions = new List<string>(options);
        isShowing = true;
        timeRemaining = selectionDuration;

        if (rootPanel != null)
        {
            rootPanel.SetActive(true);
        }

        EventSystem.current.SetSelectedGameObject(null);

        button1Text.text = FormatUpgradeName(currentOptions[0]);
        button2Text.text = FormatUpgradeName(currentOptions[1]);
        button3Text.text = FormatUpgradeName(currentOptions[2]);

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    void ChooseOption(int index)
    {
        if (!isShowing) return;
        if (index < 0 || index >= currentOptions.Count) return;

        string chosen = currentOptions[index];
        Debug.Log("Player selected upgrade: " + chosen);

        Hero.S.ApplyUpgrade(chosen);
        HideOptions();
        Main.S.ResumeGameplay();
    }

    void AutoPickUpgrade()
    {
        if (!isShowing) return;
        if (currentOptions.Count == 0) return;

        int ndx = Random.Range(0, currentOptions.Count);
        string chosen = currentOptions[ndx];

        Debug.Log("Timer expired. Auto-picked upgrade: " + chosen);

        Hero.S.ApplyUpgrade(chosen);
        HideOptions();
        Main.S.ResumeGameplay();
    }

    void HideOptions()
    {
        isShowing = false;
        currentOptions.Clear();

        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
    }

    string FormatUpgradeName(string upgradeId)
    {
        switch (upgradeId)
        {
            case "projectile": return "+1 Projectile";
            case "firerate": return "+20% Fire Rate";
            case "damage": return "+20% Damage";
            case "speed": return "+10% Move Speed";
            case "missile": return "Missile Unlock";
            case "phaser": return "Phaser Unlock";
            case "laser": return "Laser Unlock";
            case "shield": return "+1 Shield";
            case "combine": return "Combine 2 Weapons";
            default: return upgradeId;
        }
    }
}
