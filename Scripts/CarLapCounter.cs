using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

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

    //Eventos
    public event Action<CarLapCounter> OnPassCheckpoint;

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

    IEnumerator ShowPositionCO(float delayUntilHidePosition)
    {
        hideUIDelayTime += delayUntilHidePosition;

        carPositionText.text = carPosition.ToString();

        carPositionText.gameObject.SetActive(true);

        if(!isHideRoutineRunning)
        {
            isHideRoutineRunning = true;

            yield return new WaitForSeconds(hideUIDelayTime);
            
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

                //Guarda el tiempo en el checkpoint.
                timeAtLastPassedCheckPoint = Time.time;

                if (checkPoint.isFinishLine)
                {
                    passedCheckPointNumber = 0;
                    lapsCompleted++;

                    if (lapsCompleted >= lapsToComplete)
                        isRaceCompleted = true;
                }

                //Invocar el evento de checkpoint pasado.
                OnPassCheckpoint?.Invoke(this);

                //Ahora muestra la posición de los autos como calculado.
                if (isRaceCompleted)
                    StartCoroutine(ShowPositionCO(100));
                else StartCoroutine(ShowPositionCO(1.5f));
            }
        }
    }
}