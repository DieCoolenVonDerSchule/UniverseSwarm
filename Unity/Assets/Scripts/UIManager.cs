using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    public GameObject startMenu;
    public InputField systemIDField;
    public InputField planetIDField;
    public MeshGenerator meshGen;


    private void Awake()
    
    {
            if (instance == null)
            {
                instance = this;
            }

            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying object");
                Destroy(this);
            }
     
    }


    public void ConnectToServer()
    {
        DateTime before = DateTime.UtcNow;
        string systemID = systemIDField.text;   
        string planetID = planetIDField.text;

        print("SYSTEM ID: "+systemID);
        print("PLANET ID: "+planetID);

        startMenu.SetActive(false);
        systemIDField.interactable = false;
        planetIDField.interactable = false;
        Client.instance.ConnectToServer(systemID, planetID, meshGen, before);
        //ClientClean.instance.ConnectToServer();


        startMenu.SetActive(true);
        systemIDField.interactable = true;
        planetIDField.interactable = true;


    }


}
