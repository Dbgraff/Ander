using TMPro;
using UnityEngine;

/// <summary>
/// Подсказка взаимодействия.
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text promptText;

    private PlayerInteractor interactor;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (interactor == null)
        {
            interactor = FindAnyObjectByType<PlayerInteractor>();
            if (interactor == null) return;
        }

        IInteractable current = interactor.Current;

        if (current == null)
        {
            if (panel.activeSelf) panel.SetActive(false);
            return;
        }

        promptText.text = $"[{interactor.InteractKey}] {current.Prompt}";
        if (!panel.activeSelf) panel.SetActive(true);
    }
}
