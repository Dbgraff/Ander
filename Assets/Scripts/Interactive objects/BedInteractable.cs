using System.Collections;
using UnityEngine;

// todo добавить Interact_00_Bed
// На Interact__00_Bed добавь Box Collider, включи Is Trigger и растяни его так, чтобы он покрывал кровать и немного места вокруг. Игрок лежит в кровати, поэтому он должен оказаться внутри радиуса (1,5 по умолчанию).
// Добавь на Interact__00_Bed скрипт BedInteractable.
// Создай дочерний пустой объект StandPoint рядом с кроватью, на уровне пола, и перетащи его в поле Stand Point. Его поворот задаёт, куда Андер смотрит после вставания.
// Позицию Player на старте поставь на кровать.

// Проверить

/// <summary>
/// Кровать: первое действие игрока после катсцены.
/// Андер садится, затем встает и оказывается у кровати.
/// Срабатывает один раз.
/// </summary>
public class BedInteractable : MonoBehaviour
{
    [Header("Запуск")]
    [Tooltip("РЕЖИМ РАЗРАБОТКИ: пропуск подъема с кровати")]
    [SerializeField] private bool skipForDev;

    [Header("Куда встает Андер")]
    [SerializeField] private Transform standPoint;

    [Header("Тайминги")]
    [SerializeField] private float sitUpDuration = 1.5f;
    [SerializeField] private float standUpDuration = 1.5f;

    [Header("Триггеры анимации")]
    [SerializeField] private string sitUpTrigger = "SitUp";
    [SerializeField] private string standUpTrigger = "StandUp";

    [Header("Старт")]
    [Tooltip("Запретить ходьбу, пока игрок не встанет с кровати")]
    [SerializeField] private bool lockPlayerOnStart = true;

    private bool used;

    public bool CanInteract => !used;

    /// <summary>
    /// true, когда игрок полностью встал и получил управление
    /// </summary>
    public bool Finished { get; private set; }

    private IEnumerator Start()
    {
        if (skipForDev)
        {
            used = true;
            Finished = true;
            yield break;
        }

        if (!lockPlayerOnStart) yield break;

        PlayerMovement movement = FindAnyObjectByType<PlayerMovement>();
        if (movement != null)
            movement.CanMove = false;
    }

    public void StartGetUp()
    {
        if (used) return;

        PlayerMovement movement = FindAnyObjectByType<PlayerMovement>();
        if (movement == null)
        {
            Debug.LogWarning("BedInteractable: игрок на сцене не найден");
            return;
        }

        used = true;
        StartCoroutine(GetUpRoutine(movement.gameObject));
    }

    /// <summary>
    /// Запускает последовательность "сесть, встать"
    /// </summary>
    /// <param name="interactor"></param>
    public void Interact(GameObject interactor)
    {
        if (!CanInteract) return;
        used = true;
        StartCoroutine(GetUpRoutine(interactor));
    }

    /// <summary>
    /// Сесть, подождать, встать, переставить игрока с кровати, вернуть управление
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator GetUpRoutine(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        CharacterController controller = player.GetComponent<CharacterController>();
        Animator animator = player.GetComponent<Animator>();

        movement.CanMove = false;

        PlayTrigger(animator, sitUpTrigger);
        yield return new WaitForSeconds(sitUpDuration);

        PlayTrigger(animator, standUpTrigger);
        yield return new WaitForSeconds(standUpDuration);

        if (standPoint != null)
        {
            controller.enabled = false;
            player.transform.SetPositionAndRotation(standPoint.position, standPoint.rotation);
            controller.enabled = true;
        }

        movement.CanMove = true;
        Finished = true;
    }

    /// <summary>
    /// Запускает триггер аниматора, если аниматор и имя заданы
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="triggerName"></param>
    private void PlayTrigger(Animator animator, string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
            animator.SetTrigger(triggerName);
    }
}
