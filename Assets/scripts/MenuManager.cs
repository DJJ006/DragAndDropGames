using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public void LoadGame1()
    {
        SceneManager.LoadScene("CityScene"); 
    }

    public void LoadGame2()
    {
        Debug.Log("Game 2 is not available yet!");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("BeginScene"); 
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game closed!"); 
    }
}
