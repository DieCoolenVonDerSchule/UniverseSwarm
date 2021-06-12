using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    float[,] heightMap;
    int[] triangles;
    Color[] colors;
    public Gradient gradient;
    public ComputeShader shader;
    public float radius;

    struct SquareInfo
    {
        int vert;
        int x;

        public SquareInfo(int vert, int x)
        {
            this.vert = vert;
            this.x = x;
        }
    };

    // Start is called before the first frame update
    void Start()
    {
        
            foreach (DecorationFunctions deco in GetComponents<DecorationFunctions>())
            {

                deco.inistializePlants();

            }


        


    }

    public void generateMesh(float[,] map)
    {
        Destroy(GetComponent<MeshFilter>().mesh);

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;   

        heightMap = map;
       
        createShape();
        updateMesh();
    }

    void createShape()
    {
        int x = heightMap.GetLength(0)-1;
        int y = heightMap.GetLength(1)-1;
        vertices = new Vector3[(x+1) * (y+1)];


        string outputInput = GameObject.FindGameObjectWithTag("output").
            GetComponent<UnityEngine.UI.InputField>().text;

        float output = float.Parse(outputInput);



        

        for (int k = 0, i = 0; i < y + 1; i++)
        {
            for (int j = 0; j < x + 1; j++)
            {
                float vecx = Mathf.Cos(Mathf.PI * 2 / x * j) * (heightMap[j, i] + radius);
                float vecz = Mathf.Sin(Mathf.PI * 2 / x * j) * -(heightMap[j, i] + radius) * Mathf.Sin(Mathf.PI * 2 / y * i);
                float vecy = Mathf.Sin(Mathf.PI * 2 / x * j) * (heightMap[j, i] + radius) * Mathf.Cos(Mathf.PI * 2 / y * i);




                vertices[k] = new Vector3(vecx, vecy, vecz);

                k++;
            }
        }

        colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = gradient.Evaluate(vertices[i].y);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = vertices[i].y * output;
        }

     

        triangles = new int[x * y * 6];
        int vert = 0;

       
                

        SquareInfo[] data = new SquareInfo[x * y];

        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {

                data[i*x+j] = new SquareInfo(vert, x);

                vert++;
            }

            vert++;
        }


        ComputeBuffer buffer = new ComputeBuffer(data.Length, 8);
        buffer.SetData(data);
        ComputeBuffer outputBuffer = new ComputeBuffer(triangles.Length, 4);
        outputBuffer.SetData(triangles);

        int kernel = shader.FindKernel("generateMesh");
        shader.SetBuffer(kernel, "dataBuffer", buffer);
        shader.SetBuffer(kernel, "output", outputBuffer);
        shader.Dispatch(kernel, data.Length / 32, 1, 1);
        outputBuffer.GetData(triangles);

        buffer.Dispose();
        outputBuffer.Dispose();


     
        uvs = new Vector2[vertices.Length];


        for (int i = 0, k = 0; i < y; i++)
        {

            for (int j = 0; j < x; j++)
            {
                uvs[k] = new Vector2((float)j/(x*0.01f), (float)i/(y*0.01f));
                k++;

            }
        }


        if (GameObject.FindGameObjectWithTag("debugtoggle").
            GetComponent<UnityEngine.UI.Toggle>().isOn) print("vert: " + vert);
    }

    void updateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        if (GameObject.FindGameObjectWithTag("debugtoggle").GetComponent<UnityEngine.UI.Toggle>().isOn) print("MESH BERECHNET");

        
    }


    public void destroyAllPlants()
    {
        print("DECO COUNT:" + GetComponents<DecorationFunctions>().Length);
        foreach(DecorationFunctions deco in GetComponents<DecorationFunctions>())
        {

            deco.destroyPlants();
            
        }
    }
}
