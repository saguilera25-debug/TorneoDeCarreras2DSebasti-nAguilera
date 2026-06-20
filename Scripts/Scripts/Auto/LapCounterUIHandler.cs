using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Este script se encarga de manejar la interfaz de usuario que muestra el número de vuelta actual del auto. Tiene una función pública para actualizar el texto de la interfaz con el número de vuelta actual, y también tiene una referencia a un componente TMP_Text para mostrar el texto en la interfaz.
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