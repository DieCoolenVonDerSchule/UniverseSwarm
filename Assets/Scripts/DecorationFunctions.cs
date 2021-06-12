using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//[RequireComponent(typeof(MeshGenerator))]

public class DecorationFunctions : MonoBehaviour
{

    public GameObject toPlace;

   
    public ArrayList plantList;

    public float scale;
    public float plantHeightMin;
    public float plantHeightMax;
    public float occurance;
    public float plantSize;
    public float probalilityThreshold;





    // Start is called before the first frame update
    void Start()
    {
        
       

    }



    public void inistializePlants()
    {
        
        
        print("PLANT START");


        try
        {
            print("PLANTLIST COUNT (INIT) VORHER: " + this.plantList.Count);


        }

        catch(System.Exception e)
        {
            print("LISTE WAR NULL "+e);
        }
      
        if (this.plantList == null) this.plantList = new ArrayList();
        print("PLANTLIST COUNT (INIT) NACHHER: " + this.plantList.Count);





    }








    public void placePlants (float[,] heightmap)
    {
        int x = heightmap.GetLength(0);
        int y = heightmap.GetLength(1);

        float startx = Random.Range(0, 1000);
        float starty = Random.Range(0, 1000);

        float[,] probabilityDistribution = GameObject.FindGameObjectWithTag("heightmapbutton").
            GetComponent<GenerationFunctions>().createHeightMapPerlinNoiseCS(x, y, scale, startx, starty, 0, 0);

        float max = 0f;
        float min = 1000f;


        for (int i=0; i<x; i++)
        {
            for (int j=0; j<y; j++)
            {

                if (probabilityDistribution[i, j]>max) max=probabilityDistribution[i,j];
                if (probabilityDistribution[i, j]<min) min=probabilityDistribution[i,j];
            }
        }


        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {

                probabilityDistribution[i, j] = Mathf.InverseLerp(min,max,probabilityDistribution[i,j]);
            }
        }


        for (int i=0; i<x; i++)
        {
            for (int j=0; j<y; j++)
            {
                if (heightmap[i,j] > plantHeightMin && heightmap[i,j] < plantHeightMax && 
                    probabilityDistribution[i,j] > probalilityThreshold)
                {
                    if (Random.value < (1 - Mathf.Pow(heightmap[i, j], 0.25f)) * 
                        Mathf.Pow(probabilityDistribution[i, j], 2) * occurance)
                    { putPlant(i, j, heightmap[i, j], GetComponent<MeshGenerator>()); }    
                }
            }
        }
    }


    void putPlant(int x, int y, float height, MeshGenerator meshgen)
    {

        string outputStr = GameObject.FindGameObjectWithTag("output").
            GetComponent<UnityEngine.UI.InputField>().text;

        float output = float.Parse(outputStr);


        float offsetX = meshgen.transform.position.x;
        float offsetHeight = meshgen.transform.position.y;
        float offsetY = meshgen.transform.position.z;

        float scaleX = meshgen.transform.localScale.x;
        float scaleHeight = meshgen.transform.localScale.y;
        float scaleY = meshgen.transform.localScale.z;


        toPlace.transform.localScale = new Vector3(0.5f*plantSize, 0.5f*plantSize, 0.5f);


        this.plantList.Add(Instantiate(toPlace, new Vector3
            (x * scaleX + offsetX, height * scaleHeight * output + offsetHeight, y * scaleY + offsetY), 
            Quaternion.Euler(0f, -90f+Random.Range(-20f, 20f), 0f)));
    }



    public void destroyPlants()
    {

        print("PLANT DESTROY");
        print("PLANTLIST SIZE VORHER: " + this.plantList.Count);

        foreach (GameObject plant in this.plantList)
        {
            
            Destroy(plant);
            print("PLANT TO DESTROY: "+plant);

        }

        plantList.Clear();
        

        print("PLANTLIST SIZE NACHHER: " + plantList.Count);

    }



}
