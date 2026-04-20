using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUIHandler : MonoBehaviour
{
    public GameObject leaderboardItemPrefab;

    SetLeaderboardItemInfo[] setLeaderboardItemInfo;

    void Awake()
    {
        VerticalLayoutGroup leaderboardLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();

        //Obtener todos los contadores de vueltas de auto en la escena.
        CarLapCounter[] carLapCounterArray = FindObjectsByType<CarLapCounter>(FindObjectsSortMode.None);

        //Ubice el array
        setLeaderboardItemInfo = new SetLeaderboardItemInfo[carLapCounterArray.Length];

        //Crear el item de leaderboard
        for (int i = 0; i < carLapCounterArray. Length; i++)
        {
            //Ajusta la posición
            GameObject leaderboardInfoGameObject = Instantiate(leaderboardItemPrefab, leaderboardLayoutGroup.transform);

            setLeaderboardItemInfo[i] = leaderboardInfoGameObject.GetComponent<SetLeaderboardItemInfo>();

            setLeaderboardItemInfo[i].SetPositionText($"{i + 1}.");
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void UpdateList(List<CarLapCounter> lapCounters)
    {
        //Crear los items del Leaderboard.
        for (int i = 0; i <  lapCounters.Count; i++)
        {
            setLeaderboardItemInfo[i].SetDriverNameText(lapCounters[i].gameObject.name);
        }
    }
}