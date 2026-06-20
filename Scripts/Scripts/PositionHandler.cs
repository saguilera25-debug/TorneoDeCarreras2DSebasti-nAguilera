using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Este script se encarga de manejar la posición de los autos en la carrera. Escucha los eventos de los contadores de vueltas de los autos para saber cuándo un auto ha pasado por un checkpoint, y actualiza la posición de los autos en consecuencia. También se encarga de comunicar a otros scripts que necesitan saber la posición de los autos, como el controlador del leaderboard para actualizar la interfaz del leaderboard, o el contador de vueltas para mostrar la posición actual del auto en la interfaz del juego.
public class PositionHandler : MonoBehaviour
{
    // Otros componentes que necesitan ser referenciados para actualizar la interfaz del leaderboard.
    LeaderboardUIHandler leaderboardUIHandler;

    // Lista de contadores de vueltas de los autos en la escena. Se llena automáticamente al inicio.
    public List<CarLapCounter> carLapCounters = new List<CarLapCounter>();

    private void Awake()
    {
        // Obtener todos los contadores de vueltas en la escena y convertirlos en una lista.
        CarLapCounter[] carLapCounterArray = FindObjectsByType<CarLapCounter>(FindObjectsSortMode.None);

        // Guardar los contadores en una lista para facilitar su manejo.
        carLapCounters = carLapCounterArray.ToList<CarLapCounter>();

        //Conectar eventos de checkpoints pasados para cada auto.
        foreach (CarLapCounter lapCounters in carLapCounters)
            lapCounters.OnPassCheckpoint += OnPassCheckpoint;

        //Obtener controlador del leaderboard para actualizar la interfaz cuando cambien las posiciones.
        leaderboardUIHandler = FindFirstObjectByType<LeaderboardUIHandler>();
    }

    void Start()
    {
        //Pedir al leaderboard que actualice la lista al inicio para mostrar posiciones correctas desde el principio.
        leaderboardUIHandler.UpdateList(carLapCounters);
    }

    void OnPassCheckpoint(CarLapCounter carLapCounter)
    {
        //Ordena la posición de los autos basado en el número de vuelta, luego en el número de checkpoint, y finalmente en la distancia al siguiente checkpoint. Después de ordenar, convierte el resultado en una lista para facilitar su manejo. Más vueltas, más checkpoints pasados, y menor tiempo en el último checkpoint, significan una mejor posición.
        carLapCounters = carLapCounters.OrderByDescending(s => s.GetNumberOfCheckpointsPassed()).ThenBy(s => s.GetTimeAtLastCheckPoint()).ToList();

        //Obtén la posición del auto que pasó el checkpoint.
        int carPosition = carLapCounters.IndexOf(carLapCounter) + 1;

        //Decirle al contador de vueltas en cuestión su posición actual, para que pueda mostrarla en la interfaz del auto.
        carLapCounter.SetCarPosition(carPosition);

        if (carLapCounter.IsRaceCompleted())
        {
            //Establece la última posición del jugador que terminó la carrera, para que pueda mostrarla en la interfaz del auto.
            int playerNumber = carLapCounter.GetComponent<CarInputHandler>().playerNumber;
            GameManager.instance.SetDriversLastRacePosition(playerNumber, carPosition);

            //Agregar puntos al campeonato basado en la posición del auto que terminó la carrera. Más puntos para mejores posiciones.
            int championshipPointsAwarded = FindAnyObjectByType<SpawnCars>().GetNumberOfCarsSpawned() - carPosition; // 1st place gets 3 points, 2nd place gets 2 points, 3rd place gets 1 point, and 4th place gets 0 points.
            GameManager.instance.AddPointsToChampionship(playerNumber, championshipPointsAwarded);
        }

        //Preguntarle al leaderboard que actualice la lista para mostrar las posiciones correctas en la interfaz del leaderboard.
        if (leaderboardUIHandler != null)
            leaderboardUIHandler.UpdateList(carLapCounters);
    }
}