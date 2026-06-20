using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpData : MonoBehaviour
{
    [Header("JumpData")]
    //La escala afecta cuánto tiempo los aútos quedarán nacidos en el aire cuando choquen con un salto.
    public float jumpHeightScale = 1.0f;

    //Cuánto deben ser empujados los autos hacia adelante cuando choquen con un salto.
    public float jumpPushScale = 1.0f;
}
