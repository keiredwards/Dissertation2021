using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public int Width;
    public int Height;
    public int OneLengthShelvesCount;
    public int PopulationSize;
    public int MaxGenerations;

    public GameObject OneShelf;
    public GameObject Path;
    public int generation;

    public int[,,,] Layouts;
    public int[,,] OneDLayouts;

    public int[,] FitnessScores;


    int[,,,] GenerateNewLayout(int[,,,] Layouts)
    {


        for (int generation = 0; generation < MaxGenerations; generation++) {
            for (int individual = 0; individual < PopulationSize; individual++)
            {
                for (int x = 0; x < Width; x++)     //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Layouts[generation, individual, x, y] = 0;
                    }
                }
            }
        }

        //Debug.Log(Layouts[1, 1, 1, 1]);
        //Debug.Log(Layouts[2, 2, 0, 0]);


        for (int individual = 0; individual < PopulationSize; individual++)
        {
            for (int x = 0; x < OneLengthShelvesCount; x++)     //Randomise Positions of Shelves and add to 2d array
            {
                int posx = Random.Range(0, Width);
                int posy = Random.Range(0, Height);

                while (Layouts[0, individual, posx, posy] != 0)      //find a new position for shelf if not on pathv 
                {
                    posx = Random.Range(0, Width);
                    posy = Random.Range(0, Height);
                }

                Layouts[0, individual, posx, posy] = 1;
            }
        }
    

        return Layouts;
    }

    int[,,] ConvertToFlatArray(int[,,,] Layouts, int[,,] OneDLayouts, int individual, int generation)
    {
        for (int x = 0; x < Width; x++)     //Make 2d array into 1d for combination
        {
            for (int y = 0; y < Height; y++)
            {
                //ebug.Log(x + "," + y);
                //Debug.Log(x + "," + y + "=" + Layouts[individual, x, y]);
                //OneDLayouts[generation, individual,(x+(y*Width))] = 0;
                OneDLayouts[generation, individual, (x + (y * Width))] = Layouts[generation, individual, x, y];
            }
        }

        return OneDLayouts;
    }

    void Start()    //Shop Floor is randomly generated.
    {
        Layouts = new int[MaxGenerations, PopulationSize, Width, Height];
        OneDLayouts = new int[MaxGenerations, PopulationSize, Width * Height];
        FitnessScores = new int[MaxGenerations, PopulationSize];

        run();
    }

    void FindBestLayout(int[,,,] Layouts, int[,,] OneDLayouts, int generation, int[,] FitnessScores)
    {

        for (int individual = 0; individual < PopulationSize; individual++)
        {
            FitnessScores[generation, individual] = 0;
            
            var Level = new GameObject();
            Level.transform.position = new Vector3(0, 0, 0);
            Level.name = individual.ToString();


            ConvertToFlatArray(Layouts, OneDLayouts, individual, generation);
            DisplayLayout(OneDLayouts, Layouts, individual, generation);

        }
    }

    void DisplayLayout(int[,,] OneDLayouts, int[,,,] Layouts, int individual, int generation)
    {

        //Debug.Log(generation + "" + individual);

        for (int x = 0; x < Width; x++)     //Place Paths and Shelves Appropriately
        {
            for (int z = 0; z < Height; z++)
            {
                if (Layouts[generation, individual, x, z] == 0)
                {
                    var path = Instantiate(Path, new Vector3(x+generation*10, 0, z + individual * 10), Quaternion.identity);
                    path.transform.parent = GameObject.Find(individual.ToString()).transform;
                }
                else if (Layouts[generation, individual, x, z] == 1)
                {
                    var Shelf = Instantiate(OneShelf, new Vector3(generation*10+x, 0.5f, z + individual * 10), Quaternion.identity);
                    Shelf.transform.parent = GameObject.Find(individual.ToString()).transform;
                    //Shelf.transform.parent.transform.position = new Vector3(individual * 20, 0, 0);
                }


            }
        }

        for (int x = 0; x < OneDLayouts.Length; x++)
        {
            //Debug.Log(OneDLayouts[generation, individual,x]);
        }
    }

    void ShowFitness(int individual, int generation)
    {
        //Debug.Log(FitnessScores[generation, individual]);
    }

    void OnePointCrossOver(int[,] OneDLayouts, int parent1, int parent2, int generation)
    {
        int CrossOverPoint = Random.Range(0, OneDLayouts.GetLength(parent1));    //choose a point to cut the gene
        //Debug.Log(CrossOverPoint);


    }

    void Update()
    {



    }

    void run()
    {
        GenerateNewLayout(Layouts);

        for (int generation = 0; generation < MaxGenerations; generation++)
        {
           // Debug.Log(generation);
            FindBestLayout(Layouts, OneDLayouts, generation, FitnessScores);
            
            GenerateChildren(generation);
            
        }
    }

    int[,,,] GenerateChildren(int generation)
    {
     
        int bestLayout = 0;
        int bestFitness = int.MaxValue;
        for (int individual = 0; individual < PopulationSize; individual++)
        {
            // Debug.Log(generation + "," + individual);
            //Debug.Log(FitnessScores[generation, individual] + "GenInd" + generation + individual);
            if (FitnessScores[generation, individual] < bestFitness)
            {
                bestFitness = FitnessScores[generation, individual];
                bestLayout = individual;
            }
        }


        if (generation < MaxGenerations - 1)
        {
            for (int individual = 0; individual < PopulationSize; individual++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int z = 0; z < Height; z++)
                    {
                        //Debug.Log(generation);
                        Layouts[generation + 1, individual, x, z] = Layouts[generation, bestLayout, x, z];
                    }
                }

            }
        }
            return Layouts;




    }
}
