using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Окно с запиской. Закрывается по нажатию кнопки взаимодействия
/// </summary>
public class NoteUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image routeImage;
    [SerializeField] private KeyCode closeKey = KeyCode.E;

    public static NoteUI Instance { get; private set; }

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public IEnumerator ShowRoutine(string title, string body, Sprite image = null)
    {
        titleText.text = title;
        bodyText.text = body;
        routeImage.sprite = image;
        routeImage.gameObject.SetActive(image != null);

        IsOpen = true;
        panel.SetActive(true);

        if (CameraInputLock.Instance != null) 
            CameraInputLock.Instance.SetLocked(true);

        yield return null;
        while(!Input.GetKeyDown(closeKey)) 
            yield return null;

        panel.SetActive(false);
        if (CameraInputLock.Instance != null)
            CameraInputLock.Instance.SetLocked(false);
        IsOpen = false;
    }
}
