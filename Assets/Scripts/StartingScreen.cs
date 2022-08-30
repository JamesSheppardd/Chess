using UnityEngine;
using UnityEngine.SceneManagement; 

public class StartingScreen : MonoBehaviour
{   
    public GameObject loading;
    public void StartGame(string colour){
        loading.SetActive(true);
        if(colour == "White")   SceneManager.LoadScene("MainBlack", LoadSceneMode.Single);
        else if(colour == "Black")  SceneManager.LoadScene("MainWhite", LoadSceneMode.Single);
        else if(colour == "Friend")  SceneManager.LoadScene("MainFriend", LoadSceneMode.Single);
    }

    public void QuitGame(){
        Application.Quit();
    }
}
