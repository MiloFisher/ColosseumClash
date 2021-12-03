using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    public GameObject shiftKeyboard;
    public InputField input;

    private void OnEnable()
    {
        shiftKeyboard.SetActive(false);
    }

    public void TypeCharacter(string character)
    {
        switch (character)
        {
            case "shift":
                ToggleShift();
                break;
            case "return":
                gameObject.SetActive(false);
                break;
            case "space":
                input.text += " ";
                break;
            case "delete":
                if(input.text.Length > 0)
                    input.text = input.text.Substring(0, input.text.Length - 1);
                break;
            default:
                input.text += character;
                break;
        }
    }

    private void ToggleShift()
    {
        shiftKeyboard.SetActive(!shiftKeyboard.activeInHierarchy);
    }
}
