using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDisplay : MonoBehaviour
{
    public static void ExitGame()
    {
#if UNITY_EDITOR
        // Если игра запущена в редакторе
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Если игра запущена в билде
        Application.Quit();
#endif

        Debug.Log("Игра завершена.");
    }

}
