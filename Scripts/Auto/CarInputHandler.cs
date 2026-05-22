using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour
{
    [Header("Jugador")]
    public int playerNumber = 1;

    [Header("UI Input")]
    public bool isUIInput = false;

    // Input actual
    private Vector2 inputVector = Vector2.zero;

    // Componentes
    private TopDownCarController topDownCarController;

    // Input System
    private CarControls carControls;

    private void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();

        // Crear instancia del sistema de controles
        carControls = new CarControls();
    }

    private void OnEnable()
    {
        carControls.Enable();
    }

    private void OnDisable()
    {
        carControls.Disable();
    }

    private void Update()
    {
        // Obtener movimiento
        inputVector = carControls.Gameplay.Move.ReadValue<Vector2>();

        // Enviar al controlador del auto
        topDownCarController.SetInputVector(inputVector);
    }

    // Para UI o input externo
    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }
}