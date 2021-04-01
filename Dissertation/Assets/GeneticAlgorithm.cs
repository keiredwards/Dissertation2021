using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GeneticAlgorithm : MonoBehaviour
{
    public static int[] shelvesChecked = new int[3];
    [FormerlySerializedAs("Width")] public int width;
    [FormerlySerializedAs("Height")] public int height;

    [FormerlySerializedAs("OneLengthShelvesCount")]
    public int oneLengthShelvesCount;

    [FormerlySerializedAs("TotalShelves")] public int totalShelves;

    [FormerlySerializedAs("PopulationSize")]
    public int populationSize;

    [FormerlySerializedAs("MaxGenerations")]
    public int maxGenerations;

    [FormerlySerializedAs("OneShelf")] public GameObject oneShelf;
    [FormerlySerializedAs("Path")] public GameObject path;
    public int generation;
    
    private static int[,,,] _layouts;
    private static int[,,] _oneDLayouts;
    public int[,] FitnessScores = new int[10, 10];

    private void Start()
    {
        //Debug.Log(shelvesChecked[0]);
        totalShelves = populationSize * oneLengthShelvesCount;

        _layouts = new int[maxGenerations, populationSize, width, height];
        _oneDLayouts = new int[maxGenerations, populationSize, width * height];


        CreateFitnessScores(FitnessScores);

        GenerateNewLayout(FitnessScores);
        FindBestLayout(_layouts, _oneDLayouts, 0, FitnessScores);
    }

    private void Update()
    {
        //Debug.Log(FitnessScores[3, 2]);
        //Debug.Log(shelvesChecked[2]);
    }


    private static void Main(string[] args)
    {
        for (var gen = 0; gen < shelvesChecked.Length; gen++) shelvesChecked[gen] = 0;
    }

    private int[,] CreateFitnessScores(int[,] fitnessScores)
    {
        for (var gen = 0; gen < maxGenerations; gen++)
        for (var ind = 0; ind < populationSize; ind++)
        {
            fitnessScores[gen, ind] = new int();
            fitnessScores[gen, ind] = 0;
        }

        return fitnessScores;
    }


    private void GenerateNewLayout(int [,] FitnessScores)
    {
        for (var generation = 0; generation < maxGenerations; generation++)
        for (var individual = 0; individual < populationSize; individual++)
        for (var x = 0; x < width; x++) //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
        for (var y = 0; y < height; y++)
        {
            _layouts[generation, individual, x, y] = 100000 + x + y * width;
            _oneDLayouts[generation, individual, y + x * width] = 0;
        }

        
        //Debug.Log(_oneDLayouts[1, 1, 1]);
        //Debug.Log(Layouts[2, 2, 0, 0]);


        for (var individual = 0; individual < populationSize; individual++)
        for (var x = 0; x < oneLengthShelvesCount; x++) //Randomise Positions of Shelves and add to 2d array
        {
            var posx = Random.Range(0, width);
            var posy = Random.Range(0, height);

            while (_layouts[0, individual, posx, posy] < 100000) //find a new position for shelf if not on pathv 
            {
                posx = Random.Range(0, width);
                posy = Random.Range(0, height);
            }

            _layouts[0, individual, posx, posy] = 10000+x;
        }
        
    }

    private int[,,] ConvertToFlatArray(int[,,,] layouts, int[,,] oneDLayouts, int individual, int generation)
    {
        for (var x = 0; x < width; x++) //Make 2d array into 1d for combination
        for (var y = 0; y < height; y++)
            //ebug.Log(x + "," + y);
            //Debug.Log(x + "," + y + "=" + Layouts[individual, x, y]);
            //OneDLayouts[generation, individual,(x+(y*Width))] = 0;
            oneDLayouts[generation, individual, x + y * width] = _layouts[generation, individual, x, y];

        return oneDLayouts;
    }


    private void FindBestLayout(int[,,,] layouts, int[,,] oneDLayouts, int generation, int[,] fitnessScores)
    {
        for (var individual = 0; individual < populationSize; individual++)
        {
            //Debug.Log("gen" + generation);
            fitnessScores[generation, individual] = 0;

            var level = new GameObject();
            level.transform.position = new Vector3(0, 0, 0);
            level.name = individual.ToString();


            ConvertToFlatArray(layouts, oneDLayouts, individual, generation);
            DisplayLayout(oneDLayouts, _layouts, individual, generation);
        }
    }

    private void DisplayLayout(int[,,] oneDLayouts, int[,,,] layouts, int individual, int generation)
    {
        //Debug.Log(generation + "" + individual);

        for (var x = 0; x < width; x++) //Place Paths and Shelves Appropriately
        for (var z = 0; z < height; z++)
            if (layouts[generation, individual, x, z] >= 100000)    //paths are denoted by 100,000+ values
            {
                var path = Instantiate(this.path, new Vector3(x + generation * 10, 0, z + individual * 10),
                    Quaternion.identity);
                path.name = _layouts[generation, individual, x, z].ToString();
                path.transform.parent = GameObject.Find(individual.ToString()).transform;
            }
            else if (layouts[generation, individual, x, z] < 100000) //shelves are denoted by <100,000 values
            {
                var shelf = Instantiate(oneShelf, new Vector3(generation * 10 + x, 0.5f, z + individual * 10),
                    Quaternion.identity);
                shelf.name = _layouts[generation, individual, x, z].ToString();
                shelf.transform.parent = GameObject.Find(individual.ToString()).transform;
                
                //Shelf.transform.parent.transform.position = new Vector3(individual * 20, 0, 0);
            }

        for (var x = 0; x < oneDLayouts.Length; x++)
        {
            //Debug.Log(OneDLayouts[generation, individual,x]);
        }
    }

    private void ChooseParents(int[,] fitnessScores, int generation)
    {
        //Debug.Log(FitnessScores[0,0]);
        var generationFitness = 0F;
        var parentNum = -1;
        var scores = new Dictionary<int, float>();
        Dictionary<int,int> sortedScores = new Dictionary<int, int>();
        for (var individual = 0; individual < populationSize; individual++)
        {
            float temp = (float) FitnessScores[generation, individual];
           // Debug.Log(FitnessScores[generation, individual]);
            
            //Debug.Log(1/temp);
            
            generationFitness += 1/temp;
            
            scores.Add( individual, 1/temp);
        }

        var fitnessThreshold = Random.Range(0, generationFitness);
        var fitnessCount = 0F;
        
        
        foreach (KeyValuePair<int, float> Fitness in scores.OrderByDescending(key => key.Value))
        {
            Debug.Log(Fitness.Key + "," + Fitness.Value);
            fitnessCount += Fitness.Value;
            if (fitnessCount > fitnessThreshold && parentNum== -1)
            {
                parentNum = Fitness.Key;
            }

        }  
        
        Debug.Log("parent: " + parentNum + "    FitnessThreshold: " + fitnessThreshold + "  FitnessCount: " + fitnessCount);
    }

    private void Pmx(int[,,] oneDLayouts, int parent1Num, int parent2Num, int generation) //Partial-Mapped Crossover
    {
        var parent1 = new int[width * height];
        var parent2 = new int[width * height];
        
        int[] child1 = new int[width * height];
        int[] child2 = new int[width * height];

        Dictionary<int, int> RelationshipsOneAsKey = new Dictionary<int, int>();
        Dictionary<int, int> RelationshipsTwoAsKey = new Dictionary<int, int>();
        

        for (var x = 0; x < width*height; x++)
        {
            parent1[x] = oneDLayouts[generation, parent1Num, x];
            parent2[x] = oneDLayouts[generation, parent2Num, x];

            child1[x] = 0;
            child2[x] = 0;
        }

        int crossOverPoint1 = 0;
        int crossOverPoint2 = 0;

        while (crossOverPoint1 == crossOverPoint2)  //generate two points to cut gene that arent the same point
        {
            crossOverPoint1 = Random.Range(0, parent1.Length+1);    //cut points are before selected index. i.e index 3:
            crossOverPoint2 = Random.Range(0, parent2.Length+1);    // 012|345
        }
         

        if (crossOverPoint1 > crossOverPoint2) //Swaps over so 1 is always smaller
        {
            var temp = crossOverPoint1;
            crossOverPoint1 = crossOverPoint2;
            crossOverPoint2 = temp;
        }


        for (int x = crossOverPoint1; x < crossOverPoint2; ++x)
        {
            child1[x] = parent2[x];
            child2[x] = parent1[x];

            RelationshipsOneAsKey.Add(parent1[x], parent2[x]);
            RelationshipsTwoAsKey.Add(parent2[x], parent1[x]);
        }

        for (int x = 0; x < crossOverPoint1; ++x)
        {
            if (child1.Contains(parent1[x]))
            {
                child1[x] = RelationshipsOneAsKey[
                    parent1[x]];     //this might need to get value from relationshipstwoaskey instead

            }
            else
            {
                child1[x] = parent1[x];
            }
            
            if (child2.Contains(parent2[x]))
            {
                child2[x] = RelationshipsTwoAsKey[
                    parent2[x]];    //this might need to get value from relationshipstwoaskey instead
            }
            else
            {
                child1[x] = parent1[x];
            }
            
        }


    }

    public void Run(int generation)
    {
        ChooseParents(FitnessScores, generation);
        //GenerateChildren(generation); //generate children from previous generation


        FindBestLayout(_layouts, _oneDLayouts, generation + 1, FitnessScores); //then build these children
    }


    private int[,,,] GenerateChildren(int generation)
    {
        var bestLayout = 0;
        var bestFitness = int.MaxValue;
        for (var individual = 0; individual < populationSize; individual++)
            // Debug.Log(generation + "," + individual);
            //Debug.Log(FitnessScores[generation, individual] + "GenInd" + generation + individual);
            if (FitnessScores[generation, individual] < bestFitness)
            {
                bestFitness = FitnessScores[generation, individual];
                bestLayout = individual;
            }


        if (generation < maxGenerations - 1)
            for (var individual = 0; individual < populationSize; individual++)
            for (var x = 0; x < width; x++)
            for (var z = 0; z < height; z++)
                //Debug.Log(generation);
                _layouts[generation + 1, individual, x, z] = _layouts[generation, bestLayout, x, z];
        return _layouts;
    }
}