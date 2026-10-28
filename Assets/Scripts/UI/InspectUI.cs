using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Окно описания при осмотре объекта.
/// </summary>
public class InspectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText; // or thinks
    [SerializeField] private KeyCode closeKey = KeyCode.E;

    /// <summary>
    /// Единственный экземпляр на сцене
    /// </summary>
    public static InspectUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public IEnumerator ShowRoutine(string title, string body)
    {
        titleText.text = title;
        titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
        bodyText.text = body;

        panel.SetActive(true);

        yield return null;
        while (!Input.GetKeyDown(closeKey))
            yield return null;

        panel.SetActive(false);
    }
}
