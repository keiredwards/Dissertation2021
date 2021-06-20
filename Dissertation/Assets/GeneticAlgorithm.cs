using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class GeneticAlgorithm : MonoBehaviour
{
    public static int[] shelvesChecked;
    public float MutationChance;
    public int width;
    private float bestLayout = Mathf.Infinity;
    private GameObject Camera;

    public bool SinglePointCrossOver;
    public bool PartiallyMappedCrossover;
    public bool RouletteWheel;
    public bool TruncatedSelect;
    public bool RandomSelect;
    public bool pathWidthCheck;
    private int bestLayoutIndividual;
    private int bestLayoutGeneration;
    public int SocialDistance;
    public float ParentSelectionPercentage;
    public float pathFitnessmult;
    public float pathWidthMult;

    public int height;

    public int oneLengthShelvesCount;
    public int totalShelves;

    public int populationSize;
    public int maxGenerations;

    public GameObject oneShelf;
    public GameObject path;
    public GameObject camShelf;
    public GameObject camPath;
    public GameObject PerimeterShelf;
    public int generation;

    private static int[,,] _oneDLayouts;
    private static int[,,] _parentIndexes;
    public static float[,] FitnessScores;

    bool genMax = false;
    private int checkPosition = 0;
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    private void Start()
    {
        generation = 0;
        totalShelves = populationSize * oneLengthShelvesCount;
        _oneDLayouts = new int[maxGenerations, populationSize, width * height];
        _parentIndexes = new int[maxGenerations, populationSize, 2];
        FitnessScores = new float[maxGenerations, populationSize];
        shelvesChecked = new int[maxGenerations];

        //Unity Best Layout Objects
        var level = new GameObject();
        level.transform.position = new Vector3(-100, 0, 0);
        level.name = "Best";
        
        generation = 0;

        GenerateNewLayout();
        FindBestLayout(_oneDLayouts, 0);
    }
    
    
    /// <summary>
    /// Controls the progression of the Genetic Algorithm. Each Generation occurs within a single frame.
    /// </summary>
    private void Update()
    {
        if (generation < maxGenerations - 1 && bestLayout > 0)
        {
            //First Update call happens after First Generation is processed in Start()
            generation++;
            Run(generation);
        }
        else if (generation == maxGenerations)
        {
            Debug.Log("HALT THERE!!");
        }
        else if (bestLayout == 0)
        {
            Debug.Log("VICTORY  " + generation);

        }
        GenerateBest(bestLayoutGeneration, bestLayoutIndividual);
    }

    /// <summary>
    /// Generate random layouts for initial population
    /// </summary>
    private void GenerateNewLayout()
    {
        for (var generation = 0; generation < maxGenerations; generation++)
        {
            for (var individual = 0; individual < populationSize; individual++)
            {
                _parentIndexes[generation, individual, 0] = 0;
                _parentIndexes[generation, individual, 1] = 0;
                for (var x = 0; x < height; x++){ //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
                for (var y = 0; y < width; y++)
                {
                    _oneDLayouts[generation, individual, y + x * width] = 200000; //sets all to path value
                    FitnessScores[generation, individual] = 0;
                    shelvesChecked[generation] = 0;
                }
            }
        }
        }
        
        for (var individual = 0; individual < populationSize; individual++)
        for (var x = 0; x < oneLengthShelvesCount; x++) //Randomise Positions of Shelves and add to 2d array
        {
            var posx = Random.Range(0, width);
            var posy = Random.Range(0, height);

            while (_oneDLayouts[0, individual, posy * width + posx] < 200000
            ) //If the position generated is another shelf (i.e under 200k, generate a new pos)
            {
                posx = Random.Range(0, width);
                posy = Random.Range(0, height);
            }

            _oneDLayouts[0, individual, posy * width + posx] = 10000 + x;
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
                    if (_oneDLayouts[0, individual, y * width + x] >= 200000)
                    {
                        _oneDLayouts[0, individual, y * width + x] = 200000 + paths;
                        paths++;
                        //Debug.Log("paths: " + paths);
                    }
                    else if (_oneDLayouts[0, individual, y * width + x] < 200000)
                    {
                        _oneDLayouts[0, individual, y * width + x] = 10000 + shelves;
                        shelves++;
                        //Debug.Log("Shelves:" + shelves);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finds 'fittest' individual from specified generation
    /// </summary>
    /// <param name="oneDLayouts">Multidimensional array representing all layouts in all generations</param>
    /// <param name="generation">Generation to find the fittest individual from</param>
    private void FindBestLayout(int[,,] oneDLayouts, int generation)
    {
        for (var individual = 0; individual < populationSize; individual++)
        {
            FitnessScores[generation, individual] = 0;
            DistinctPathSectionsCheck(0, individual);
            ShelfFitness(0, individual);
        }
    }

    void LayoutMake(int individual)
    {
        var level = new GameObject();
        level.transform.position = new Vector3(0, 0, 0);
        level.name = individual.ToString();
    }

    private void DisplayLayout(int individual, int generation)
    {
        LayoutMake(individual);
        for (var x = -1; x < width + 1; x++)
        {
   
            for (var z = -1; z < height + 1; z++)
            {
                if (x == -1 || x == width || z == -1 || z == height)
                {
                    var shelf = Instantiate(PerimeterShelf,
                        new Vector3(x + generation * (height + 10), 0.5f, z + individual * (width + 10)),
                        Quaternion.identity);
                    shelf.transform.parent = GameObject.Find(individual.ToString()).transform;
                }
            }
        }



        for (var x = 0; x < width; x++)
        {
            //Place Paths and Shelves Appropriately
            for (var z = 0; z < height; z++)
            {





                if (_oneDLayouts[generation, individual, x + z * width] < 200000
                ) //shelves are denoted by <200,000 values
                {


                    var shelf = Instantiate(oneShelf,
                        new Vector3(x + generation * (height + 10), 0.5f, z + individual * (width + 10)),
                        Quaternion.identity);
                    shelf.name = _oneDLayouts[generation, individual, x + z * width].ToString();
                    shelf.transform.parent = GameObject.Find(individual.ToString()).transform;


                    //Shelf.transform.parent.transform.position = new Vector3(individual * 20, 0, 0);
                }


            }
        }

    }

    private void ChooseParents(int generation, int individual, int Parent)
    {

        var generationFitness = 0F;
        var parentNum = -1;
        var scores = new Dictionary<int, float>();
        Dictionary<int, int> sortedScores = new Dictionary<int, int>();
        for (var ind = 0; ind < populationSize; ind++)
        {
            //Debug.Log(generation + "  " + ind);
            float temp = (float) FitnessScores[generation - 1, ind];
            if (temp == 0)
            {
                temp = 0.001F;
            }
            //Debug.Log(FitnessScores[generation, ind]);

            //Debug.Log(temp + "  <<temp");

            generationFitness += 1 / temp;

            scores.Add(ind, 1 / temp);
        }

        var fitnessThreshold = Random.Range(0, generationFitness);
        var fitnessCount = 0F;


        foreach (KeyValuePair<int, float> Fitness in scores.OrderByDescending(key => key.Value))
        {
            //Debug.Log(Fitness.Key + "," + Fitness.Value);
            fitnessCount += Fitness.Value;
            if (fitnessCount > fitnessThreshold && parentNum == -1)
            {
                parentNum = Fitness.Key;
            }

        }

        //Debug.Log("parent: " + parentNum + "    FitnessThreshold: " + fitnessThreshold + "  FitnessCount: " + fitnessCount);
        //Debug.Log(_parentIndexes[generation, individual, Parent]);
        //Debug.Log(generation + "ind  " + individual + "parent:   " + Parent + "PARENTNNUM:  " + parentNum);
        _parentIndexes[generation, individual, Parent] = parentNum;
    }

    void TruncatedSelection(int Generation)
    {
        var generationFitness = 0F;
        var scores = new Dictionary<int, float>();
        List<int> sortedScores = new List<int> ();
        for (var ind = 0; ind < populationSize; ind++)
        {
            scores.Add(ind, (float) FitnessScores[generation - 1, ind]);
        }

        var fitnessThreshold = Random.Range(0, generationFitness);
        var fitnessCount = 0F;
        float parentCount = ParentSelectionPercentage * populationSize;
        //Debug.Log(fitnessCount);
        //Debug.Log(parentCount);
        
        foreach (KeyValuePair<int, float> Fitness in scores.OrderBy(key => key.Value))
        {
            //Debug.Log(Fitness.Key);     //this is the layout num
            //Debug.Log(Fitness.Value);   //its fitness value
            
            if (fitnessCount < parentCount)
            {
                sortedScores.Add(Fitness.Key);
            }

            fitnessCount += 1;
        }
        
        for (int x = 0; x < sortedScores.Count; x++)        //randomly shuffles the list.
        {
            int randompos = UnityEngine.Random.Range(x, sortedScores.Count);
            var temp = sortedScores[x];
            sortedScores[x] = sortedScores[randompos];
            sortedScores[randompos] = temp;
        }



        for (int index = 0; index < populationSize; index++)
        {
            _parentIndexes[generation, index, 0] = sortedScores[index % sortedScores.Count];
            _parentIndexes[generation, index, 1] = sortedScores[index * 2 % sortedScores.Count];
            //Debug.Log(_parentIndexes[generation, index, 0] + "          " + _parentIndexes[generation, index, 1]);
            //Debug.Log("FITNESS SCORES:     " + FitnessScores[generation-1,_parentIndexes[generation, index, 0]] + "          " + FitnessScores[generation-1,_parentIndexes[generation, index, 1]]);
        }

    }

    void RandomSelection(int generation)
    {
        for (int index = 0; index < populationSize / 2; index++)
        {
            _parentIndexes[generation, index, 0] = Random.Range(0, populationSize);
            _parentIndexes[generation, index, 1] = Random.Range(0, populationSize);
        }
        
    }

    private void OnePointCrossover(int parent1Num, int parent2Num, int generation, int individual)
    {
        //("onpoint");
        var parent1 = new int[width * height];
        var parent2 = new int[width * height];

        int[] child1 = new int[width * height];
        int[] child2 = new int[width * height];

        for (var x = 0; x < width * height; x++)
        {
            //Debug.Log(x + "," + generation + "," + parent1Num);
            parent1[x] = _oneDLayouts[generation - 1, parent1Num, x];
            parent2[x] = _oneDLayouts[generation - 1, parent2Num, x];

            //Debug.Log(generation-1 + "         " + parent1Num +  "       " + x + "parent1[x]:" + parent1[x]);

            //Debug.Log("parent1[x]:" + parent1[x] + "  parent2[x]:" + parent2[x]);

            child1[x] = 0;
            child2[x] = 0;
        }

        int crossoverpoint1 = Random.Range(0, width * height);

        for (int x = 0; x < crossoverpoint1; x++)
        {
            child1[x] = parent2[x];
            child2[x] = parent1[x];
        }

        for (int x = crossoverpoint1; x < width * height; x++)
        {
            child1[x] = parent2[x];
            child2[x] = parent1[x];


            //("Child1[x]: " + child1[x]);

        }

        for (int x = 0; x < width * height; x++)
        {
            _oneDLayouts[generation, individual, x] = child1[x];
        }




    }

    private void Pmx(int parent1Num, int parent2Num, int generation, int individual) //Partial-Mapped Crossover
    {
        //Debug.Log("PMX BEGAN  " + individual);
        var parent1 = new int[width * height];
        var parent2 = new int[width * height];

        int[] child1 = new int[width * height];
        int[] child2 = new int[width * height];

        Dictionary<int, int> RelationshipsOneAsKey = new Dictionary<int, int>();
        Dictionary<int, int> RelationshipsTwoAsKey = new Dictionary<int, int>();

        //Debug.Log("new dicts made!!");


        //Debug.Log(parent1Num);
        //Debug.Log(parent2Num);
        //Debug.Log(generation); 
        //Debug.Log("------------------------Start of Parents ------------------------------");
        //Debug.Log(parent1Num + "                        " + parent2Num);


        for (var x = 0; x < width * height; x++)
        {
            //Debug.Log(x + "," + generation + "," + parent1Num);
            parent1[x] = _oneDLayouts[generation - 1, parent1Num, x];
            parent2[x] = _oneDLayouts[generation - 1, parent2Num, x];

            //Debug.Log(generation-1 + "         " + parent1Num +  "       " + x + "parent1[x]:" + parent1[x]);

            //Debug.Log("parent1[x]:" + parent1[x] + "  parent2[x]:" + parent2[x]);

            child1[x] = 0;
            child2[x] = 0;
        }

        //Debug.Log("-------------------------End of Parents -------------------------------");




        int crossOverPoint1 = 0;
        int crossOverPoint2 = 0;



        while (crossOverPoint1 == crossOverPoint2) //generate two points to cut gene that arent the same point
        {
            crossOverPoint1 = Random.Range(0, parent1.Length + 1); //cut points are before selected index. i.e index 3:
            crossOverPoint2 = Random.Range(0, parent2.Length + 1); // 012|345
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

            // Debug.Log(parent1[x]);
            // Debug.Log(parent2[x]);

            RelationshipsOneAsKey.Add(parent1[x], parent2[x]);
            RelationshipsTwoAsKey.Add(parent2[x], parent1[x]);
        }

        for (int x = 0; x < width * height; x++)
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

        for (int x = 0; x < width * height; x++)
        {
            _oneDLayouts[generation, individual, x] = child1[x];
            //Debug.Log("Child1[x]: " + child1[x]);
        }


        //Debug.Log("-------------------------End of Child -------------------------------");


    }

    void SwapMutation(int generation, int individual)
    {

        int x = Random.Range(0, width * height);
        int y = Random.Range(0, width * height);

        int temp = _oneDLayouts[generation, individual, x];

        //Debug.Log(x + "   " + y);
        _oneDLayouts[generation, individual, x] = _oneDLayouts[generation, individual, y];
        _oneDLayouts[generation, individual, y] = temp;
    }


    void ClearLayouts()
    {
        for (int x = 0; x < populationSize; x++)
        {
            var layout = GameObject.Find(x.ToString());
            layout.name = "done";
            Destroy(layout);
        }
    }

    void MoveCamera(int generation, int individual)
    {
        Camera.transform.position =
            new Vector3(generation * (width + 3.5F), 10, individual * (height + 4));
    }

    void GenerateBest(int generation, int individual)
    {
        var oldbest = GameObject.Find("Best");
        oldbest.name = "oldbest";
        oldbest.transform.position = new Vector3(1000, 0, 0);
        Destroy(oldbest);

        int pathcount = 0;



        var level = new GameObject();
        level.transform.position = new Vector3(-100, 0, 0);
        level.name = "Best";


        for (var x = -1; x < width + 1; x++)
        {

            for (var z = -1; z < height + 1; z++)
            {
                if (x == -1 || x == width || z == -1 || z == height)
                {
                    var shelf = Instantiate(PerimeterShelf,
                        new Vector3(x - 100, 0.5f, z - 100),
                        Quaternion.identity);
                    shelf.transform.parent = GameObject.Find("Best").transform;

                }
            }
        }

        for (var x = 0; x < width; x++) //Place Paths and Shelves Appropriately
        for (var z = 0; z < height; z++)
            if (_oneDLayouts[generation, individual, x + z * width] >= 200000) //paths are denoted by 200,000+ values
            {
                var path = Instantiate(camPath, new Vector3(x + -100, 0, z + -100),
                    Quaternion.identity);
                path.transform.parent = GameObject.Find("Best").transform;
                path.name = pathcount.ToString();
                pathcount += 1;
            }
            else if (_oneDLayouts[generation, individual, x + z * width] < 200000
            ) //shelves are denoted by <200,000 values
            {
                var shelf = Instantiate(camShelf, new Vector3(x + -100, 0.5f, z + -100),
                    Quaternion.identity);
                shelf.transform.parent = GameObject.Find("Best").transform;
                shelf.name = _oneDLayouts[bestLayoutGeneration, bestLayoutIndividual, x + z * width].ToString();

                //Shelf.transform.parent.transform.position = new Vector3(individual * 20, 0, 0);
            }
    }

    public void Run(int generation)
    {
        //Debug.Log("A NEW GENERATION");

        //ClearLayouts();


        float averageFitness = 0;
        if (generation > 0)
        {
            for (int individual = 0; individual < populationSize; individual++)
            {


                averageFitness += FitnessScores[generation - 1, individual];
                if (FitnessScores[generation - 1, individual] < bestLayout)
                {
                    //MoveCamera(generation, individual);
                    bestLayout = FitnessScores[generation - 1, individual];
                    bestLayoutGeneration = generation - 1;
                    bestLayoutIndividual = individual;
                    Debug.Log(bestLayout + "gen:" + generation + "  indv:" + individual);

                }
            }
        }
        else
        {
            averageFitness = 100f;
        }
        
        
        Debug.Log("average fitness:         " + averageFitness/populationSize + "     best fitness:" + bestLayout);

        //Debug.Log(averageFitness);

        if (bestLayout == 0)
        {
            //Debug.Log("I DID IT!!!!!!!!!!!!!!!!!!1");
        }
        else
        {
            if (RouletteWheel == true)
            {

                for (int individual = 0; individual < populationSize; individual++) //2 parents for each individual
                {

                    ChooseParents(generation, individual, 0);

                    ChooseParents(generation, individual, 1);
                }
            }
            else if (TruncatedSelect == true)
            {
                //Debug.Log("truncated Selection");
                TruncatedSelection(generation);
            }
            else if (RandomSelect == true)
            {
                RandomSelection(generation);
            }

            for (int individual = 0; individual < populationSize; individual++) //2 parents for each individual
            {
                int parent1Num = _parentIndexes[generation, individual, 0];
                int parent2Num = _parentIndexes[generation, individual, 1];
                //Debug.Log(_parentIndexes[generation, individual] + "" + _parentIndexes[generation, individual+populationSize]);
                if (PartiallyMappedCrossover == true)
                {
                    Pmx(parent1Num, parent2Num, generation, individual);
                }
                else if (SinglePointCrossOver == true)
                {
                    OnePointCrossover(parent1Num, parent2Num, generation, individual);
                }

                //Check for mutation
                int MutationRoll = Random.Range(0, 1);
                if (MutationRoll <= MutationChance)
                {
                    SwapMutation(generation, individual);
                }
                
                //Complex Fitness Functions
                DistinctPathSectionsCheck(generation, individual);
                ShelfFitness(generation, individual);
                
            }


        }


    }


    /// <summary>
    /// Checks the number of separated path sections in the layout and returns this value-1
    /// </summary>
    /// <param name="generation">Generation of individual to be checked</param>
    /// <param name="individual">Number of layout to be checked</param>
    void DistinctPathSectionsCheck(int generation, int individual)
    {
        //List of all visited paths
        List<int> allvisited = new List<int>();
        int pathssearched = 0;
        int numberOfPathSections = 0;
        
        //Search until all paths are found
        while (pathssearched < width * height - oneLengthShelvesCount)
        {

            //List of paths visited in current path section
            List<int> visited = new List<int>();
            bool pathfound = false;
            int x = 0;
            int pathsconnected = 0;

            bool searchedall = false;

            
            //find first path to begin search by sequentially looking through each position
            while (pathfound == false)
            {
                
                if (_oneDLayouts[generation, individual, x] >= 200000 && !(allvisited.Contains(x)))
                {
                    pathfound = true;
                    checkPosition = x;
                    visited.Add(checkPosition);
                    //Debug.Log(checkPosition);
                    pathsconnected += 1;

                }
                x++;
            }

            int starterposition = checkPosition;

            //Look at each neighbour of the path by Right, Left, Up, Down to find un-discovered path.
            while (searchedall == false)
            {
                //Debug.Log(checkPosition);
                if (Right(individual, visited))
                {
                    //Debug.Log("right     " );
                    pathsconnected += 1;
                }
                else if (Left(individual, visited))
                {
                    //Debug.Log("left    " );
                    pathsconnected += 1;
                }
                else if (Up(individual, visited))
                {
                    //Debug.Log("up    " );
                    pathsconnected += 1;
                }
                else if (Down(individual, visited))
                {
                    //Debug.Log("down    " );
                    pathsconnected += 1;
                }
                //If no neighbours are undiscovered, backtrack
                else if (Back(individual, visited, starterposition))
                {
                    //Debug.Log("Back   " );
                }
                else
                {
                    //If backtracking returns no new paths then un-discovered paths are not connected
                    searchedall = true;
                    if (pathsconnected == width * height - totalShelves)
                    {
                        //Debug.Log("CONNECTED");
                    }
                    else
                    {
                        pathssearched += pathsconnected;
                        //Debug.Log("Disconnected" + pathsconnected);

                        numberOfPathSections += 1;
                        foreach (int path in visited)
                        {
                            allvisited.Add(path);
                        }

                    }

                    
                }
            }
        }
        
        //Complex fitness function
        //Only want to add for each section beyond the minimum of 1.
        FitnessScores[generation, individual] += (int) ((numberOfPathSections - 1)* pathFitnessmult); 
    }
    
    
    bool Back(int individual, List<int> visited, int starterpos)
    {
        if (checkPosition != starterpos)
        {

            int index = visited.IndexOf(checkPosition);
            index -= 1;
            checkPosition = visited[index];
            return true;
        }

        return false;
        //go back one on list
    }

    /// <summary>
    /// Check for a path to the right of the current path
    /// </summary>
    /// <param name="individual">Layout Index</param>
    /// <param name="visited">List of discovered path connected to current</param>
    /// <returns>True if undiscovered path to the right, else false</returns>
    bool Right(int individual, List<int> visited)
    {
        if (checkPosition % width != width - 1)
        {
            if (checkPosition + 1 <= width * height)
            {
                if (_oneDLayouts[generation, individual, checkPosition + 1] >= 200000) //if its a path to the right
                {

                    //if that path isnt on the list
                    if (!visited.Contains(checkPosition + 1))
                        
                    {
                        checkPosition += 1;
                        visited.Add(checkPosition); //add to the list of visited places
                        return true;

                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check for a path to the left of the current path
    /// </summary>
    /// <param name="individual">Layout Index</param>
    /// <param name="visited">List of discovered path connected to current</param>
    /// <returns>True if undiscovered path to the left, else false</returns>
    bool Left(int individual, List<int> visited)
    {
        if (checkPosition % width != 0)
        {
            if (checkPosition - 1 >= 0)
            {
                if (_oneDLayouts[generation, individual, checkPosition - 1] >= 200000)
                {
                    if (!visited.Contains(checkPosition - 1))
                    {
                        checkPosition -= 1;
                        visited.Add(checkPosition);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Check for a path to the top of the current path
    /// </summary>
    /// <param name="individual">Layout Index</param>
    /// <param name="visited">List of discovered path connected to current</param>
    /// <returns>True if undiscovered path to the top, else false</returns>
    bool Up(int individual, List<int> visited)
    {
        if (checkPosition + width < width * height
        ) //if moving up one block wouldnt go beyond array limits of width * height
        {
            if (_oneDLayouts[generation, individual, checkPosition + width] >= 200000)
            {
                if (!visited.Contains(checkPosition + width))
                {
                    checkPosition += width;
                    visited.Add(checkPosition);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Check for a path to the bottom of the current path
    /// </summary>
    /// <param name="individual">Layout Index</param>
    /// <param name="visited">List of discovered path connected to current</param>
    /// <returns>True if undiscovered path to the bottom, else false</returns>
    bool Down(int individual, List<int> visited)
    {
        if (checkPosition >= width) //if the current position is not on the bottom row. i.e all values up to width
        {
            if (_oneDLayouts[generation, individual, checkPosition - width] >= 200000)
            {
                if (!visited.Contains(checkPosition - width))
                {
                    checkPosition -= width;
                    visited.Add(checkPosition);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Fitness Function based on a shelf's requirement to be facing at least one path and
    /// be the social distance away from other shelves.
    /// </summary>
    /// <param name="generation">Generation of layout to check fitness of</param>
    /// <param name="individual">Layout in generation to check fitness of</param>
    void ShelfFitness(int generation, int individual)
    {
        for (int x = 0; x < width * height; x++)
        {
            //Number of adjacent paths for each shelf
            int adjacentpathsL = 0;
            int adjacentpathsR = 0;
            int adjacentpathsU = 0;
            int adjacentpathsD = 0;

            //if shelf
            if (_oneDLayouts[generation, individual, x] < 200000) 
            {
                //check each distance up to social distance
                for (int distance = 1; distance <= SocialDistance; distance++)
                {
                    //layout boundary checks
                    if (x + distance < width * height && width > distance + (x % width))
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x + distance] < 200000)) 
                        {
                            adjacentpathsR += 1;
                        }
                    }

                    //layout boundary checks
                    if (x - distance >= 0 && x % width >= distance)
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x - distance] < 200000)) 
                        {
                            adjacentpathsL += 1;
                        }
                    }

                    //layout boundary checks
                    if (x + width * distance < width * height)
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x + width * distance] < 200000)) 
                        {
                            adjacentpathsU += 1;
                        }
                    }

                    //layout boundary checks
                    if (x - width * distance >= 0)
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x - width * distance] < 200000)) //is a shelf
                        {
                            adjacentpathsD += 1;
                        }
                    }
                    
                    if (pathWidthCheck == true)
                    {

                        //if a side is open but the gap is less than the distance
                        if (adjacentpathsL < SocialDistance && adjacentpathsL > 0
                        ) 
                        {
                            FitnessScores[generation, individual] += pathWidthMult;
                        }

                        if (adjacentpathsR < SocialDistance && adjacentpathsR > 0)
                        {
                            FitnessScores[generation, individual] += pathWidthMult;
                        }

                        if (adjacentpathsU < SocialDistance && adjacentpathsU > 0)
                        {
                            FitnessScores[generation, individual] += pathWidthMult;
                        }

                        if (adjacentpathsD < SocialDistance && adjacentpathsD > 0)
                        {
                            FitnessScores[generation, individual] += pathWidthMult;
                        }
                    }
                }
                // if no sides are open
                if ((adjacentpathsL < SocialDistance) && (adjacentpathsR < SocialDistance) && (adjacentpathsU < SocialDistance) && (adjacentpathsD < SocialDistance)) 
                {
                    FitnessScores[generation, individual] += 1;
                }
            }
        }
    }
}

