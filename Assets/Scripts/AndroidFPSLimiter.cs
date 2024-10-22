using UnityEngine;

public class AndroidFPSLimiter : MonoBehaviour
{
    public RectTransform OpenKeyboardAndroid;
    void Start()
    {
    #if UNITY_ANDROID
            Application.targetFrameRate = 120; // Устанавливаем 120 FPS для Android
            QualitySettings.vSyncCount = 0; // Отключаем VSync для Android
            OpenKeyboardAndroid.gameObject.SetActive(true);
    #endif
    }
}
