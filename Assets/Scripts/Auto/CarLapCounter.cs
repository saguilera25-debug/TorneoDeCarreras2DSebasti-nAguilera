using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class CarLapCounter : MonoBehaviour
{
    public TMP_Text carPositionText;

    int passedCheckPointNumber = 0;
    float timeAtLastPassedCheckPoint = 0;

    int numberOfPassedCheckpoints = 0;

    int lapsCompleted = 0;
    const int lapsToComplete = 2;

    bool isRaceCompleted = false;

    int carPosition = 0;

    bool isHideRoutineRunning = false;

    // Otros componentes
    LapCounterUIHandler lapCounterUIHandler;

    // Eventos
    public event Action<CarLapCounter> OnPassCheckpoint;

    void Start()
    {
        if (CompareTag("Player"))
        {
            lapCounterUIHandler = FindFirstObjectByType<LapCounterUIHandler>();

            if (lapCounterUIHandler != null)
            {
                lapCounterUIHandler.SetLapText($"LAP {lapsCompleted + 1}/{lapsToComplete}");
            }
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
            // Cuando el auto completa la carrera
            if (isRaceCompleted)
                return;

            CheckPoint checkPoint = collider2D.GetComponent<CheckPoint>();

            // Verifica checkpoints en orden
            if (passedCheckPointNumber + 1 == checkPoint.checkPointNumber)
            {
                passedCheckPointNumber = checkPoint.checkPointNumber;

                numberOfPassedCheckpoints++;

                // Guarda el tiempo del checkpoint
                timeAtLastPassedCheckPoint = Time.time;

                if (checkPoint.isFinishLine)
                {
                    passedCheckPointNumber = 0;
                    lapsCompleted++;

                    if (lapsCompleted >= lapsToComplete)
                    {
                        isRaceCompleted = true;
                    }

                    if (!isRaceCompleted && lapCounterUIHandler != null)
                    {
                        lapCounterUIHandler.SetLapText($"LAP {lapsCompleted + 1}/{lapsToComplete}");
                    }
                }

                // Invoca evento
                OnPassCheckpoint?.Invoke(this);

                // Mostrar posición
                if (isRaceCompleted)
                {
                    StartCoroutine(ShowPositionCO(100f));
                }
                else
                {
                    StartCoroutine(ShowPositionCO(1.5f));
                }
            }
        }
    }
}