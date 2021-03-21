using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public int Width;
    public int Height;
    public int OneLengthShelvesCount;
    public int PopulationSize;
    public GameObject OneShelf;
    public GameObject Path;

    public int[] FitnessScores;


    int[,,] GenerateNewLayout(int[,,] Layouts, int individual) {

        for (int x = 0; x < Width; x++)     //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
        {
            for (int y = 0; y < Height; y++)
            {
                Layouts[individual, x, y] = 0;
            }
        }

        for (int x = 0; x < OneLengthShelvesCount; x++)     //Randomise Positions of Shelves and add to 2d array
        {
            int posx = Random.Range(0, Width);
            int posy = Random.Range(0, Height);

            while (Layouts[individual,posx, posy] != 0)      //find a new position for shelf if not on pathv 
            {
                posx = Random.Range(0, Width);
                posy = Random.Range(0, Height);
            }

            Layouts[individual, posx, posy] = 1;
        }

        return Layouts;
    }

    int[,] ConvertToFlatArray(int[,,] Layouts, int[,] OneDLayouts, int individual)
    {
        for (int x = 0; x < Width; ++x)     //Make 2d array into 1d for combination
        {
            for (int y = 0; y < Height; ++y)
            {
                //ebug.Log(x + "," + y);
                //Debug.Log(x + "," + y + "=" + Layouts[individual, x, y]);
                OneDLayouts[individual,(x+(y*Width))] = Layouts[individual,x, y];
                
            }
        }

        return OneDLayouts; 
    }

    void Start()    //Shop Floor is randomly generated.
    {

        




        
    
       

    }

    void FindBestLayout(int[,,] Layouts, int[,] OneDLayouts)
    {

        for(int individual = 0; individual < PopulationSize; individual++)
        {
            FitnessScores[individual] = 0;
            var Level = new GameObject();
            Level.name = individual.ToString();
            GenerateNewLayout(Layouts, individual);
            ConvertToFlatArray(Layouts, OneDLayouts, individual);
            DisplayLayout(OneDLayouts, Layouts, individual);
        }
    }

    void DisplayLayout(int[,] OneDLayouts, int[,,] Layouts, int individual)
    {

        //Debug.Log(individual);

        for (int x = 0; x < Width; x++)     //Place Paths and Shelves Appropriately
        {
            for (int z = 0; z < Height; z++)
            {
                if (Layouts[individual,x, z] == 0)
                {
                    var path = Instantiate(Path, new Vector3(x, 0, z), Quaternion.identity);
                    path.transform.parent = GameObject.Find(individual.ToString()).transform;
                }
                else if (Layouts[individual,x, z] == 1)
                {
                    var Shelf = Instantiate(OneShelf, new Vector3(x, 0.5f, z), Quaternion.identity);
                    Shelf.transform.parent = GameObject.Find(individual.ToString()).transform;
                }


            }
        }

        for (int x = 0; x < OneDLayouts.Length; x++)
        {
            //Debug.Log(OneDLayouts[individual,x]);
        }
    }

    void Update()
    {
        if (Time.frameCount == 1)
        {
            int[,,] Layouts = new int[PopulationSize, Width, Height];
            int[,] OneDLayouts = new int[PopulationSize, Width * Height];
            FitnessScores = new int[PopulationSize];
            FindBestLayout(Layouts, OneDLayouts);
        }

    }
}
