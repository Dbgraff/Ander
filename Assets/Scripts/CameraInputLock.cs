using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Позволяет другим скриптам включать и выключать вращение камерой
/// </summary>
[RequireComponent(typeof(CinemachineInputAxisController))]
public class CameraInputLock : MonoBehaviour
{
    private CinemachineInputAxisController axisController;

    public static CameraInputLock Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetLocked(bool locked)
    {
        axisController.enabled = !locked;
    }
}
