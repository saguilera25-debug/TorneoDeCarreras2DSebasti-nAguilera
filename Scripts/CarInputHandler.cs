using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;

    //Componentes
    TopDownCarController topDownCarController;

    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
    }

    void Start()
    {

    }

    //Update es llamada por cada frame y dependiente a los frames.
    void Update()
    {
        Vector2 inputVector = Vector2.zero;

        switch (playerNumber)
        {
            case 1:
                //Consigue el input del sistema de inputs de Unity.
                inputVector.x = Input.GetAxis("Horizontal_P1");
                inputVector.y = Input.GetAxis("Vertical_P1");
                break;

            case 2:
                //Consigue el input del sistema de inputs de Unity.
                inputVector.x = Input.GetAxis("Horizontal_P2");
                inputVector.y = Input.GetAxis("Vertical_P2");
                break;

            case 3:
                //Consigue el input del sistema de inputs de Unity.
                inputVector.x = Input.GetAxis("Horizontal_P3");
                inputVector.y = Input.GetAxis("Vertical_P3");
                break;

            case 4:
                //Consigue el input del sistema de inputs de Unity.
                inputVector.x = Input.GetAxis("Horizontal_P4");
                inputVector.y = Input.GetAxis("Vertical_P4");
                break;
        }

        //Envia el input al controlador de autos.
        topDownCarController.SetInputVector(inputVector);
    }
}
