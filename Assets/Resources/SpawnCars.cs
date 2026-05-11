using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnCars : MonoBehaviour
{
    int numberOfCarsSpawned = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        //Asegurate que los spawn points estén ordenados por nombre.
        spawnPoints = spawnPoints.ToList().OrderBy(s => s.name).ToArray();

        //Cargar el data del auto
        CarData[] carDatas = Resources.LoadAll<CarData>("CarData/");

        //Información del conductor
        List<DriverInfo> driverInfoList = new List<DriverInfo>(GameManager.instance.GetDriverList());

        //Ordena a los conductores dependiendo de su última posición.
        driverInfoList = driverInfoList.OrderBy(s => s.lastRacePosition).ToList();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i].transform;

            if (driverInfoList.Count == 0)
                return;

            DriverInfo driverInfo = driverInfoList[0];

            int selectedCarID = driverInfo.carUniqueID;

            //Encuentra el auto seleccionado.
            foreach (CarData cardata in carDatas)
            {
                //Encontramos el data del auto para el jugador.
                if (cardata.CarUniqueID == selectedCarID)
                {
                    //Ahora lo spawneamos en el spawnpoint.
                    GameObject car = Instantiate(cardata.CarPrefab, spawnPoint.position, spawnPoint.rotation);

                    car.name = driverInfo.name;

                    car.GetComponent<CarInputHandler>().playerNumber = driverInfo.playerNumber;

                    if (driverInfo.isAI)
                    {
                        car.GetComponent<CarInputHandler>().enabled = false;
                        car.tag = "AI";
                    }
                    else
                    {
                        car.GetComponent<CarAIHandler>().enabled = false;
                        car.GetComponent<AStarLite>().enabled = false;
                        car.tag = "Player";
                    }

                    numberOfCarsSpawned++;

                    break;
                }
            }
            
            //Borrar al conductor spawneado.
            driverInfoList.Remove(driverInfo);
        }

    }
    public int GetNumberOfCarsSpawned()
    {
        return numberOfCarsSpawned;
    }
}
