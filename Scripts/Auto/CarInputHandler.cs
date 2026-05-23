using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour
{
    [Header("Jugador")]
    public int playerNumber = 1;

    [Header("UI Input")]
    public bool isUIInput = false;

    //Input actual
    private Vector2 inputVector = Vector2.zero;

    //Componentes
    private TopDownCarController topDownCarController;

    //Input System
    private CarControls carControls;

    private void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();

        if (topDownCarController == null)
            Debug.LogError("No se encontró TopDownCarController.");

        //Crear instancia de controles
        carControls = new CarControls();
    }

    private void OnEnable()
    {
        carControls.Enable();

        //Activar Gameplay explícitamente
        carControls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        carControls.Gameplay.Disable();

        carControls.Disable();
    }

    private void Update()
    {
        if (topDownCarController == null)
            return;

        //Leer movimiento
        inputVector = carControls.Gameplay.Move.ReadValue<Vector2>();

        Debug.Log("INPUT: " + inputVector);

        //Enviar input al auto
        topDownCarController.SetInputVector(inputVector);
    }

    //Para UI o input externo
    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;

        if (topDownCarController != null)
            topDownCarController.SetInputVector(inputVector);
    }
}