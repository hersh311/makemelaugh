using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_menu : MonoBehaviour
{
    public static bool Gamepaused = false;
    public GameObject PauseMenuUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            try
            {
                if (Gamepaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
            catch
            {
                Debug.Log("Pause Menu not attached");
            }

        }
        
    }
    public void Resume()
    {
        Gamepaused = false;
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

    }
    public void Pause()
    {
        PauseMenuUI.SetActive(true);
        Gamepaused = true;
        Time.timeScale = 0f;
    }
}
