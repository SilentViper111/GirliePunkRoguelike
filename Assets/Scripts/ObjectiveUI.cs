using UnityEngine;

/// <summary>
/// Objective display UI panel.
/// Shows current objectives with progress.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class ObjectiveUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private Transform objectiveContainer;
    [SerializeField] private GameObject objectiveEntryPrefab;

    [Header("Animation")]
    [SerializeField] private float showDuration = 0.3f;
    [SerializeField] private CanvasGroup canvasGroup;

    private bool _isVisible = true;

    private void Start()
    {
        if (ObjectiveSystem.Instance != null)
        {
            ObjectiveSystem.Instance.OnObjectiveProgress += OnProgress;
            ObjectiveSystem.Instance.OnObjectiveCompleted += OnComplete;
        }

        RefreshUI();
    }

    private void Update()
    {
        // Toggle with Tab key
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
        {
            ToggleVisibility();
        }
    }

    private void ToggleVisibility()
    {
        _isVisible = !_isVisible;

        if (canvasGroup != null)
            canvasGroup.alpha = _isVisible ? 1f : 0f;
    }

    private void RefreshUI()
    {
        if (ObjectiveSystem.Instance == null) return;

        // This would instantiate objective entry UI for each active objective
        // For now, just log
        var objectives = ObjectiveSystem.Instance.GetActiveObjectives();
        foreach (var obj in objectives)
        {
            Debug.Log($"[ObjectiveUI] {obj.description}: {obj.currentValue}/{obj.targetValue}");
        }
    }

    private void OnProgress(ObjectiveSystem.Objective obj)
    {
        RefreshUI();
    }

    private void OnComplete(ObjectiveSystem.Objective obj)
    {
        RefreshUI();
        // Show completion animation
        Debug.Log($"[ObjectiveUI] COMPLETED: {obj.description}");
    }

    private void OnDestroy()
    {
        if (ObjectiveSystem.Instance != null)
        {
            ObjectiveSystem.Instance.OnObjectiveProgress -= OnProgress;
            ObjectiveSystem.Instance.OnObjectiveCompleted -= OnComplete;
        }
    }
}
