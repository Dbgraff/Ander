using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    [Tooltip("На сколько предмет может выходить за край области")]
    [SerializeField] private Vector2 edgeOverflow = new Vector2(50f, 50f);

    private RectTransform rect;
    private RectTransform parentRect;
    private Vector2 grabOffset;

    private void Awake()
    {
        rect = (RectTransform)transform;
        parentRect = rect.parent as RectTransform;
    }

    /// <summary>
    /// Поднимает предмет поверх остальных, как только на него нажали
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// Запоминает, за какую точку предмета взялись, чтобы не прыгал к курсору
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out Vector2 local))
        {
            grabOffset = rect.anchoredPosition - local;
        }
    }

    /// <summary>
    /// Двигает предмет за курсором
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out Vector2 local))
            return;

        Vector2 pos = local + grabOffset;
        Vector2 halfArea = parentRect.rect.size * 0.5f + edgeOverflow;
        pos.x = Mathf.Clamp(pos.x, -halfArea.x, halfArea.x);
        pos.y = Mathf.Clamp(pos.y, -halfArea.y, halfArea.y);
        rect.anchoredPosition = pos;
    }
}
