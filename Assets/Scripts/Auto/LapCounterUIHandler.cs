using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LapCounterUIHandler : MonoBehaviour
{
    TMP_Text lapText;

    private void Awake()
    {
        lapText = GetComponent<TMP_Text>();
    }

    public void SetLapText(string text)
    {
        lapText.text = text;
    }
}