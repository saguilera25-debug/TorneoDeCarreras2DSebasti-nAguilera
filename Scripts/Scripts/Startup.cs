using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startup 
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InstantiatePrefabs()
    {
        Debug.Log("-- Instanciando objetos --");

        GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("InstantiateOnLoad/");

        foreach (GameObject prefab in prefabsToInstantiate)
        {
            Debug.Log($"Creando {prefab.name}");

            GameObject.Instantiate(prefab);
        }

        Debug.Log("-- Instanciando objetos completado --");
    }
}