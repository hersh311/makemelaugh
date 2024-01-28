using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Start_game : MonoBehaviour
{
    public GameObject PauseMenuUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    public void Instructions()
    {
        Debug.Log("INS");
        PauseMenuUI.SetActive(true);
    }
    public void Exit()
    {
        PauseMenuUI.SetActive(false); 
    }
}
