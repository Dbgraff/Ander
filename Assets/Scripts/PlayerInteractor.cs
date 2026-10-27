using UnityEngine;

/// <summary>
/// Ищет ближайший интерактивный объект рядом с игроком
/// и вызывает его по нажатию кнопки.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private Vector3 originOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private LayerMask interactMask = -0;

    public KeyCode InteractKey => interactKey;

    private readonly Collider[] hits = new Collider[5];

    /// <summary>
    /// Блокирует взаимодействие
    /// </summary>
    public bool InteractionLocked { get; set; }

    /// <summary>
    /// Ближайший доступный объект или null. Для UI подсказки.
    /// </summary>
    public IInteractable Current { get; private set; }

    private void Update()
    {
        if (InteractionLocked)
        {
            Current = null;
            return;
        }

        Current = FindClosest();

        if (Current != null && Input.GetKeyDown(interactKey))
            Current.Interact(gameObject);
    }

    /// <summary>
    /// Находит ближайший объект с IInteractable в радиусе.
    /// </summary>
    /// <returns>Ближайший доступный объект или null.</returns>
    private IInteractable FindClosest()
    {
        Vector3 origin = transform.position + originOffset;
        int count = Physics.OverlapSphereNonAlloc(origin, interactRadius, hits, interactMask,
                QueryTriggerInteraction.Collide);

        IInteractable best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            IInteractable candidate = hits[i].GetComponentInParent<IInteractable>();
            if (candidate == null || !candidate.CanInteract) continue;

            float dist = Vector3.Distance(origin, hits[i].ClosestPoint(origin));
            if (dist < bestDist)
            {
                bestDist = dist;
                best = candidate;
            }
        }

        return best;
    }
}
