using UnityEngine;

public class PerformanceManager : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 300;
    }
}
