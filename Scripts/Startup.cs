using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startup 
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InstantitatePrefabs()
    {
        Debug.Log("-- instanciando objetos --");

        GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("InstantiateOnLoad/");

        foreach (GameObject prefab in prefabsToInstantiate)
        {
            Debug.Log($"Creando {prefab.name}");

            GameObject.Instantiate(prefab);
        }

        Debug.Log("--Terminamos de instanciar los objetos--");
    }
}