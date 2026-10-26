using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Шокер под подушкой: Андер достает его, читает записку Кэмерон,
/// после чего шокер остается у нее в руке
/// </summary>
public class ShockerInteractable : MonoBehaviour, IInteractable
{
    [Header("Взаимодействие")]
    [SerializeField] private string prompt = "Взять шокер";
    [Tooltip("Если задана, шокер доступен только после того, как игрок встал с кровати")]
    [SerializeField] private BedInteractable requiredBed;

    [Header("Предмет")]
    [Tooltip("Модель шокера. Должна быть отдельным объектом")]
    [SerializeField] private GameObject shockerModel;

    [Header("Анимация")]
    [SerializeField] private string takeTrigger = "TakeShocker";
    [SerializeField] private float takeDuration = 1.5f;

    [Header("Записка")]
    [SerializeField] private string noteTitle = "Записка от Кэмерон";
    [TextArea(3, 8)]
    [SerializeField] private string noteText = "Окно 3 минуты.\nПуть к архиву:";
    [SerializeField] private Sprite routeSprite;

    private bool taken;

    public event Action OnShockerTaken;

    public bool Taken => taken;

    public string Prompt => prompt;
    public bool CanInteract => !taken && (requiredBed == null || requiredBed.Finished);

    /// <summary>
    /// Запускает последовательность "достать шокер, прочитать записку"
    /// </summary>
    /// <param name="interactor"></param>
    public void Interact(GameObject interactor)
    {
        if (!CanInteract) return;
        taken = true;
        StartCoroutine(TakeRoutine(interactor));
    }

    /// <summary>
    /// Достать шокер, положить в руку, показать записку, вернуть управление
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator TakeRoutine(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
        Animator animator = player.GetComponent<Animator>();

        movement.CanMove = false;
        interactor.InteractionLocked = true;

        if (animator != null && !string.IsNullOrEmpty(takeTrigger))
            animator.SetTrigger(takeTrigger);
        yield return new WaitForSeconds(takeDuration);

        PutInHand(player);

        if (NoteUI.Instance != null)
            yield return NoteUI.Instance.ShowRoutine(noteTitle, noteText, routeSprite);
        else
            Debug.LogWarning("На сцене нет NoteUI, записка не показана.");

        yield return null;

        interactor.InteractionLocked = false;
        movement.CanMove = true;
        OnShockerTaken?.Invoke();
    }

    private void PutInHand(GameObject player)
    {
        if (shockerModel == null) return;

        Transform socket = player.GetComponentsInChildren<Transform>().First(x => x.name == "HandSocket");
        if (socket != null)
        {
            shockerModel.transform.SetParent(socket, false);
            shockerModel.transform.localPosition = Vector3.zero;
            shockerModel.transform.localRotation = Quaternion.identity;
        }
    }
}
