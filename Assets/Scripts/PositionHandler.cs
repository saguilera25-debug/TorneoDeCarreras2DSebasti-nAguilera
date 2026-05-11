using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PositionHandler : MonoBehaviour
{
    // Otros componentes
    LeaderboardUIHandler leaderboardUIHandler;

    public List<CarLapCounter> carLapCounters = new List<CarLapCounter>();

    private void Awake()
    {

    }

    void Start()
    {
        // Obtiene todos los contadores de vueltas en la escena.
        CarLapCounter[] carLapCounterArray = FindObjectsByType<CarLapCounter>(FindObjectsSortMode.None);

        // Guarda los contadores de vueltas en una lista.
        carLapCounters = carLapCounterArray.ToList();

        // Conectar el evento del punto de control superado
        foreach (CarLapCounter lapCounter in carLapCounters)
        {
            lapCounter.OnPassCheckpoint += OnPassCheckpoint;
        }

        // Obtén el controlador de la interfaz de usuario de la tabla de clasificación
        leaderboardUIHandler = FindFirstObjectByType<LeaderboardUIHandler>();

        // Pedir que el controlador de la interfaz de usuario de la tabla de clasificación actualice la lista.
        if (leaderboardUIHandler != null)
        {
            leaderboardUIHandler.UpdateList(carLapCounters);
        }
    }

    void OnPassCheckpoint(CarLapCounter carLapCounter)
    {
        // Ordena la posición de los coches según checkpoints y tiempo.
        carLapCounters = carLapCounters
            .OrderByDescending(s => s.GetNumberOfCheckpointsPassed())
            .ThenBy(s => s.GetTimeAtLastCheckPoint())
            .ToList();

        // Consigue la posición del auto.
        int carPosition = carLapCounters.IndexOf(carLapCounter) + 1;

        // Decirle al contador de vueltas en qué posición se encuentra el auto.
        carLapCounter.SetCarPosition(carPosition);

        // Verifica si la carrera terminó
        if (carLapCounter.IsRaceCompleted())
        {
            // Establece la última posición del jugador.
            CarInputHandler inputHandler = carLapCounter.GetComponent<CarInputHandler>();

            if (inputHandler != null)
            {
                int playerNumber = inputHandler.playerNumber;

                GameManager.instance.SetDriversLastRacePosition(playerNumber, carPosition);

                // Agrega puntos al campeonato.
                SpawnCars spawnCars = FindFirstObjectByType<SpawnCars>();

                if (spawnCars != null)
                {
                    int championshipPointAwarded =
                        spawnCars.GetNumberOfCarsSpawned() - carPosition;

                    GameManager.instance.AddPointsToChampionship(
                        playerNumber,
                        championshipPointAwarded
                    );
                }
            }
        }

        // Pídele al encargado de la clasificación que actualice la lista.
        if (leaderboardUIHandler != null)
        {
            leaderboardUIHandler.UpdateList(carLapCounters);
        }
    }
}