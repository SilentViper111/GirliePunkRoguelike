using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss health bar UI displayed during boss fights.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Header("Colors")]
    [SerializeField] private Color phase1Color = Color.magenta;
    [SerializeField] private Color phase2Color = Color.yellow;
    [SerializeField] private Color phase3Color = Color.red;
    [SerializeField] private Color enragedColor = new Color(1f, 0f, 0f, 1f);

    private BossController _boss;

    private void Start()
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);
    }

    public void SetBoss(BossController boss, string bossName = "CYBER QUEEN")
    {
        _boss = boss;
        
        if (bossUIPanel != null)
            bossUIPanel.SetActive(true);

        if (bossNameText != null)
            bossNameText.text = bossName;

        // Subscribe to events
        if (_boss != null)
        {
            _boss.OnHealthChanged += UpdateHealthBar;
            _boss.OnPhaseChanged += UpdatePhase;
        }

        UpdateHealthBar(_boss.CurrentHealth, _boss.MaxHealth);
        UpdatePhase(BossController.BossPhase.Phase1);
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (healthSlider != null)
            healthSlider.value = current / max;

        // Check if boss died
        if (current <= 0 && bossUIPanel != null)
        {
            StartCoroutine(FadeOutUI());
        }
    }

    private void UpdatePhase(BossController.BossPhase phase)
    {
        Color phaseColor = phase1Color;
        string phaseName = "PHASE 1";

        switch (phase)
        {
            case BossController.BossPhase.Phase1:
                phaseColor = phase1Color;
                phaseName = "PHASE 1";
                break;
            case BossController.BossPhase.Phase2:
                phaseColor = phase2Color;
                phaseName = "PHASE 2";
                break;
            case BossController.BossPhase.Phase3:
                phaseColor = phase3Color;
                phaseName = "PHASE 3";
                break;
            case BossController.BossPhase.Enraged:
                phaseColor = enragedColor;
                phaseName = "ENRAGED!";
                break;
        }

        if (healthFill != null)
            healthFill.color = phaseColor;

        if (phaseText != null)
            phaseText.text = phaseName;
    }

    private System.Collections.IEnumerator FadeOutUI()
    {
        yield return new WaitForSeconds(2f);
        
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_boss != null)
        {
            _boss.OnHealthChanged -= UpdateHealthBar;
            _boss.OnPhaseChanged -= UpdatePhase;
        }
    }
}
