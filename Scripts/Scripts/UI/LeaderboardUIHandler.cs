using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Este script se encarga de manejar la interfaz del leaderboard en la pantalla, mostrando la posición de cada auto en la carrera. Tiene una función pública UpdateList que se llama cada vez que se necesita actualizar la lista del leaderboard, para actualizar la posición de cada auto en la interfaz del leaderboard según su posición actual en la carrera. También se encarga de crear los items del leaderboard al inicio de la escena, para tener los items listos para ser actualizados cuando sea necesario.
public class LeaderboardUIHandler : MonoBehaviour
{
    public GameObject leaderboardItemPrefab;

    SetLeaderboardItemInfo[] setLeaderboardItemInfo;

    void Awake()
    {
        VerticalLayoutGroup leaderboardLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();

        //Obtener todos los contadores de vueltas de auto en la escena. 
        CarLapCounter[] carLapCounterArray = FindObjectsByType<CarLapCounter>(FindObjectsSortMode.None);

        //Ubice el array de SetLeaderboardItemInfo al mismo tamaño que el array de contadores de vueltas de auto, para tener un item de leaderboard por cada auto en la carrera.
        setLeaderboardItemInfo = new SetLeaderboardItemInfo[carLapCounterArray.Length];

        //Crear el item de leaderboard para cada auto en la carrera, y asignar el texto de posición del item de leaderboard según la posición del auto en la carrera, para que el item del auto en primer lugar tenga el texto "1.", el item del auto en segundo lugar tenga el texto "2.", y así sucesivamente.
        for (int i = 0; i < carLapCounterArray.Length; i++)
        {
            //Ajusta la posición del item de leaderboard según su posición en la carrera, para que el item del auto en primer lugar esté en la parte superior del leaderboard, el item del auto en segundo lugar esté debajo del primer lugar, y así sucesivamente.
            GameObject leaderboardInfoGameObject = Instantiate(leaderboardItemPrefab, leaderboardLayoutGroup.transform);

            setLeaderboardItemInfo[i] = leaderboardInfoGameObject.GetComponent<SetLeaderboardItemInfo>();

            setLeaderboardItemInfo[i].SetPositionText($"{i + 1}.");
        }
    }

    void Start()
    {

    }

    public void UpdateList(List<CarLapCounter> lapCounters)
    {

        //Crear los items del Leaderboard. Actualizar el texto de cada item del leaderboard para mostrar el nombre del auto correspondiente a cada posición en la carrera, para que el item del auto en primer lugar muestre el nombre del auto que está en primer lugar, el item del auto en segundo lugar muestre el nombre del auto que está en segundo lugar, y así sucesivamente.
        for (int i = 0; i <  lapCounters.Count; i++)
        {
            setLeaderboardItemInfo[i].SetDriverNameText(lapCounters[i].gameObject.name);
        }
    }
}