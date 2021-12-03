using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleKeyboard : MonoBehaviour
{
    public GameObject keyboard;

    private void OnEnable()
    {
        keyboard.SetActive(false);
    }

    private void OnDisable()
    {
        keyboard.SetActive(false);
    }

    public void Toggle_Keyboard()
    {
        keyboard.SetActive(!keyboard.activeInHierarchy);
    }
}
