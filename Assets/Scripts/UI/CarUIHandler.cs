using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class CarUIHandler : MonoBehaviour
{
    [Header("Detalles del auto")]
    public Image carImage;
    
    //Otros componentes
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

    //Eventos
    public void OnCarExitAnimationCompleted()
    {
        Destroy(gameObject);
    }
}