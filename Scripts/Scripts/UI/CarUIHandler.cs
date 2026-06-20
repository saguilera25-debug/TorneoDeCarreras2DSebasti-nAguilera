using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

// Este script se encarga de manejar la interfaz del auto en la pantalla, mostrando el sprite del auto seleccionado por el jugador, y reproduciendo las animaciones de entrada y salida del auto en la interfaz cuando el auto aparece o desaparece en la carrera. También se encarga de destruir el objeto de la interfaz del auto cuando la animación de salida ha terminado, para limpiar la escena y evitar que haya objetos innecesarios en la escena después de que el auto haya desaparecido.
public class CarUIHandler : MonoBehaviour
{
    [Header("Detalles del auto")]
    public Image carImage;

    //Otros componentes que se necesitan para reproducir las animaciones de entrada y salida del auto en la interfaz.
    Animator animator = null;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {

    }

    public void SetupCar(CarData carData)
    {
        carImage.sprite = carData.CarUISprite;
    }

    public void StartCarEntranceAnimation(bool isAppearingOnRightSide)
    {
        if (isAppearingOnRightSide)
            animator.Play("UI De Auto Aparece Desde La Derecha");
        else animator.Play("UI De Auto Aparece Desde La Izquierda");
    }

    public void StartCarExitAnimation(bool isExitingOnRightSide)
    {
        if (isExitingOnRightSide)
            animator.Play("UI De Auto Desaparece A La Derecha");
        else animator.Play("UI De Auto Desaparece A La Izquierda");
    }

    //Eventos de animación para destruir el objeto de la interfaz del auto cuando la animación de salida ha terminado, para limpiar la escena y evitar que haya objetos innecesarios en la escena después de que el auto haya desaparecido.
    public void OnCarExitAnimationCompleted()
    {
        Destroy(gameObject);
    }
}