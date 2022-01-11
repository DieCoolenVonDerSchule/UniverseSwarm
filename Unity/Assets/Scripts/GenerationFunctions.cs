using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;




public class GenerationFunctions : MonoBehaviour
{

    
    public static Vector3[] startwerte;

    public ComputeShader shader;

    public MeshGenerator meshgen;

    public GameObject wasser;
    public float wasserspiegel;
    public float radius;

    public Texture3D inspectionTexture;


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
            startwerte = new Vector3[mapcount];
            for (int i = 0; i < mapcount; i++)
            {
                startwerte[i] = new Vector3(Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000));
                
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



    public static List<float> createHeightMapPerlinNoiseLP(
                 int x, int y, float scale, float startx, float starty, float startz, float radius)
    {

        List<float> heightmap = new List<float>();

        float goldenRatio = (1 + Mathf.Pow(5f, 0.5f)) / 2f;

        for (int i = 0; i < x; i++)
        {
            float theta = 2 * Mathf.PI * i / goldenRatio;
            float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / x);

            float vx = Mathf.Cos(theta) * Mathf.Sin(phi);
            float vy = Mathf.Sin(theta) * Mathf.Sin(phi);
            float vz = Mathf.Cos(phi);

            heightmap.Add(Noise(startx + vx * 0.01f, starty + vy * 0.01f, startz + vz * 0.01f));
        }

        /*
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                float vecx = Mathf.Cos(Mathf.PI / x * j) * (radius);
                float vecz = Mathf.Sin(Mathf.PI / x * j) * -(radius) * Mathf.Sin(Mathf.PI * 2 / y * i);
                float vecy = Mathf.Sin(Mathf.PI / x * j) * (radius) * Mathf.Cos(Mathf.PI * 2 / y * i);

                //heightmap[i, j] = Mathf.PerlinNoise(startx + Mathf.Sin(i * Mathf.PI*2/x) * scale * radius/y *j, starty + Mathf.Cos(i * Mathf.PI * 2 / x) * scale * radius/y * j);
                heightmap[i, j] = Noise(startx + vecx*0.1f, starty + vecy * 0.1f, startz + vecz * 0.1f);
            }
        }
        */

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

   // [MenuItem("CreateExamples/3DTexture")]
    private void Start()
    {
        float[,,] density = generateDensityMap(50, 50, 50, 500, 500, 500, 0.01f);

        inspectionTexture = new Texture3D(100, 100, 100, TextureFormat.RGBA32, false);
        inspectionTexture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < 50; i++)
        {
            for (int j = 0; j < 50; j++)
            {
                for (int k = 0; k < 50; k++)
                {
                    inspectionTexture.SetPixel(i, j, k, new Color(255, 255, 255, density[i, j, k]));
                }
            }
        }

        print("WTF???");
        inspectionTexture.Apply();
        UnityEditor.AssetDatabase.CreateAsset(inspectionTexture, "Assets/inspectMe.asset");
    }

    public void GenerateStart()
    {
        
        GenerateCubeMarching(meshgen);
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



        List<float>[] heightmaps = new List<float>[mapcount];

        for(int i = 0; i<heightmaps.Length; i++)
        {
            heightmaps[i] = new List<float>();
        }


        bool shaderIsOn = (GameObject.FindGameObjectWithTag("shadertoggle").GetComponent<UnityEngine.UI.Toggle>().isOn);
        bool threadingIsOn = (GameObject.FindGameObjectWithTag("threadingtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn);

        initializeConstants(mapcount);
       
         
        for (int i=0; i<mapcount; i++)
        {
            
            heightmaps[i] = createHeightMapPerlinNoiseLP(x, y, scale * Mathf.Pow(i+1,coarse),startwerte[i].x, startwerte[i].y, startwerte[i].z, radius);
        }


        List<float> heightmapCombined = new List<float>();

        for (int i=0; i< heightmaps[0].Count; i++)
        {
            float sum = 0f;

            for(int j = 0; j<mapcount; j++)
            {
                sum += heightmaps[j][i] * (j * contribution + 1);
            }

            heightmapCombined.Add(sum);
        }

        
        float highest = 0;
        float lowest = 10000;

        for (int i = 0; i < x; i++)
        {
            if (heightmapCombined[i] > highest) highest = heightmapCombined[i];
            if (heightmapCombined[i] < lowest) lowest = heightmapCombined[i];
        }

        
        for (int i = 0; i < x; i++)
        {
            heightmapCombined[i] = Mathf.InverseLerp(lowest, highest, heightmapCombined[i]);
        }
               
        

        
        meshgen.generateMesh(heightmapCombined);

    }

    public void GenerateCubeMarching(MeshGenerator meshgen)
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

        initializeConstants(1);

        float[,,] densityMap = generateDensityMap(x, x, x, startwerte[0].x, startwerte[0].y, startwerte[0].z, 0.05f);

        meshgen.generateMeshCubeMarching(densityMap);
    }

    public float[,,] generateDensityMap(int x, int y, int z, float startx, float starty, float startz, float scale)
    {
        float[,,] densityMap = new float[x, y, z];

        Vector3 center = new Vector3(x/2f, y/2f, z/2f);
        float maxDistance = Vector3.Distance(center, new Vector3(0, 0, 0));

        for(int i = 0; i<x; i++)
        {
            for(int j = 0; j<y; j++)
            {
                for(int k = 0; k<z; k++)
                {
                    densityMap[i, j, k] = ((1 - (Vector3.Distance(new Vector3(i, j, k), center) / maxDistance))*1.3f + Noise(startx + i*scale, starty + j*scale, startz + k*scale) * 0.5f)*0.8f;
                }
            }
        }

        return densityMap;
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

    static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    static float Grad(int hash, float x)
    {
        return (hash & 1) == 0 ? x : -x;
    }

    static float Grad(int hash, float x, float y)
    {
        return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
    }

    static float Grad(int hash, float x, float y, float z)
    {
        var h = hash & 15;
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
    public static float Noise(float x, float y, float z)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        var Z = Mathf.FloorToInt(z) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);
        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);
        var A = (perm[X] + Y) & 0xff;
        var B = (perm[X + 1] + Y) & 0xff;
        var AA = (perm[A] + Z) & 0xff;
        var BA = (perm[B] + Z) & 0xff;
        var AB = (perm[A + 1] + Z) & 0xff;
        var BB = (perm[B + 1] + Z) & 0xff;
        return Lerp(w, Lerp(v, Lerp(u, Grad(perm[AA], x, y, z), Grad(perm[BA], x - 1, y, z)),
                               Lerp(u, Grad(perm[AB], x, y - 1, z), Grad(perm[BB], x - 1, y - 1, z))),
                       Lerp(v, Lerp(u, Grad(perm[AA + 1], x, y, z - 1), Grad(perm[BA + 1], x - 1, y, z - 1)),
                               Lerp(u, Grad(perm[AB + 1], x, y - 1, z - 1), Grad(perm[BB + 1], x - 1, y - 1, z - 1))));
    }

    static int[] perm = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
        151
    };
}
