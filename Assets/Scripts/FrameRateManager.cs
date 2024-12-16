using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    void Start()
    {
#if UNITY_WEBGL
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
#endif
    }
}
