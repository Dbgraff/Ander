using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BoxInteractable : MonoBehaviour, IInteractable
{
    [Header("Взаимодействие")]
    [SerializeField] private string prompt = "Открыть коробку";

    [Header("Содержимое")]
    [SerializeField] private GameObject panel;
    [SerializeField] private KeyCode closeKey = KeyCode.E;

    [Header("События")]
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;

    private bool isOpen;

    public bool IsOpen => isOpen;
    public bool WasOpened { get; private set; }

    public string Prompt => prompt;
    public bool CanInteract => !isOpen && panel != null;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract) return;
        StartCoroutine(ViewRoutine(interactor));
    }

    private IEnumerator ViewRoutine(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();

        CursorLockMode prevLock = Cursor.lockState;
        bool prevVisible = Cursor.visible;

        isOpen = true;
        WasOpened = true;
        movement.CanMove = false;
        interactor.InteractionLocked = true;
        if (CameraInputLock.Instance != null) CameraInputLock.Instance.SetLocked(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panel.SetActive(true);
        onOpened?.Invoke();

        yield return null;
        while (!Input.GetKeyDown(closeKey))
            yield return null;

        panel.SetActive(false);
        onClosed?.Invoke();

        Cursor.lockState = prevLock;
        Cursor.visible = prevVisible;
        if (CameraInputLock.Instance != null)
            CameraInputLock.Instance.SetLocked(false);
        movement.CanMove = true;
        isOpen = false;

        yield return null;
        interactor.InteractionLocked = false;
    }
}
