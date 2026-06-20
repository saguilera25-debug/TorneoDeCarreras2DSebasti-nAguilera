using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Esta clase se encarga de almacenar los datos de la carrera del auto fantasma. Tiene una lista de objetos GhostCarDataListItem, que representan los datos registrados en cada frame de la carrera del auto fantasma, y tiene funciones para agregar nuevos objetos GhostCarDataListItem a la lista y para obtener la lista completa de objetos GhostCarDataListItem.

[System.Serializable]

public class GhostCarData
{
    [SerializeField]
    List<GhostCarDataListItem> ghostCarRecorderList = new List<GhostCarDataListItem>();

    public void AddDataItem(GhostCarDataListItem ghostCarDataListItem)
    {
        ghostCarRecorderList.Add(ghostCarDataListItem);
    }

    public List<GhostCarDataListItem> GetDataList()
    {
        return ghostCarRecorderList;
    }
} 
