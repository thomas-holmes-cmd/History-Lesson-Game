using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Maps_Selection : MonoBehaviour
{
    public void LoadMap1()
    {
        SceneManager.LoadScene("Map1");
    }
    public void LoadMap2()
    {
        SceneManager.LoadScene("Map2");
    }
    public void LoadMap3()
    {
        SceneManager.LoadScene("Map3");
    }
}
