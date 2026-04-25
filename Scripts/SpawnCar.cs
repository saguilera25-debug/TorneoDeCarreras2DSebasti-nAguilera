using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCars : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        //Cargar el data del auto.
        CarData[] carDatas = Resources.LoadAll<CarData>("CarData/");

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i].transform;

            int playerSelectedCarID = PlayerPrefs.GetInt($"P{i + 1}SelectedCarID");

            //Encontrar el prefab de los carros de los jugadores.
            foreach (CarData cardata in carDatas)
            {
                //Encontramos el data del auto del jugador.
                if (cardata.CarUniqueID == playerSelectedCarID)
                {
                    //Ahora lo spawneamos en el spawnpoint
                    GameObject playerCar = Instantiate(cardata.CarPrefab, spawnPoint.position, spawnPoint.rotation);

                    int playerNumber = i + 1;

                    playerCar.GetComponent<CarInputHandler>().playerNumber = i + 1;

                    if (PlayerPrefs.GetInt($"P{playerNumber}_IsAI") == 1)
                    {
                        playerCar.GetComponent<CarInputHandler>().enabled = false;
                        playerCar.tag = "AI";
                    }
                    else
                    {
                        playerCar.GetComponent<CarAIHandler>().enabled = false;
                        playerCar.GetComponent<AStarLite>().enabled = false;
                        playerCar.tag = "Player";
                    }

                    break;
                }
            }
        }
    }
}