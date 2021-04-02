using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


public class GeneticAlgorithm : MonoBehaviour
{
    public static int[] shelvesChecked;
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
    private static int[,] _parentIndexes;
    public static int[,] FitnessScores;

    private void Start()
    {
        //Debug.Log(shelvesChecked[0]);
        totalShelves = populationSize * oneLengthShelvesCount;

        _layouts = new int[maxGenerations, populationSize, width, height];
        _oneDLayouts = new int[maxGenerations, populationSize, width * height];
        _parentIndexes = new int[maxGenerations, populationSize*2];
        FitnessScores = new int[maxGenerations, populationSize];
        shelvesChecked = new int[maxGenerations];


        //CreateFitnessScores(FitnessScores);

        GenerateNewLayout();
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


    private void GenerateNewLayout()
    {
        for (var generation = 0; generation < maxGenerations; generation++)
        {
            for (var individual = 0; individual < populationSize; individual++)
            {

                for (var x = 0;
                    x < width;
                    x++) //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
                for (var y = 0; y < height; y++)
                {
                    _layouts[generation, individual, x, y] = 200000 + x + y * width;
                    _oneDLayouts[generation, individual, y + x * width] = 0;
                    FitnessScores[generation, individual] = 0;
                    shelvesChecked[generation] = 0;

                }

            }
        }

        for (int generation = 0; generation < maxGenerations; generation++)
        {
            
        }
        //Debug.Log(_oneDLayouts[1, 1, 1]);
        //Debug.Log(Layouts[2, 2, 0, 0]);


        for (var individual = 0; individual < populationSize; individual++)
        for (var x = 0; x < oneLengthShelvesCount; x++) //Randomise Positions of Shelves and add to 2d array
        {
            var posx = Random.Range(0, width);
            var posy = Random.Range(0, height);

            while (_layouts[0, individual, posx, posy] < 200000) //find a new position for shelf if not on pathv 
            {
                posx = Random.Range(0, width);
                posy = Random.Range(0, height);
            }

            _layouts[0, individual, posx, posy] = 10000+x;
        }
        
        //fixing shelf and path numbering
        
        for (var individual = 0; individual < populationSize; individual++)
        {
            int shelves = 0;
            int paths = 0;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //Debug.Log(_layouts[0, individual, x, y]);
                    if (_layouts[0, individual, x, y] >= 200000)
                    {
                        _layouts[0, individual, x, y] = 200000 + paths;
                        paths++;
                        //Debug.Log("paths: " + paths);
                    }
                    else if (_layouts[0, individual, x, y] < 200000)
                    {
                        _layouts[0, individual, x, y] = 10000 + shelves;
                        shelves++;
                        //Debug.Log("Shelves:" + shelves);
                    }
                }
            }
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
            if (oneDLayouts[generation, individual, x + z*width] >= 200000)    //paths are denoted by 200,000+ values
            {
                var path = Instantiate(this.path, new Vector3(x + generation * (2*width + 2), 0, z + individual * (2*width + 2)),
                    Quaternion.identity);
                path.name = oneDLayouts[generation, individual, x + z*width].ToString();
                path.transform.parent = GameObject.Find(individual.ToString()).transform;
            }
            else if (oneDLayouts[generation, individual, x + z*width] < 200000) //shelves are denoted by <200,000 values
            {
                var shelf = Instantiate(oneShelf, new Vector3(x + generation * (2*width + 2), 0.5f, z + individual * (2*width + 2)),
                    Quaternion.identity);
                shelf.name = oneDLayouts[generation, individual, x + z*width].ToString();
                shelf.transform.parent = GameObject.Find(individual.ToString()).transform;
                
                //Shelf.transform.parent.transform.position = new Vector3(individual * 20, 0, 0);
            }

        for (var x = 0; x < oneDLayouts.Length; x++)
        {
            //Debug.Log(OneDLayouts[generation, individual,x]);
        }
    }

    private void ChooseParents(int[,] fitnessScores, int generation, int individual)
    {
        //Debug.Log(FitnessScores[0,0]);
        var generationFitness = 0F;
        var parentNum = -1;
        var scores = new Dictionary<int, float>();
        Dictionary<int,int> sortedScores = new Dictionary<int, int>();
        for (var ind = 0; ind < populationSize; ind++)
        {
            float temp = (float) FitnessScores[generation, ind];
            if (temp == 0)
            {
                temp = 0.001F;
            }
           //Debug.Log(FitnessScores[generation, ind]);
            
            //Debug.Log(temp + "  <<temp");
            
            generationFitness += 1/temp;
            
            scores.Add( ind, 1/temp);
        }

        var fitnessThreshold = Random.Range(0, generationFitness);
        var fitnessCount = 0F;
        
        
        foreach (KeyValuePair<int, float> Fitness in scores.OrderByDescending(key => key.Value))
        {
            //Debug.Log(Fitness.Key + "," + Fitness.Value);
            fitnessCount += Fitness.Value;
            if (fitnessCount > fitnessThreshold && parentNum== -1)
            {
                parentNum = Fitness.Key;
            }

        }  
        
        //Debug.Log("parent: " + parentNum + "    FitnessThreshold: " + fitnessThreshold + "  FitnessCount: " + fitnessCount);
        _parentIndexes[generation, individual] = parentNum;
    }

    private void Pmx(int[,,] oneDLayouts, int parent1Num, int parent2Num, int generation, int individual) //Partial-Mapped Crossover
    {
        var parent1 = new int[width * height];
        var parent2 = new int[width * height];
        
        int[] child1 = new int[width * height];
        int[] child2 = new int[width * height];

        Dictionary<int, int> RelationshipsOneAsKey = new Dictionary<int, int>();
        Dictionary<int, int> RelationshipsTwoAsKey = new Dictionary<int, int>();
        
       //Debug.Log(parent1Num);
        //Debug.Log(parent2Num);
        
        //Debug.Log("------------------------Start of Parents ------------------------------");
       //Debug.Log(parent1Num + "                        " + parent2Num);
        
        
        for (var x = 0; x < width*height; x++)
        {
            //Debug.Log(x + "," + generation + "," + parent1Num);
            parent1[x] = oneDLayouts[generation, parent1Num, x];
            parent2[x] = oneDLayouts[generation, parent2Num, x];
            
            //Debug.Log("parent1[x]:" + parent1[x] + "  parent2[x]:" + parent2[x]);

            child1[x] = 0;
            child2[x] = 0;
        }
        
        //Debug.Log("-------------------------End of Parents -------------------------------");
        
        
        
        
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
        
        //Debug.Log(crossOverPoint1 + " " + crossOverPoint2);
        
        //Debug.Log("----------------------Start of Crossover----------------------");

        for (int x = crossOverPoint1; x < crossOverPoint2; ++x)
        {
            child1[x] = parent2[x];
            child2[x] = parent1[x];

            RelationshipsOneAsKey.Add(parent1[x], parent2[x]);
            RelationshipsTwoAsKey.Add(parent2[x], parent1[x]);
        }
            
        for (int x = 0; x < width*height; x++)
        {
            if (x < crossOverPoint2 && x >= crossOverPoint1)
            {
                //Debug.Log("in the middle");
            }
            else
            {
                //Debug.Log("on the outside");
                if (child1.Contains(parent1[x]))
                {
                    int key = parent1[x];

                    while (child1.Contains(key))
                    {
                        key = RelationshipsTwoAsKey[
                            key];  
                        
                        //Debug.Log("parent1[x]>>>>" + parent1[x] + "child1[x]>>>> " + key);
                    }


                    child1[x] = key;


                    //this might need to get value from relationshipstwoaskey instead

                }
                else
                {
                    child1[x] = parent1[x];
                }

               // if (child2.Contains(parent2[x]))
              //  {
                   // child2[x] = RelationshipsOneAsKey[
                      //  parent2[x]]; //this might need to get value from relationshipstwoaskey instead
                


            }
            //Debug.Log(child1[x] + "      " + parent1[x] + "      " + parent2[x]);

        }
        
        //Debug.Log("-------------------------Start of Child -------------------------------");
        
        for (int x = 0; x < width*height; x++)
        {
            oneDLayouts[generation + 1, individual, x] = child1[x];
                //Debug.Log("Child1[x]: " + child1[x]);
            }
        
        
        //Debug.Log("-------------------------End of Child -------------------------------");

    
    }

    void SwapMutation(int generation, int individual)
    {

            int x = Random.Range(0, width * height);
            int y = Random.Range(0, width * height);

            int temp = _oneDLayouts[generation + 1, individual, x];

            _oneDLayouts[generation + 1, individual, x] = _oneDLayouts[generation + 1, individual, y];
            _oneDLayouts[generation + 1, individual, y] = temp;
        }
    

    public void Run(int generation)
    {
        int averageFitness = 0;
        if (generation > 1)
        {
            for (int individual = 0; individual < populationSize; individual++)
            {
                averageFitness += FitnessScores[generation - 1, individual];
            }
        }

        Debug.Log(averageFitness);
        //Debug.Log();
        for (int individual = 0; individual < populationSize*2; individual++)   //2 parents for each individual
        {
            ChooseParents(FitnessScores, generation, individual);
        }
        
        for (int individual = 0; individual < populationSize; individual++)   //2 parents for each individual
        {
            //Debug.Log(_parentIndexes[generation, individual] + "" + _parentIndexes[generation, individual+populationSize]);
            Pmx(_oneDLayouts, _parentIndexes[generation,individual], _parentIndexes[generation,individual+1], generation, individual);
            //Destroy(GameObject.Find(individual.ToString()));
           // var level = new GameObject();
            //level.transform.position = new Vector3(0, 0, 0);
           // level.name = individual.ToString();
           //SwapMutation(generation, individual);
            DisplayLayout(_oneDLayouts,_layouts,individual, generation+1);
        }
        //GenerateChildren(generation); //generate children from previous generation
        
        

        //FindBestLayout(_layouts, _oneDLayouts, generation + 1, FitnessScores); //then build these children
    }


    private int[,,,] GenerateChildren(int generation)
    {
        var bestLayout = 0;
        var bestFitness = int.MaxValue;
        for (var individual = 0; individual < populationSize; individual++)
            //Debug.Log(generation + "," + individual);
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