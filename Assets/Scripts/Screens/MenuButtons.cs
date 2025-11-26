using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;

        // Asegurarnos de que exista el RoomRandomizer
        if (RoomRandomizer.Instance == null)
        {
            Debug.LogError("[Buttons] No se encontró un RoomRandomizer en la escena inicial.");
            return;
        }

        // Generar la run antes de empezar
        RoomRandomizer.Instance.GenerateRun();

        // Cargar la primera sala de la lista
        string firstScene = RoomRandomizer.Instance.GetNextRoom();

        if (!string.IsNullOrEmpty(firstScene))
        {
            SceneManager.LoadScene(firstScene);
        }
        else
        {
            Debug.LogError("[Buttons] La run generada no tiene salas configuradas.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Options");
    }

    public void Back()
    {
        SceneManager.LoadScene("Menu");
    }
}
