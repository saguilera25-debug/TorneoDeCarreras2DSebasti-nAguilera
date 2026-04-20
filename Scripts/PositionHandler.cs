using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PositionHandler : MonoBehaviour
{
    public List<CarLapCounter> carLapCounters = new List<CarLapCounter>();

    void Start()
    {
        //Obtiene todos los contadores de vueltas en la escena.
        CarLapCounter[] carLapCounterArray = FindObjectsByType<CarLapCounter>(FindObjectsSortMode.None);

        //Guarda los contadores de vueltas en una lista.
        carLapCounters = carLapCounterArray.ToList<CarLapCounter>();

        //Conectar el evento del punto de control superado
        foreach (CarLapCounter lapCounters in carLapCounters)
            lapCounters.OnPassCheckpoint += OnPassCheckpoint;
    }


    void OnPassCheckpoint(CarLapCounter carLapCounter)
    {
        Debug.Log($"Event: Auto {carLapCounter.gameObject.name} pasó un checkpoint");
    }
}