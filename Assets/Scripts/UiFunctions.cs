using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;








public class Preset
{
    public string _name;

    public float _scale;
    public float _coarse;
    public float _contrib;
    public float _output;

    public float _plantscale;
    public float _occurance;
    public float _radius;

    public bool _shader;
    public bool _threading;
    public bool _plants;


    public Preset(string name, float scale, float coarse, float contrib, float output, bool shader, bool threading, bool plants)
    {
        _name = name;
        _scale = scale;
        _coarse = coarse;
        _contrib = contrib;
        _output = output;
        
        _shader = shader;
        _threading = threading;
        _plants = plants;
    }


}


public class UiFunctions : MonoBehaviour
{


    public static List<Preset> presets;



    public void Start()
    {
        if (presets == null) initialize();

        
        
    }

    public static void initialize()
    {

        presets = new List<Preset>();
        GameObject.FindGameObjectWithTag("presets").GetComponent<UnityEngine.UI.Dropdown>().ClearOptions();
        

    }


   

    
    



    public void setDefault()       // Setzen der Standartwerte
    {
        int sizex = 500;
        int sizey = 500;

        float scale = 0.015f;
        float coarse = 1.3f;
        float contrib = 6.5f;
        float output = 1.0f;

        int maps = 5;
        float speed = 5.0f;

   

        string filename = "presets";


        GameObject.FindGameObjectWithTag("sizex").GetComponent<UnityEngine.UI.InputField>().text = "" + sizex;
        GameObject.FindGameObjectWithTag("sizey").GetComponent<UnityEngine.UI.InputField>().text = "" + sizey;
        
        GameObject.FindGameObjectWithTag("scale").GetComponent<UnityEngine.UI.InputField>().text = "" + scale;
        GameObject.FindGameObjectWithTag("coarse").GetComponent<UnityEngine.UI.InputField>().text = "" + coarse;  
        GameObject.FindGameObjectWithTag("contrib").GetComponent<UnityEngine.UI.InputField>().text = "" + contrib;
        GameObject.FindGameObjectWithTag("output").GetComponent<UnityEngine.UI.InputField>().text = "" + output;


        GameObject.FindGameObjectWithTag("maps").GetComponent<UnityEngine.UI.InputField>().text = "" + maps;
        GameObject.FindGameObjectWithTag("speed").GetComponent<UnityEngine.UI.InputField>().text = "" + speed;

        GameObject.FindGameObjectWithTag("scaleslider").GetComponent<UnityEngine.UI.Slider>().value = scale;
        GameObject.FindGameObjectWithTag("coarseslider").GetComponent<UnityEngine.UI.Slider>().value = coarse;
        GameObject.FindGameObjectWithTag("contribslider").GetComponent<UnityEngine.UI.Slider>().value = contrib;
        GameObject.FindGameObjectWithTag("outputslider").GetComponent<UnityEngine.UI.Slider>().value = output;
  

        GameObject.FindGameObjectWithTag("shadertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = true;
        GameObject.FindGameObjectWithTag("threadingtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = false;
        GameObject.FindGameObjectWithTag("plantstoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = false;

        GameObject.FindGameObjectWithTag("filename").GetComponent<UnityEngine.UI.InputField>().text = filename;

    }


    public void setScale()      // Scale wird per Slider verändert
    {
        GameObject.FindGameObjectWithTag("scale").GetComponent<UnityEngine.UI.InputField>().text =
            GameObject.FindGameObjectWithTag("scaleslider").GetComponent<UnityEngine.UI.Slider>().value.ToString();
    }

    public void setScaleField()      // Scale wird per Field verändert
    {
        string scaleStr = GameObject.FindGameObjectWithTag("scale").GetComponent<UnityEngine.UI.InputField>().text;
        float scaleSet = float.Parse(scaleStr);
        GameObject.FindGameObjectWithTag("scaleslider").GetComponent<UnityEngine.UI.Slider>().value = scaleSet;
    }



    public void setCoarse()     // Coarseness wird per Slider verändert
    {
        GameObject.FindGameObjectWithTag("coarse").GetComponent<UnityEngine.UI.InputField>().text =
            GameObject.FindGameObjectWithTag("coarseslider").GetComponent<UnityEngine.UI.Slider>().value.ToString();
    }

    public void setCoarseField()     // Coarseness wird per Field verändert
    {
        string coarseStr = GameObject.FindGameObjectWithTag("coarse").GetComponent<UnityEngine.UI.InputField>().text;
        float coarseSet = float.Parse(coarseStr);
        GameObject.FindGameObjectWithTag("coarseslider").GetComponent<UnityEngine.UI.Slider>().value = coarseSet;
    }


    public void setContrib()    // Contribution wird per Slider verändert
    {
        GameObject.FindGameObjectWithTag("contrib").GetComponent<UnityEngine.UI.InputField>().text =
            GameObject.FindGameObjectWithTag("contribslider").GetComponent<UnityEngine.UI.Slider>().value.ToString();
    }

    public void setContribField()    // Contribution wird per Field verändert
    {
        string contribStr = GameObject.FindGameObjectWithTag("contrib").GetComponent<UnityEngine.UI.InputField>().text;
        float contribSet = float.Parse(contribStr);
        GameObject.FindGameObjectWithTag("contribslider").GetComponent<UnityEngine.UI.Slider>().value = contribSet;
    }

   



    public void setOutput()     // Output wird per Slider verändert
    {
        GameObject.FindGameObjectWithTag("output").GetComponent<UnityEngine.UI.InputField>().text =
            GameObject.FindGameObjectWithTag("outputslider").GetComponent<UnityEngine.UI.Slider>().value.ToString();
    }

    public void setOutputField()     // Output wird per Field verändert
    {
        string outputStr = GameObject.FindGameObjectWithTag("output").GetComponent<UnityEngine.UI.InputField>().text;
        float outputSet = float.Parse(outputStr);
        GameObject.FindGameObjectWithTag("outputslider").GetComponent<UnityEngine.UI.Slider>().value = outputSet;
    }


    

    public void toggleMenu()      // Münü an-aus schalten
    {
        GameObject.FindGameObjectWithTag("canvas").GetComponent<UnityEngine.Canvas>().enabled = !GameObject.FindGameObjectWithTag("canvas").GetComponent<UnityEngine.Canvas>().enabled;
    }



    public void toggleShader()     // Compute Shader wird geklickt
    {
        if (GameObject.FindGameObjectWithTag("shadertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            GameObject.FindGameObjectWithTag("threadingtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = false;
        }
    }


    public void toggleThreading()      // Threading wird geklickt
    {
        if (GameObject.FindGameObjectWithTag("threadingtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            GameObject.FindGameObjectWithTag("shadertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = false;
        }
    }



    public void setPreset()
    {

        

        int p = GameObject.FindGameObjectWithTag("presets").GetComponent<UnityEngine.UI.Dropdown>().value;

        print("set p# = " + p);


       

        GameObject.FindGameObjectWithTag("scale").GetComponent<UnityEngine.UI.InputField>().text = "" + presets[p]._scale;

        
        GameObject.FindGameObjectWithTag("coarse").GetComponent<UnityEngine.UI.InputField>().text = "" + presets[p]._coarse;
        GameObject.FindGameObjectWithTag("contrib").GetComponent<UnityEngine.UI.InputField>().text = "" + presets[p]._contrib;
        GameObject.FindGameObjectWithTag("output").GetComponent<UnityEngine.UI.InputField>().text = "" + presets[p]._output;
      

        GameObject.FindGameObjectWithTag("scaleslider").GetComponent<UnityEngine.UI.Slider>().value = presets[p]._scale;
        GameObject.FindGameObjectWithTag("coarseslider").GetComponent<UnityEngine.UI.Slider>().value = presets[p]._coarse;
        GameObject.FindGameObjectWithTag("contribslider").GetComponent<UnityEngine.UI.Slider>().value = presets[p]._contrib;
        GameObject.FindGameObjectWithTag("outputslider").GetComponent<UnityEngine.UI.Slider>().value = presets[p]._output;
   
    


        GameObject.FindGameObjectWithTag("shadertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = presets[p]._shader;
        GameObject.FindGameObjectWithTag("threadingtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = presets[p]._threading;
        GameObject.FindGameObjectWithTag("plantstoggle").GetComponent<UnityEngine.UI.Toggle>().isOn = presets[p]._plants;



    }

    public void savePreset()
    {

        string name = GameObject.FindGameObjectWithTag("presetname").GetComponent<UnityEngine.UI.InputField>().text;

        float scale = float.Parse(GameObject.FindGameObjectWithTag("scale").GetComponent<UnityEngine.UI.InputField>().text);
        float coarse = float.Parse(GameObject.FindGameObjectWithTag("coarse").GetComponent<UnityEngine.UI.InputField>().text);
        float contrib = float.Parse(GameObject.FindGameObjectWithTag("contrib").GetComponent<UnityEngine.UI.InputField>().text);
        float output = float.Parse(GameObject.FindGameObjectWithTag("output").GetComponent<UnityEngine.UI.InputField>().text);

        bool shader = GameObject.FindGameObjectWithTag("shadertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn;
        bool threading = GameObject.FindGameObjectWithTag("threadingtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn;
        bool plants = GameObject.FindGameObjectWithTag("plantstoggle").GetComponent<UnityEngine.UI.Toggle>().isOn;

        Preset p = new Preset(name, scale, coarse, contrib, output, shader, threading, plants);


        print("save p# = " +p);
        presets.Add(p);

        UnityEngine.UI.Dropdown.OptionData newoption = new UnityEngine.UI.Dropdown.OptionData(name); 
        GameObject.FindGameObjectWithTag("presets").GetComponent<UnityEngine.UI.Dropdown>().options.Add(newoption);
    }
    

    public void deletePreset()
    {
        int p = GameObject.FindGameObjectWithTag("presets").GetComponent<UnityEngine.UI.Dropdown>().value;
        print("delete p# = "+p);
        presets.RemoveAt(p);
        GameObject.FindGameObjectWithTag("presets").GetComponent<UnityEngine.UI.Dropdown>().options.RemoveAt(p);
    }



    public void writeFile()
    {

        string filename = GameObject.FindGameObjectWithTag("filename").GetComponent<UnityEngine.UI.InputField>().text;

        print(Application.dataPath + "/Saved Presets/" + filename);
   

        if (File.Exists(Application.dataPath + "/Saved Presets/" + filename+".txt")) File.Delete(Application.dataPath + "/Saved Presets/" + filename+".txt");
        File.WriteAllText(Application.dataPath + "/Saved Presets/" + filename + ".txt", "");


        foreach (Preset p in presets)
        {
            string savestr = p._name + "*" + p._scale + "*" + p._coarse + "*" + p._contrib + "*" + p._output + "*" + p._shader + "*" + p._threading + "*" + p._plants;
            // (name, scale, coarse, contrib, output, shader, threading, plants)


            print(savestr);

            File.AppendAllText(Application.dataPath + "/Saved Presets/" + filename + ".txt", savestr);
            File.AppendAllText(Application.dataPath + "/Saved Presets/" + filename + ".txt", "#");

        }

        File.AppendAllText(Application.dataPath + "/Saved Presets/" + filename + ".txt", "|");



    }

    public void readFile()
    {

        setDefault();

        string filename = GameObject.FindGameObjectWithTag("filename").GetComponent<UnityEngine.UI.InputField>().text;

        
        string loadstr = File.ReadAllText(Application.dataPath + "/Saved Presets/" + filename + ".txt");

        print(loadstr);


        string[] loaddata = loadstr.Split(separator: '#');



        for (int i = 0; i < loaddata.Length-1; i++)
            {
                print("------------------------------------");
                print(loaddata[i]);
                print("------------------------------------");


                string[] loadpreset = loaddata[i].Split(separator: '*');


                print("------------------------------------");
                print("loadpreset[0] : " + loadpreset[0]);      // name
                print("loadpreset[1] : " + loadpreset[1]);      // scale
                print("loadpreset[2] : " + loadpreset[2]);      // coarse
                print("loadpreset[3] : " + loadpreset[3]);      // contrib
                print("loadpreset[4] : " + loadpreset[4]);      // output
                
                print("loadpreset[8] : " + loadpreset[5]);      // shader
                print("loadpreset[9] : " + loadpreset[6]);      // threading
                print("loadpreset[10] : " + loadpreset[7]);    // plants
                print("------------------------------------");


                string name = loadpreset[0];
                float scale = float.Parse(loadpreset[1]);
                float coarse = float.Parse(loadpreset[2]);
                float contrib = float.Parse(loadpreset[3]);
                float output = float.Parse(loadpreset[4]);
                
                bool shader = false;
                bool threading = false;
                bool plants = false;


                if (loadpreset[5] == "True") shader = true;
                if (loadpreset[6] == "True") threading = true;
                if (loadpreset[7] == "True") plants = true;


            print("--------BOOL VARAIBLES-------");
            print("shader: "+shader);
            print("thread: "+shader);
            print("plants: "+shader);
            print("-----------------------------");


            Preset p = new Preset(name, scale, coarse, contrib, output, shader, threading, plants);
                presets.Add(p);


                UnityEngine.UI.Dropdown.OptionData newoption = new UnityEngine.UI.Dropdown.OptionData(name);
                GameObject.FindGameObjectWithTag("presets").GetComponent<UnityEngine.UI.Dropdown>().options.Add(newoption);


            }

            
        }


      
}
