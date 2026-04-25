using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostCarRecorder : MonoBehaviour
{
    public Transform carSpriteObject;
    public GameObject ghostCarPlaybackPrefab;

    //Variables locales
    GhostCarData ghostCarData = new GhostCarData();

    bool isRecording = true;

    //Otros componentes
    Rigidbody2D carRigidbody2D;
    CarInputHandler carInputHandler;

    private void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        carInputHandler = GetComponent<CarInputHandler>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Crea un auto fantasma.
        GameObject ghostCar = Instantiate(ghostCarPlaybackPrefab);

        //Carga el data para el jugador currente.
        ghostCar.GetComponent<GhostCarPlayback>().LoadData(carInputHandler.playerNumber);

        StartCoroutine(RecordCarPositionCO());
        StartCoroutine(SaveCarPositionCO());
    }

    IEnumerator RecordCarPositionCO()
    {
        while (isRecording)
        {
            if (carSpriteObject != null)
                ghostCarData.AddDataItem(new GhostCarDataListItem(carRigidbody2D.position, carRigidbody2D.rotation, carSpriteObject.localScale, Time.timeSinceLevelLoad));

            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator SaveCarPositionCO()
    {
        yield return new WaitForSeconds(5);

        SaveData();
    }

    void SaveData()
    {
        string jsonEncodedData = JsonUtility.ToJson(ghostCarData);

        Debug.Log($"Data de auto fantasma guardado {jsonEncodedData}");

        if (carInputHandler != null)
        {
            PlayerPrefs.SetString($"{SceneManager.GetActiveScene().name}_{carInputHandler.playerNumber}_ghost", jsonEncodedData);
            PlayerPrefs.Save();
        }

        //Detener la grabación si ya guardamos el data.
        isRecording = false;
    }
}
