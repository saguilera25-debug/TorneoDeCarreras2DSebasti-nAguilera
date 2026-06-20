using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Este script se encarga de manejar la entrada del jugador para controlar el auto. Toma la entrada del teclado o del gamepad, y se la pasa al controlador del auto para que pueda moverlo en consecuencia. También tiene una función pública para recibir entrada desde la interfaz de usuario, para que el jugador pueda controlar el auto desde la interfaz si lo desea.

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;
    public bool isUIInput = false;

    Vector2 inputVector = Vector2.zero;

    //Componentes
    TopDownCarController topDownCarController;

    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
    }

    void Start()
    {

    }

    void Update()
    {
        if (isUIInput)
        {

        }
        else
        {
            inputVector = Vector2.zero;

            switch (playerNumber)
            {
                case 1:
                    //Obtén input del sistema de inputs de Unity. 
                    inputVector.x = Input.GetAxis("Horizontal_P1");
                    inputVector.y = Input.GetAxis("Vertical_P1");
                    break;

                case 2:
                    //Obtén input del sistema de inputs de Unity.
                    inputVector.x = Input.GetAxis("Horizontal_P2");
                    inputVector.y = Input.GetAxis("Vertical_P2");
                    break;

                case 3:
                    //Obtén input del sistema de inputs de Unity.
                    inputVector.x = Input.GetAxis("Horizontal_P3");
                    inputVector.y = Input.GetAxis("Vertical_P3");
                    break;

                case 4:
                    //Obtén input del sistema de inputs de Unity.
                    inputVector.x = Input.GetAxis("Horizontal_P4");
                    inputVector.y = Input.GetAxis("Vertical_P4");
                    break;
            }

            //Envia el input al controlador de autos.
            topDownCarController.SetInputVector(inputVector);
        }
    }

    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }

}