using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDisplay : MonoBehaviour
{
    public static void ExitGame()
    {
#if UNITY_EDITOR
        // ���� ���� �������� � ���������
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ���� ���� �������� � �����
        Application.Quit();
#endif

        Debug.Log("���� ���������.");
    }

}
