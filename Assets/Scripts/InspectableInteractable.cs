using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Объект, который можно осмотреть.
/// </summary>
public class InspectableInteractable : MonoBehaviour, IInteractable
{
    [Header("Взаимодействие")]
    [SerializeField] private string prompt = "Осмотреть";

    [Header("Камера осмотра")]
    [Tooltip("Cinemachine Camera, расположеная у объекта.")]
    [SerializeField] private GameObject inspectCamera;
    [Tooltip("Запасное время ожидания, если Cinemachine Brain не найден.")]
    [SerializeField] private float fallbackBlendTime = 1f;

    [Header("Описание")]
    [SerializeField] private string inspectTitle;
    [TextArea(3, 8)]
    [SerializeField] private string inspectText;

    [Header("Курсор")]
    [SerializeField] private bool showCursor;

    [Header("События")]
    [SerializeField] private UnityEvent onInspectStart;
    [SerializeField] private UnityEvent onInspectEnd;

    /// <summary>
    /// Идет ли осмотр прямо сейчас
    /// </summary>
    public bool IsInspecting { get; private set; }
    
    public string Prompt => prompt;
    public bool CanInteract => !IsInspecting;

    /// <summary>
    /// Запускает осмотр.
    /// </summary>
    /// <param name="interactor"></param>
    public void Interact(GameObject interactor)
    {
        if (!CanInteract) return;
        StartCoroutine(InspectRoutine(interactor));
    }

    /// <summary>
    /// Последовательность осмотра.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator InspectRoutine(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
        CinemachineBrain brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;

        CursorLockMode prevLock = Cursor.lockState;
        bool prevVisible = Cursor.visible;

        IsInspecting = true;
        movement.CanMove = false;
        interactor.InteractionLocked = true;
        SetPlayerCameraLocked(true);
        if (showCursor) SetCursor(true);

        try
        {
            if (inspectCamera != null) inspectCamera.SetActive(true);
            yield return WaitForBlend(brain);

            bool hasContent = !string.IsNullOrEmpty(inspectTitle)
                || !string.IsNullOrEmpty(inspectText);

            if (hasContent && InspectUI.Instance != null)
            {
                yield return InspectUI.Instance.ShowRoutine(inspectTitle, inspectText);
                SetPlayerCameraLocked(true);
            }

            onInspectEnd?.Invoke();

            if (inspectCamera != null) inspectCamera.SetActive(false);
            yield return WaitForBlend(brain);

            yield return null;
        }
        finally
        {
            if (inspectCamera != null) inspectCamera.SetActive(false);
            IsInspecting = false;
            SetPlayerCameraLocked(false);
            Cursor.lockState = prevLock;
            Cursor.visible = prevVisible;
            interactor.InteractionLocked = false;
            movement.CanMove = true;
        }
    }

    /// <summary>
    /// Ждет окончания перелета камеры.
    /// </summary>
    /// <param name="brain"></param>
    /// <returns></returns>
    private IEnumerator WaitForBlend(CinemachineBrain brain)
    {
        if (brain == null)
        {
            yield return new WaitForSeconds(fallbackBlendTime);
            yield break;
        }

        yield return null;
        while (brain.IsBlending)
            yield return null;
    }

    /// <summary>
    /// Блокирует или разблокирует вращение камеры игрока
    /// </summary>
    /// <param name="locked"></param>
    private void SetPlayerCameraLocked(bool locked)
    {
        if (CameraInputLock.Instance != null)
            CameraInputLock.Instance.SetLocked(locked);
    }

    /// <summary>
    /// Показывает курсор и снимает с него блокировку
    /// </summary>
    /// <param name="visible"></param>
    private void SetCursor(bool visible)
    {
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }
}
