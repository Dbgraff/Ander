using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Тележка, которую игрок берёт за ручку и двигает вперёд и назад вдоль одной оси.
/// Скрипт висит на объекте взаимодействия, сама тележка подключается ссылкой.
/// W толкает от игрока, S тянет к игроку, повторное нажатие кнопки взаимодействия отпускает.
/// Вместе с тележкой едет и объект взаимодействия, чтобы зона E не оставалась на старом месте.
/// </summary>
public class CartInteractable : MonoBehaviour, IInteractable
{
    [Header("Взаимодействие")]
    [SerializeField] private string prompt = "Взяться за тележку";

    [Header("Тележка")]
    [Tooltip("Объект тележки (модель и коллайдер). Ось движения: синяя стрелка Z этого объекта")]
    [SerializeField] private Transform cart;

    [Header("Точки хвата")]
    [Tooltip("Где стоит игрок. Синяя ось Z точки должна смотреть на тележку. Лучше сделать их дочерними к объекту взаимодействия, тогда они поедут вместе с ним")]
    [SerializeField] private Transform[] grabPoints;

    [Header("Движение")]
    [SerializeField] private float pushSpeed = 1.5f;
    [Tooltip("Насколько можно сдвинуть тележку против оси Z тележки от стартовой позиции (метры, 0 или меньше)")]
    [SerializeField] private float minOffset = -2f;
    [Tooltip("Насколько можно сдвинуть тележку по оси Z тележки от стартовой позиции (метры, 0 или больше)")]
    [SerializeField] private float maxOffset = 2f;

    [Header("События")]
    [SerializeField] private UnityEvent onGrab;
    [SerializeField] private UnityEvent onRelease;

    private Collider[] cartColliders;
    private Vector3 axis;
    private float offset;
    private bool moveSelf;

    private bool grabbed;
    private int grabFrame;
    private Transform currentGrab;
    private CharacterController controller;
    private PlayerMovement movement;
    private PlayerInteractor interactor;

    public string Prompt => prompt;
    public bool CanInteract => !grabbed && cart != null && grabPoints != null && grabPoints.Length > 0;

    private void Awake()
    {
        if (cart == null)
        {
            Debug.LogError("CartInteractable: не задана тележка (поле Cart).", this);
            return;
        }

        cartColliders = cart.GetComponentsInChildren<Collider>();
        axis = Vector3.ProjectOnPlane(cart.forward, Vector3.up).normalized;

        // Если объект взаимодействия лежит внутри тележки, он и так поедет вместе с ней
        moveSelf = !transform.IsChildOf(cart);
    }

    /// <summary>
    /// Берёт тележку: игрок встаёт в ближайшую точку хвата, управление ходьбой отключается.
    /// </summary>
    /// <param name="interactorObject">Объект игрока.</param>
    public void Interact(GameObject interactorObject)
    {
        if (!CanInteract) return;

        controller = interactorObject.GetComponent<CharacterController>();
        movement = interactorObject.GetComponent<PlayerMovement>();
        interactor = interactorObject.GetComponent<PlayerInteractor>();
        currentGrab = NearestGrabPoint(interactorObject.transform.position);

        movement.CanMove = false;
        interactor.InteractionLocked = true;

        // Игрок не должен упираться в тележку, пока держит её
        foreach (Collider c in cartColliders)
            Physics.IgnoreCollision(controller, c, true);

        // CharacterController перебивает позицию, пока включён
        controller.enabled = false;
        interactorObject.transform.SetPositionAndRotation(currentGrab.position, currentGrab.rotation);
        controller.enabled = true;

        grabFrame = Time.frameCount;
        grabbed = true;
        onGrab?.Invoke();
    }

    private void Update()
    {
        if (!grabbed) return;

        // Нажатие, которым взялись за тележку, не должно её сразу отпустить
        if (Time.frameCount != grabFrame && Input.GetKeyDown(interactor.InteractKey))
        {
            Release();
            return;
        }

        float input = Input.GetAxisRaw("Vertical");
        if (Mathf.Approximately(input, 0f)) return;

        // Направление "от игрока" в плоскости пола. Знак показывает, совпадает ли оно с осью тележки
        Vector3 pushDir = Vector3.ProjectOnPlane(currentGrab.forward, Vector3.up).normalized;
        float sign = Mathf.Sign(Vector3.Dot(pushDir, axis));

        float target = Mathf.Clamp(offset + sign * input * pushSpeed * Time.deltaTime, minOffset, maxOffset);
        float step = target - offset;
        if (Mathf.Approximately(step, 0f)) return;

        Vector3 delta = axis * step;
        offset = target;

        // Тележка, объект взаимодействия и игрок смещаются на одно и то же расстояние
        cart.position += delta;
        if (moveSelf) transform.position += delta;
        controller.Move(delta);
    }

    /// <summary>
    /// Отпускает тележку и возвращает игроку управление.
    /// </summary>
    private void Release()
    {
        grabbed = false;

        foreach (Collider c in cartColliders)
            Physics.IgnoreCollision(controller, c, false);

        movement.CanMove = true;
        onRelease?.Invoke();

        // Разблокируем взаимодействие на следующем кадре, иначе то же нажатие E снова возьмёт тележку
        StartCoroutine(UnlockInteractionNextFrame(interactor));
    }

    /// <summary>
    /// Снимает блокировку взаимодействия через кадр.
    /// </summary>
    /// <param name="target">Взаимодействие игрока.</param>
    private IEnumerator UnlockInteractionNextFrame(PlayerInteractor target)
    {
        yield return null;
        target.InteractionLocked = false;
    }

    /// <summary>
    /// Находит ближайшую к игроку точку хвата.
    /// </summary>
    /// <param name="playerPosition">Позиция игрока.</param>
    /// <returns>Ближайшая точка хвата.</returns>
    private Transform NearestGrabPoint(Vector3 playerPosition)
    {
        Transform best = grabPoints[0];
        float bestDist = float.MaxValue;

        foreach (Transform point in grabPoints)
        {
            float dist = (point.position - playerPosition).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = point;
            }
        }

        return best;
    }

    /// <summary>
    /// Рисует в Scene отрезок, по которому тележка может ездить.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (cart == null) return;

        Vector3 a = Application.isPlaying
            ? axis
            : Vector3.ProjectOnPlane(cart.forward, Vector3.up).normalized;
        Vector3 origin = Application.isPlaying ? cart.position - a * offset : cart.position;

        Vector3 from = origin + a * minOffset;
        Vector3 to = origin + a * maxOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(from, 0.15f);
        Gizmos.DrawWireSphere(to, 0.15f);
    }
}
