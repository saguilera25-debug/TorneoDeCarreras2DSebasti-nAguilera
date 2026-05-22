using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuUIHandler : MonoBehaviour
{
    //Otros componentes
    Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();

        canvas.enabled = false;

        //Conectar eventos
        GameManager.instance.OnGameStateChanged += OnGameStateChanged;
    }

    public void OnRaceAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExitToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    IEnumerator ShowMenuCO()
    {
        yield return new WaitForSeconds(1);

        canvas.enabled = true;
    }

//Eventos
void OnGameStateChanged(GameManager gameManager)
{
    if (GameManager.instance.GetGameState() == GameStates.raceOver)
    {
        StartCoroutine(ShowMenuCO());
    }
}

void OnDestroy()
{
    //Desconectar eventos
    GameManager.instance.OnGameStateChanged -= OnGameStateChanged;
}
}
