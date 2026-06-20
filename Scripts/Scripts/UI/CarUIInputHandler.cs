using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este script se encarga de manejar la entrada del jugador a través de la interfaz de usuario, enviando las entradas del jugador al CarInputHandler para que el auto pueda responder a las entradas del jugador. Tiene funciones públicas para manejar las entradas de acelerar, frenar, girar a la izquierda y girar a la derecha, y también tiene una función pública para manejar la liberación de las entradas de acelerar y frenar, para que el auto pueda dejar de acelerar o frenar cuando el jugador suelta los botones correspondientes en la interfaz de usuario.
public class CarUIInputHandler : MonoBehaviour
{
    private CarInputHandler playerCarInputHandler;

    private Vector2 inputVector = Vector2.zero;

    private void Awake()
    {
        CarInputHandler[] carInputHandlers = FindObjectsByType<CarInputHandler>(FindObjectsSortMode.None);

        foreach (CarInputHandler carInputHandler in carInputHandlers)
        {
            if (carInputHandler.isUIInput)
            {
                playerCarInputHandler = carInputHandler;
                break;
            }
        }
    }

    // Start se utiliza antes de llamar a la actualización del primer frame. En este caso, no necesitamos hacer nada en Start, pero lo dejamos aquí por si acaso necesitamos agregar algo en el futuro.
    private void Start()
    {

    }

    public void OnAcceleratePress()
    {
        inputVector.y = 1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnBrakePress()
    {
        inputVector.y = -1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnAccelerateBrakeRelease()
    {
        inputVector.y = 0.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnSteerLeftPress()
    {
        inputVector.x = -1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnSteerRightPress()
    {
        inputVector.x = 1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnSteerRelease()
    {
        inputVector.x = 0.0f;
        playerCarInputHandler.SetInput(inputVector);
    }
}