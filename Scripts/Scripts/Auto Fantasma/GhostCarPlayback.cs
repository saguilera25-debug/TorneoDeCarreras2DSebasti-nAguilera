using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Este script se encarga de reproducir la carrera del auto fantasma. Tiene una función pública LoadData para cargar los datos de la carrera del auto fantasma desde PlayerPrefs, y en cada frame de Update, reproduce la posición, rotación y escala del auto fantasma según los datos cargados.
public class GhostCarPlayback : MonoBehaviour
{
    //Variables locales que se usan para manejar la reproducción de la carrera del auto fantasma.
    GhostCarData ghostCarData = new GhostCarData();
    List<GhostCarDataListItem> ghostCarDataList = new List<GhostCarDataListItem>();

    //Indice de reproducción actual. Se usa para saber qué item de data del auto fantasma estamos reproduciendo actualmente, y para avanzar al siguiente item de data cuando sea necesario.
    int currentPlaybackIndex = 0;

    //Información guardada del playback anterior. Se usa para almacenar la información del item de data del auto fantasma que se está reproduciendo actualmente, para poder lerpear entre esa información y la información del siguiente item de data del auto fantasma para hacer que la reproducción sea suave y fluida, en lugar de tener cambios bruscos en la posición, rotación y escala del auto fantasma cada vez que avanzamos al siguiente item de data.
    float lastStoredTime = 0.1f;
    Vector2 lastStoredPosition = Vector2.zero;
    float lastStoredRotation = 0;
    Vector3 lastStoredLocalScale = Vector3.zero;

    //Duración del data del frame actual. Se usa para calcular el porcentaje de lerp entre el item de data del auto fantasma que se está reproduciendo actualmente y el siguiente item de data del auto fantasma, para hacer que la reproducción sea suave y fluida, en lugar de tener cambios bruscos en la posición, rotación y escala del auto fantasma cada vez que avanzamos al siguiente item de data.
    float duration = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame after the MonoBehaviour is created
    void Update()
    {
        //Solo podemos reproducir datos si hay datos disponibles. Si no hay datos disponibles, no hacemos nada.
        if (ghostCarDataList.Count == 0)
            return;

        if (Time.timeSinceLevelLoad >= ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded)
        {
            lastStoredTime = ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded;
            lastStoredPosition = ghostCarDataList[currentPlaybackIndex].position;
            lastStoredRotation = ghostCarDataList[currentPlaybackIndex].rotationZ;
            lastStoredLocalScale = ghostCarDataList[currentPlaybackIndex].localScale;

            //Seguir al siguiente item de data del auto fantasma, si es que hay un siguiente item de data disponible. Si no hay un siguiente item de data disponible, entonces nos quedamos en el último item de data y simplemente dejamos de actualizar la posición, rotación y escala del auto fantasma, para evitar que haya errores de índice fuera de rango o que el auto fantasma desaparezca o se teletransporte a una posición incorrecta.
            if (currentPlaybackIndex < ghostCarDataList.Count - 1)
                currentPlaybackIndex++;

            duration = ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded - lastStoredTime;
        }

        //Calcular la cantidad del frame de data que completamos. Esto se hace calculando el tiempo que ha pasado desde que comenzamos a reproducir el item de data del auto fantasma que se está reproduciendo actualmente, y dividiendo ese tiempo entre la duración del item de data del auto fantasma que se está reproduciendo actualmente, para obtener un porcentaje de lerp entre el item de data del auto fantasma que se está reproduciendo actualmente y el siguiente item de data del auto fantasma, para hacer que la reproducción sea suave y fluida, en lugar de tener cambios bruscos en la posición, rotación y escala del auto fantasma cada vez que avanzamos al siguiente item de data.
        float timePassed = Time.timeSinceLevelLoad - lastStoredTime;
        float lerpPercentage = timePassed / duration;

        //Lerpear todo entre el item de data del auto fantasma que se está reproduciendo actualmente y el siguiente item de data del auto fantasma, usando el porcentaje de lerp calculado anteriormente, para hacer que la reproducción sea suave y fluida, en lugar de tener cambios bruscos en la posición, rotación y escala del auto fantasma cada vez que avanzamos al siguiente item de data.
        transform.position = Vector2.Lerp(lastStoredPosition, ghostCarDataList[currentPlaybackIndex].position, lerpPercentage);
        transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, lastStoredRotation), Quaternion.Euler(0, 0, ghostCarDataList[currentPlaybackIndex].rotationZ), lerpPercentage);
        transform.localScale = Vector3.Lerp(lastStoredLocalScale, ghostCarDataList[currentPlaybackIndex].localScale, lerpPercentage);
    }

    public void LoadData(int playerNumber)
    {
        if (!PlayerPrefs.HasKey($"{SceneManager.GetActiveScene().name}_{playerNumber}_ghost"))
            Destroy(gameObject);
        else
        {
            string jsonEncodedData = PlayerPrefs.GetString($"{SceneManager.GetActiveScene().name}_{playerNumber}_ghost");

            ghostCarData = JsonUtility.FromJson<GhostCarData>(jsonEncodedData);
            ghostCarDataList = ghostCarData.GetDataList();
        }
    }
}
