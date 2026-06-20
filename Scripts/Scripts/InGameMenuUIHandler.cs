using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Este script se encarga de mostrar el menú de fin de carrera cuando la carrera termina, y de manejar los botones del menú para reiniciar la carrera o volver al menú principal.
public class InGameMenuUIHandler : MonoBehaviour
{
    //Otros componentes que necesitan ser referenciados para mostrar el menú de fin de carrera.
    Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();

        canvas.enabled = false;

        //Conectar eventos para mostrar el menú de fin de carrera cuando la carrera termina.
        GameManager.instance.OnGameStateChanged += OnGameStateChanged;
    }

    public void OnRaceAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator ShowMenuCO()
    {
        yield return new WaitForSeconds(1);

        canvas.enabled = true;
    }

    //Eventos para mostrar el menú de fin de carrera cuando la carrera termina.
    void OnGameStateChanged(GameManager gameManager)
{
    if (GameManager.instance.GetGameState() == GameStates.raceOver)
    {
        StartCoroutine(ShowMenuCO());
    }
}

void OnDestroy()
{
        //Desconectar eventos para evitar errores de objetos destruidos.
        GameManager.instance.OnGameStateChanged -= OnGameStateChanged;
}
}
