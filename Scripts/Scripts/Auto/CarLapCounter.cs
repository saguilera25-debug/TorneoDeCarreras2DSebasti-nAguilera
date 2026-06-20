using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

// Este script se encarga de contar los checkpoints y vueltas que ha pasado el auto, para determinar su posición en la carrera. También se encarga de mostrar la posición actual del auto en la interfaz del juego cuando pase por la línea de meta, y de mostrar la posición final del auto permanentemente cuando termine la carrera.

public class CarLapCounter : MonoBehaviour
{
    int passedCheckPointNumber = 0;
    float timeAtLastPassedCheckPoint = 0;

    int numberOfPassedCheckpoints = 0;

    int lapsCompleted = 0;
    const int lapsToComplete = 2;

    bool isRaceCompleted = false;

    int carPosition = 0;

    bool isHideRoutineRunning = false;
    float hideUIDelayTime;

    public TMP_Text carPositionText;

    //Otros componentes que necesitan ser referenciados para actualizar la interfaz del leaderboard.
    LapCounterUIHandler lapCounterUIHandler;

    //Eventos para comunicar a otros scripts que el auto ha pasado un checkpoint, para que puedan actualizar la posición de los autos en la interfaz del juego.
    public event Action<CarLapCounter> OnPassCheckpoint;

    void Start()
    {
        if (CompareTag("Player"))
        {
            lapCounterUIHandler = FindAnyObjectByType<LapCounterUIHandler>();
            lapCounterUIHandler.SetLapText($"LAP {lapsCompleted + 1}/{lapsToComplete}");

        }
    }

    public void SetCarPosition(int position)
    {
        carPosition = position;
    }

    public int GetNumberOfCheckpointsPassed()
    {
        return numberOfPassedCheckpoints;
    }
    public float GetTimeAtLastCheckPoint()
    {
        return timeAtLastPassedCheckPoint;
    }

    public bool IsRaceCompleted()
    {
        return isRaceCompleted;
    }

    IEnumerator ShowPositionCO(float delayUntilHidePosition)
    {
        hideUIDelayTime = delayUntilHidePosition;

        carPositionText.text = carPosition.ToString();

        carPositionText.gameObject.SetActive(true);
        
        if (!isHideRoutineRunning)
        {
            isHideRoutineRunning = true;

            yield return new WaitForSeconds(delayUntilHidePosition);
            carPositionText.gameObject.SetActive(false);

            isHideRoutineRunning = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("CheckPoint"))
        {
            //Cuando el auto haya completado las carreras no necesitamos revisar checkpoints y vueltas.
            if (isRaceCompleted)
                return;

            CheckPoint checkPoint = collider2D.GetComponent<CheckPoint>();

            //Asegurate que el auto esté pasando por los puntos de partida en el orden correcto.
            if (passedCheckPointNumber + 1 == checkPoint.checkPointNumber)
            {
                passedCheckPointNumber = checkPoint.checkPointNumber;

                numberOfPassedCheckpoints++;

                //Guarda el tiempo en el checkpoint. Esto se usará para comparar el progreso de los autos en la carrera, para mostrar la posición actual de cada auto en la interfaz del juego.
                timeAtLastPassedCheckPoint = Time.time;

                if (checkPoint.isFinishLine)
                {
                    passedCheckPointNumber = 0;
                    lapsCompleted++;

                    if (lapsCompleted >= lapsToComplete)
                        isRaceCompleted = true;

                    if (!isRaceCompleted && lapCounterUIHandler != null)
                        lapCounterUIHandler.SetLapText($"LAP {lapsCompleted + 1}/{lapsToComplete}");
                }

                //Invocar el evento de checkpoint pasado. Esto le permite a otros scripts que estén escuchando este evento actualizar la posición de los autos en la interfaz del juego, basándose en el progreso de cada auto en la carrera.
                OnPassCheckpoint?.Invoke(this);

                //Ahora muestra la posición del auto como si se hubiera calculado pero solo hazlo cuando el auto pase por la línea de meta. Esto es para evitar mostrar la posición del auto cada vez que pase por un checkpoint, lo que podría saturar la interfaz del juego. Mostrar la posición del auto solo cuando pase por la línea de meta es suficiente para mantener a los jugadores informados sobre su posición en la carrera sin saturar la interfaz.
                if (isRaceCompleted)
                {
                    StartCoroutine(ShowPositionCO(100));

                    if (CompareTag("Player"))
                    {
                        GameManager.instance.OnRaceCompleted();

                        GetComponent<CarInputHandler>().enabled = false;
                        GetComponent<CarAIHandler>().enabled = true;
                        GetComponent<AStarLite>().enabled = true;
                    }
                }
                else if (checkPoint.isFinishLine) StartCoroutine(ShowPositionCO(1.5f));
            }
        }
    }
}