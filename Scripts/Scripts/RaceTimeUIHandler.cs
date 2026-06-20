using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceUITimeHandler : MonoBehaviour
{
    private TMP_Text timeText;

    private float lastRaceTimeUpdate = -1f;

    private void Awake()
    {
        timeText = GetComponent<TMP_Text>();

        if (timeText == null)
        {
            Debug.LogError("No se encontró un componente TMP_Text.");
        }
    }

    private void Start()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager.instance es NULL.");
            return;
        }

        StartCoroutine(UpdateTimeCO());
    }

    IEnumerator UpdateTimeCO()
    {
        while (true)
        {
            float raceTime = GameManager.instance.GetRaceTime();

            if (lastRaceTimeUpdate != raceTime)
            {
                int minutes = Mathf.FloorToInt(raceTime / 60);
                int seconds = Mathf.FloorToInt(raceTime % 60);

                timeText.text = $"{minutes:00}:{seconds:00}";

                lastRaceTimeUpdate = raceTime;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}