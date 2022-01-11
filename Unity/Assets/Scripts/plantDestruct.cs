using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plantDestruct : MonoBehaviour
{
    

    public void plantDestruction()
    {

        foreach (MeshGenerator meshgens in Movement.meshGenerators)
        {
            foreach (DecorationFunctions deco in meshgens.GetComponents<DecorationFunctions>())
            {

                foreach (GameObject plant in deco.plantList)
                {
                    Destroy(plant);
                }
            }

        }

    }
}
