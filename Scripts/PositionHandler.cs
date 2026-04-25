using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PositionHandler : MonoBehaviour
{
    //Otros componentes
    LeaderboardUIHandler leaderboardUIHandler;

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

        //Obtén el controlador de la interfaz de usuario de la tabla de clasificación
        leaderboardUIHandler = FindFirstObjectByType<LeaderboardUIHandler>();

    }


    void OnPassCheckpoint(CarLapCounter carLapCounter)
    {
        //Primero, ordena la posición de los coches según la cantidad de puntos de control que hayan superado; cuantos más, mejor.
        carLapCounters = carLapCounters.OrderByDescending(s => s.GetNumberOfCheckpointsPassed()).ThenBy(s => s.GetTimeAtLastCheckPoint()).ToList();

        //Consigue la posición de los autos.
        int carPosition = carLapCounters.IndexOf(carLapCounter) + 1;

        //Decirle al contador de vueltas en que posición se encuentra el auto.
        carLapCounter.SetCarPosition(carPosition);

        //Pídele al encargado de la clasificación que actualice la lista.
        leaderboardUIHandler.UpdateList(carLapCounters);
        Debug.Log($"Event: Auto {carLapCounter.gameObject.name} pasó un checkpoint");
    }
}