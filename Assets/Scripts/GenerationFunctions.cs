using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;




public class GenerationFunctions : MonoBehaviour
{

    
    public static Vector2[] startwerte;

    public ComputeShader shader;

    public MeshGenerator meshgen;

    public GameObject wasser;
    public float wasserspiegel;
    public float radius;


    public struct PerlinInfo
    {
        public int x;
        public int y;
        public float scale;
        public float startx;
        public float starty;
        public float shiftx;
        public float shifty;

        public PerlinInfo(int x, int y, float scale, float startx, float starty, float shiftx, float shifty)
        {
            this.x = x;
            this.y = y;
            this.scale = scale;
            this.startx = startx;
            this.starty = starty;
            this.shiftx = shiftx;
            this.shifty = shifty;
        }
    };

    public struct generatePerlinJob : IJobParallelFor
    {
        public NativeArray<PerlinInfo> perlinInfoArray;
        public NativeArray<float> perlinOutput;

        public void Execute(int index)
        {
            var data = perlinInfoArray[index];
            perlinOutput[index] = Mathf.PerlinNoise(data.startx + data.shiftx * data.scale, data.starty + data.shifty * data.scale);
        }
    }





    public static void initializeConstants(int mapcount)
    {
        
        if (startwerte == null)
        {
            startwerte = new Vector2[mapcount];
            for (int i = 0; i < mapcount; i++)
            {
                startwerte[i] = new Vector2(Random.Range(0, 1000), Random.Range(0, 1000));
                
            }
        }
      
    }


    public static float[,] createHeightMapRandom(int x, int y)
    {
        float[,] heightmap = new float[x, y];

        for (int i=0; i<x; i++)
        {
            for (int j=0; j<y; j++)
            {
                heightmap[i, j] = Random.value;
            }
        }


        return heightmap;
    }



    public float[,] createHeightMapPerlinNoiseCS(
            int x, int y, float scale, float startx, float starty, float shiftx, float shifty)
    {

        if (GameObject.FindGameObjectWithTag("debugtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn) 
           { print("START COMPUTE SHADER"); }

        PerlinInfo[] data = new PerlinInfo[x * y];
        float[] output = new float[x * y];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                data[i * y + j] = new PerlinInfo ( x, y, scale, startx, starty, shiftx+i, shifty+j);
            }
        }

        ComputeBuffer buffer = new ComputeBuffer(data.Length, 28);
        buffer.SetData(data);
        ComputeBuffer outputBuffer = new ComputeBuffer(output.Length, 4);
        outputBuffer.SetData(output);


        int kernel = shader.FindKernel("computeHeightMap");
        shader.SetBuffer(kernel, "dataBuffer", buffer);
        shader.SetBuffer(kernel, "output", outputBuffer);
        shader.Dispatch(kernel, data.Length/32, 1, 1);
        outputBuffer.GetData(output);

        buffer.Dispose();
        outputBuffer.Dispose();


        float[,] heightmap = new float[x, y];


        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                heightmap[i, j] = output[i * y + j];
            }
        }

        if (GameObject.FindGameObjectWithTag("debugtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        { print("END COMPUTE SHADER"); }

        return heightmap;
    }


    public static float[,] createHeightMapPerlinNoise(
                 int x, int y, float scale, float startx, float starty, float shiftx, float shifty)
    {
            
        float[,] heightmap = new float[x, y];

       
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                heightmap[i, j] = Mathf.PerlinNoise(startx+(shiftx+i)*scale, starty+(shifty+j) * scale);
            }
        }

        return heightmap;
    }



    public static float[,] createHeightMapPerlinNoiseJobs(
          int x, int y, float scale, float startx, float starty, float shiftx, float shifty)
    {

        var perlinInfoArray = new NativeArray<PerlinInfo>(x * y, Allocator.Persistent);
        var output = new NativeArray<float>(x * y, Allocator.Persistent);

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                perlinInfoArray[i * y + j] = 
                       new PerlinInfo(x, y, scale, startx, starty, shiftx + i, shifty + j);
            }
        }

        var job = new generatePerlinJob
        {
            perlinInfoArray = perlinInfoArray,
            perlinOutput = output
        };

        var jobHandle = job.Schedule(x * y, 1);
        jobHandle.Complete();

        float[,] heightmap = new float[x, y];


        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                heightmap[i, j] = output[i * y + j];
            }
        }

        perlinInfoArray.Dispose();
        output.Dispose();

        return heightmap;
    }



    public static float[,] createHeightMapPerlinNoiseLP(
                 int x, int y, float scale, float startx, float starty, float radius)
    {

        float[,] heightmap = new float[x, y];


        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                heightmap[i, j] = Mathf.PerlinNoise(startx + Mathf.Sin(i * Mathf.PI*2/x) * scale * radius/y *j, starty + Mathf.Cos(i * Mathf.PI * 2 / x) * scale * radius/y * j);
            }
        }

        return heightmap;
    }



    public void GenerateRandom()
    {
     
        string xinput = GameObject.FindGameObjectWithTag("sizex").GetComponent<UnityEngine.UI.InputField>().text;
        string yinput = GameObject.FindGameObjectWithTag("sizey").GetComponent<UnityEngine.UI.InputField>().text;

        int x = int.Parse(xinput);
        int y = int.Parse(yinput);

        float[,] heightmap = createHeightMapRandom(x, y);


        if (GameObject.FindGameObjectWithTag("debugtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            debugHeightMap(heightmap);
        }
        
    }



    public void GenerateStart()
    {
        
        GeneratePerlinNoise(meshgen);
        Movement.initializeConstants();
    }

    public void GeneratePerlinNoise(MeshGenerator meshgen)
    {

        string xinput = GameObject.FindGameObjectWithTag("sizex").GetComponent<UnityEngine.UI.InputField>().text;
        string yinput = GameObject.FindGameObjectWithTag("sizey").GetComponent<UnityEngine.UI.InputField>().text;

        int x = int.Parse(xinput);
        int y = int.Parse(yinput);

        string scaleInput = GameObject.FindGameObjectWithTag("scale").GetComponent<UnityEngine.UI.InputField>().text;
        float scale = float.Parse(scaleInput);

        string coarseInput = GameObject.FindGameObjectWithTag("coarse").GetComponent<UnityEngine.UI.InputField>().text;
        string contributionInput = GameObject.FindGameObjectWithTag("contrib").GetComponent<UnityEngine.UI.InputField>().text;

        float coarse = float.Parse(coarseInput);
        float contribution = float.Parse(contributionInput);


        if (GameObject.FindGameObjectWithTag("debugtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            print("scaleInput: " + scaleInput);
            print("scale     : " + scale);
        }

        string mapsinput = GameObject.FindGameObjectWithTag("maps").GetComponent<UnityEngine.UI.InputField>().text;
        int mapcount = int.Parse(mapsinput);



        float[][,] heightmaps = new float[mapcount][,];

        float shiftx = 0f;
        float shifty = 0f;

        bool shaderIsOn = (GameObject.FindGameObjectWithTag("shadertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn);
        bool threadingIsOn = (GameObject.FindGameObjectWithTag("threadingtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn);

        initializeConstants(mapcount);
       
         
        for (int i=0; i<mapcount; i++)
        {
            
            if (shaderIsOn) heightmaps[i] = createHeightMapPerlinNoiseLP(x, y, scale * Mathf.Pow(i+1,coarse),startwerte[i].x, startwerte[i].y, radius);
            else if (threadingIsOn) heightmaps[i] = createHeightMapPerlinNoiseJobs(x, y, scale * Mathf.Pow(i + 1, coarse), startwerte[i].x, startwerte[i].y, shiftx, shifty);
            else heightmaps[i] = createHeightMapPerlinNoise(x, y, scale * Mathf.Pow(i + 1, coarse), startwerte[i].x, startwerte[i].y, shiftx, shifty);
        }


        float[,] heightmapCombined = new float[x, y];

        for (int i=0; i<mapcount; i++)
        {
            for (int j=0; j<x; j++)
            {
                for (int k=0; k<y; k++)
                {
                    heightmapCombined[j, k] += (heightmaps[i][j, k]/((i*contribution+1)));
                }
            }
            
        }

        float highest = 0;
        float lowest = 10000;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (heightmapCombined[i, j] > highest) highest = heightmapCombined[i, j];
                if (heightmapCombined[i, j] < lowest) lowest = heightmapCombined[i, j];
            }
        }

        
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                heightmapCombined[i, j] = Mathf.InverseLerp(lowest, highest, heightmapCombined[i, j]);
            }
        }
               
        if (GameObject.FindGameObjectWithTag("debugtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn) debugHeightMap(heightmapCombined);      


        
        meshgen.generateMesh(heightmapCombined);


        if (GameObject.FindGameObjectWithTag("plantstoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        {


  
            foreach(DecorationFunctions decfun in meshgen.GetComponents<DecorationFunctions>())
            {
                decfun.placePlants(heightmapCombined);
            }
        }


        if (GameObject.FindGameObjectWithTag("watertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            wasser.GetComponent<MeshRenderer>().enabled = true;

        }

    }

    

    public void debugHeightMap(float[,] map)
    {
        float lowestHeight = 100;
        float highestHeight = 0;
        float lowestStep = 100;
        float highestStep = 0;
        float step = 0;
        float lastHeight = 0;


        for (int i=0; i<map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                print(map[i,j]);

                if (map[i, j] < lowestHeight) lowestHeight = map[i, j];
                if (map[i, j] > highestHeight) highestHeight = map[i, j];

                step = (map[i, j] - lastHeight);
                if (step < 0) step = step * (-1);

                if (step < lowestStep) lowestStep = step;
                if (step > highestStep) highestStep = step;
                
            }         

        }

        print("------------------------");
        print("Lowest : " + lowestHeight);
        print("Highest: " + highestHeight);
        print("------------------------");
        print("Lowest Step : " + lowestStep);
        print("Highest Step: " + highestStep);
        print("------------------------");

    }







    public void randomize()
    {

        for (int i = 0; i < startwerte.Length; i++)
        {
            startwerte[i] = new Vector2(Random.Range(0, 1000), Random.Range(0, 1000));
        }

    
        GeneratePerlinNoise(meshgen);
    }



    public void randomHeightmap()                   // RANDOM PN MAP ERSTELLEN
    {
        float scale = Random.Range(0.0f, 0.1f);
        float coarse = Random.Range(0.0f, 2.0f);
        float contrib = Random.Range(0.0f, 10.0f);

        float output = Random.Range(2.0f, 8.0f);


        GameObject.FindGameObjectWithTag("scale").GetComponent<UnityEngine.UI.InputField>().text = "" + scale;
        GameObject.FindGameObjectWithTag("coarse").GetComponent<UnityEngine.UI.InputField>().text = "" + coarse;
        GameObject.FindGameObjectWithTag("contrib").GetComponent<UnityEngine.UI.InputField>().text = "" + contrib;
        GameObject.FindGameObjectWithTag("output").GetComponent<UnityEngine.UI.InputField>().text = "" + output;


        GameObject.FindGameObjectWithTag("scaleslider").GetComponent<UnityEngine.UI.Slider>().value = scale;
        GameObject.FindGameObjectWithTag("coarseslider").GetComponent<UnityEngine.UI.Slider>().value = coarse;
        GameObject.FindGameObjectWithTag("contribslider").GetComponent<UnityEngine.UI.Slider>().value = contrib;
        GameObject.FindGameObjectWithTag("outputslider").GetComponent<UnityEngine.UI.Slider>().value = output;




        if (GameObject.FindGameObjectWithTag("debugtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            print("RANDOM GENERATED PN:");
            print("--------------------");
            print("Scale:   " + scale);
            print("Coarse:  " + coarse);
            print("Contrib: " + contrib);
            print("Output:  " + output);

        }



    }

}
