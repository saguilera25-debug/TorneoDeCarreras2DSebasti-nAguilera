using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;
    public bool isUIInput = false;

    private Vector2 inputVector = Vector2.zero;

    // Componentes
    private TopDownCarController topDownCarController;

    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
    }

    void Update()
    {
        // Si NO usa UI, leer teclado/gamepad
        if (!isUIInput)
        {
            inputVector = Vector2.zero;

            switch (playerNumber)
            {
                case 1:
                    inputVector.x = Input.GetAxis("Horizontal_P1");
                    inputVector.y = Input.GetAxis("Vertical_P1");
                    break;

                case 2:
                    inputVector.x = Input.GetAxis("Horizontal_P2");
                    inputVector.y = Input.GetAxis("Vertical_P2");
                    break;

                case 3:
                    inputVector.x = Input.GetAxis("Horizontal_P3");
                    inputVector.y = Input.GetAxis("Vertical_P3");
                    break;

                case 4:
                    inputVector.x = Input.GetAxis("Horizontal_P4");
                    inputVector.y = Input.GetAxis("Vertical_P4");
                    break;
            }
        }

        // Siempre enviar el input al auto
        topDownCarController.SetInputVector(inputVector);
    }

    // Método usado por UI móvil o joystick virtual
    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }
}