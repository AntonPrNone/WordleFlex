using UnityEngine;

public class AndroidFPSLimiter : MonoBehaviour
{
    public RectTransform OpenKeyboardAndroid;
    void Start()
    {
    #if UNITY_ANDROID
            Application.targetFrameRate = 120; // ������������� 120 FPS ��� Android
            QualitySettings.vSyncCount = 0; // ��������� VSync ��� Android
            OpenKeyboardAndroid.gameObject.SetActive(true);
    #endif
    }
}
