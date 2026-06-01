using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PositionHandler : MonoBehaviour
{
    // Otros componentes
    private LeaderboardUIHandler leaderboardUIHandler;

    // Lista de contadores de vueltas
    public List<CarLapCounter> carLapCounters = new List<CarLapCounter>();

    private void Start()
    {
        // Obtener todos los contadores de vueltas en la escena
        CarLapCounter[] carLapCounterArray = FindObjectsByType<CarLapCounter>(FindObjectsSortMode.None);

        // Verificar si existen contadores
        if (carLapCounterArray.Length == 0)
        {
            Debug.LogWarning("No se encontraron CarLapCounter en la escena.");

            return;
        }

        // Guardar los contadores en una lista
        carLapCounters = carLapCounterArray.ToList();

        // Conectar eventos de checkpoints
        foreach (CarLapCounter lapCounter in carLapCounters)
        {
            // Verificar referencia válida
            if (lapCounter == null)
                continue;

            lapCounter.OnPassCheckpoint += OnPassCheckpoint;
        }

        // Obtener controlador del leaderboard
        leaderboardUIHandler = FindFirstObjectByType<LeaderboardUIHandler>();

        // Verificar existencia del leaderboard
        if (leaderboardUIHandler == null)
        {
            Debug.LogWarning("No se encontró LeaderboardUIHandler en la escena.");
        }

        // Ordenar posiciones iniciales
        UpdateCarPositions();

        Debug.Log($"PositionHandler inicializado con {carLapCounters.Count} autos.");
    }

    //Se ejecuta cuando un auto pasa un checkpoint.
    private void OnPassCheckpoint(CarLapCounter carLapCounter)
    {
        // Verificar referencia válida
        if (carLapCounter == null)
        {
            Debug.LogWarning("OnPassCheckpoint recibió un CarLapCounter NULL.");

            return;
        }

        // Actualizar posiciones
        UpdateCarPositions();

        Debug.Log($"Event: Auto {carLapCounter.gameObject.name} pasó un checkpoint.");
    }

    //Ordena los autos y actualiza posiciones.
    private void UpdateCarPositions()
    {
        // Eliminar referencias nulas
        carLapCounters = carLapCounters.Where(car => car != null).ToList();

        // Ordenar:
        // 1. Más checkpoints superados = mejor posición
        // 2. Menor tiempo en último checkpoint = mejor posición
        carLapCounters = carLapCounters.OrderByDescending(car => car.GetNumberOfCheckpointsPassed()).ThenBy(car => car.GetTimeAtLastCheckPoint()).ToList();

        // Actualizar posición de cada auto
        for (int i = 0; i < carLapCounters.Count; i++)
        {
            if (carLapCounters[i] == null)
                continue;

            // Posición real = índice + 1
            int carPosition = i + 1;

            // Asignar posición
            carLapCounters[i].SetCarPosition(carPosition);
        }

        // Actualizar interfaz del leaderboard
        if (leaderboardUIHandler != null)
        {
            leaderboardUIHandler.UpdateList(carLapCounters);
        }
    }

    // Desconecta eventos al destruir el objeto.
    private void OnDestroy()
    {
        foreach (CarLapCounter lapCounter in carLapCounters)
        {
            if (lapCounter == null)
                continue;

            lapCounter.OnPassCheckpoint -= OnPassCheckpoint;
        }
    }
}