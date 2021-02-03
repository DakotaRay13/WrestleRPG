using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class tst_FinishMenu : MonoBehaviour
{
    //Change scene to the Main Menu
    public void MainMenuButtonClick()
    {
        SceneManager.LoadScene("tst_SetUpMatch");
    }

    //Exit Prototype
    public void ExitApplicationButtonClick()
    {
        Application.Quit();
    }
}
