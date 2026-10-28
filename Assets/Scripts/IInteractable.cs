using UnityEngine;

/// <summary>
/// Контракт для любого объекта, которым игрок может взаимодействовать.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Текст подсказки.
    /// </summary>
    string Prompt { get; }

    /// <summary>
    /// Можно ли взаимодействовать прямо сейчас.
    /// </summary>
    bool CanInteract { get; }

    /// <summary>
    /// Вызывается, когда игрок нажал на кнопку взаимодействия
    /// </summary>
    /// <param name="interaactor"></param>
    void Interact(GameObject interaactor);
}
