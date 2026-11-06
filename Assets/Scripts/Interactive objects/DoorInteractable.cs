using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Подсказки")]
    [SerializeField] private string openPrompt = "Открыть";
    [SerializeField] private string closePrompt = "Закрыть";
    [SerializeField] private string lockedPrompt = "Заперто";

    [Header("Анимация")]
    [Tooltip("Аниматор двери")]
    [SerializeField] private Animator animator;
    [Tooltip("Имя парамерта в Animator")]
    [SerializeField] private string openParameter = "Open";
    [Tooltip("Может ли дверь закрываться обратно")]
    [SerializeField] private bool closeable = false;
    [Tooltip("Длина анимации")]
    [SerializeField] private float actionDuration = 1f;

    [SerializeField] private bool locked = false;

    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;
    [Tooltip("Попытка игрока открыть запертую дверь")]
    [SerializeField] private UnityEvent onLockedAttempt;

    private bool isOpen;
    private bool busy;

    public bool IsOpen => isOpen;

    public bool IsLocked => locked;

    public string Prompt => locked ? lockedPrompt : (isOpen ? closePrompt : openPrompt);
    public bool CanInteract => !busy && (!isOpen || closeable);

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
            Debug.LogWarning("Аниматор двери не найден");
        }
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract) return;

        if (locked)
        {
            onLockedAttempt?.Invoke();
            return;
        }

        StartCoroutine(isOpen ? CloseRoutine() : OpenRoutine());
    }

    public void Unlock()
    {
        locked = false;
    }

    public void Lock()
    {
        locked = true;
    }

    private IEnumerator OpenRoutine()
    {
        busy = true;

        SetAnimator(true);

        yield return new WaitForSeconds(actionDuration);

        isOpen = true;
        busy = false;
        onOpened?.Invoke();
        
    }

    private IEnumerator CloseRoutine()
    {
        busy = true;

        SetAnimator(false);

        yield return new WaitForSeconds(actionDuration);

        isOpen = false;
        busy = false;
        onClosed?.Invoke();
    }

    private void SetAnimator(bool open)
    {
        if (animator == null || string.IsNullOrEmpty(openParameter)) return;

        if (closeable)
            animator.SetBool(openParameter, open);
        else
            animator.SetTrigger(openParameter);
    }
}
