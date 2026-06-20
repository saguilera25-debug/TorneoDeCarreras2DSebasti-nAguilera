using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

// Este script se encarga de manejar la interfaz de selección de auto. Permite al jugador cambiar entre los diferentes autos disponibles para seleccionar, y luego guardar la selección del auto para que pueda ser usada en la carrera. También se encarga de mostrar una animación de entrada y salida del auto cada vez que el jugador cambia de auto, para hacer la interfaz más atractiva visualmente.
public class SelectCarUIHandler : MonoBehaviour
{
    [Header("Prefab de auto")]
    public GameObject carPrefab;

    [Header("Spawnear")]
    public Transform spawnOnTransform;

    bool isChangingCar = false;

    CarData[] carDatas;

    int selectedCarIndex = 0;

    //Otros componentes que necesitan ser referenciados para mostrar la animación de entrada y salida del auto.
    CarUIHandler carUIHandler = null;

    void Start()
    {
        //Cargar el data del auto. Esto se hace para poder mostrar la información del auto en la interfaz de selección, y para poder usar esa información en la carrera después de que el jugador seleccione un auto. El data del auto se guarda como un ScriptableObject, lo que permite crear diferentes tipos de autos con diferentes características sin necesidad de escribir código adicional para cada tipo de auto.
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

        //Crea una nueva lista de autos disponibles para seleccionar, sin el auto que el jugador seleccionó. Esto se hace para que los autos que no fueron seleccionados por el jugador puedan ser usados como autos de AI en la carrera, lo que hace la carrera más variada y divertida.
        List<CarData> uniqueCars = new List<CarData>(carDatas);

        //Borra el auto que el jugador seleccionó de la lista de autos disponibles para seleccionar, para que no pueda ser usado como auto de AI en la carrera.
        uniqueCars.Remove(carDatas[selectedCarIndex]);

        string[] names = { "Jett Walker", "Blaze Carter", "Axel Reid", "Nova King" };
        List<string> uniqueNames = names.ToList<string>();

        //Agrega conductores IA 
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