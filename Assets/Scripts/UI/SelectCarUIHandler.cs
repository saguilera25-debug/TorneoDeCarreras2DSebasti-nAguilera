using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SelectCarUIHandler : MonoBehaviour
{
    [Header("Prefab de auto")]
    public GameObject carPrefab;

    [Header("Spawnear")]
    public Transform spawnOnTransform;

    bool isChangingCar = false;

    CarData[] carDatas;

    int selectedCarIndex = 0;

    //Otros componentes
    CarUIHandler carUIHandler = null;

    void Start()
    {
        //Cargar el data del auto.
        carDatas = Resources.LoadAll<CarData>("CarData/");

        StartCoroutine(SpawnCarCO(true));
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            OnPreviousCar();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            OnNextCar();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSelectCar();
        }
    }

    public void OnPreviousCar()
    {
        if (isChangingCar)
            return;

        selectedCarIndex--;

        if (selectedCarIndex < 0)
            selectedCarIndex = carDatas.Length - 1;

        StartCoroutine(SpawnCarCO(true));
    }

    public void OnNextCar()
    {
        if (isChangingCar)
            return;

        selectedCarIndex++;

        if (selectedCarIndex > carDatas.Length - 1)
            selectedCarIndex = 0;

        StartCoroutine(SpawnCarCO(false));
    }

public void OnSelectCar()
{
        GameManager.instance.ClearDriversList();

        GameManager.instance.AddDriverToList(1, "P1", carDatas[selectedCarIndex].CarUniqueID, false);

        //Crea una nueva lista de autos.
        List<CarData> uniqueCars = new List<CarData>(carDatas);

        //Borra el auto que el jugador escogió.
        uniqueCars.Remove(carDatas[selectedCarIndex]);

        string[] names = { "Axel Reid", "Blaze Carter", "Jett Walker", "Nova King" };
        List<string> uniqueNames = names.ToList<string>();

        //Agregar conductores IA.
        for (int i = 2; i < 5; i++)
        {
            string driverName = uniqueNames[Random.Range(0, uniqueNames.Count)];
            uniqueNames.Remove(driverName);

            CarData carData = uniqueCars[Random.Range(0, uniqueCars.Count)];

            GameManager.instance.AddDriverToList(i, driverName, carData.CarUniqueID, true);
        }

        SceneManager.LoadScene("Course");
}

IEnumerator SpawnCarCO(bool isCarAppearingOnRightSide)
{
    isChangingCar = true;

    if (carUIHandler != null)
        carUIHandler.StartCarExitAnimation(!isCarAppearingOnRightSide);

    GameObject instantiatedCar = Instantiate(carPrefab, spawnOnTransform);

    carUIHandler = instantiatedCar.GetComponent<CarUIHandler>();
    carUIHandler.SetupCar(carDatas[selectedCarIndex]);
    carUIHandler.StartCarEntranceAnimation(isCarAppearingOnRightSide);

    yield return new WaitForSeconds(0.4f);

    isChangingCar = false;
    }
}