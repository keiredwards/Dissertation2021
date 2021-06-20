using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GeneticAlgorithm : MonoBehaviour
{
    [Header("-----Selection-----")]
    [SerializeField] private bool rouletteWheelSelection;
    [SerializeField] private bool truncatedSelection;
    [SerializeField] private bool randomSelection;
    
    [Header("-----Crossover-----")]
    [SerializeField] private bool singlePointCrossOver;
    [SerializeField] private bool partiallyMappedCrossover;
    
    [Header("-----Layout Descriptors-----")]
    [SerializeField] private int layoutWidth;
    [SerializeField] private int layoutHeight;
    [SerializeField] private int numberOfShelves;
    
    [Header("-----Genetic Algorithm Variables-----")]
    [SerializeField] private int populationSize;
    [SerializeField] private int maxGenerations;
    [SerializeField] private float parentSelectionPercentage;
    [SerializeField] private float mutationChance;
    
    [Header("-----Fitness Function Variables-----")]
    [SerializeField] private int socialDistance;
    [SerializeField] private float pathFitnessMult;
    [SerializeField] private float pathWidthMult;
    [SerializeField] private bool pathWidthCheck;

    [Header("-----Unity Objects-----")]
    [SerializeField] private GameObject shelf;
    [SerializeField] private GameObject path;
    [SerializeField] private GameObject bestLayoutShelf;
    [SerializeField] private GameObject bestLayoutPath;
    [SerializeField] private GameObject wall;
    
    private int _generation;
    private static int[,,] _oneDLayouts;
    private static int[,,] _parentIndexes;
    private static float[,] _fitnessScores;
    private int _bestLayoutIndividual;
    private int _bestLayoutGeneration;
    private float _bestLayout = Mathf.Infinity;
    private int _checkPosition = 0;
    private int _totalShelves;
    private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

    private void Start()
    {
        _generation = 0;
        _totalShelves = populationSize * numberOfShelves;
        _oneDLayouts = new int[maxGenerations, populationSize, layoutWidth * layoutHeight];
        _parentIndexes = new int[maxGenerations, populationSize, 2];
        _fitnessScores = new float[maxGenerations, populationSize];

        //Unity Best Layout Objects
        var level = new GameObject();
        level.transform.position = new Vector3(-100, 0, 0);
        level.name = "Best";
        
        _generation = 0;
        GenerateNewLayout();
        FindBestLayout(_oneDLayouts, 0);
    }
    
    /// <summary>
    /// Controls the progression of the Genetic Algorithm. Each Generation occurs within a single frame.
    /// </summary>
    private void Update()
    {
        if (_generation < maxGenerations - 1 && _bestLayout > 0)
        {
            //First Update call happens after First Generation is processed in Start()
            _generation++;
            Run(_generation);
        }
        else if (_generation == maxGenerations)
        {
            Debug.Log("HALT THERE!!");
        }
        else if (_bestLayout == 0)
        {
            Debug.Log("VICTORY  " + _generation);

        }
        GenerateBest(_bestLayoutGeneration, _bestLayoutIndividual);
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
                for (var x = 0; x < layoutHeight; x++){ //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
                for (var y = 0; y < layoutWidth; y++)
                {
                    _oneDLayouts[generation, individual, y + x * layoutWidth] = 200000; //sets all to path value
                    _fitnessScores[generation, individual] = 0;
                }
            }
        }
        }
        
        for (var individual = 0; individual < populationSize; individual++)
        for (var x = 0; x < numberOfShelves; x++) //Randomise Positions of Shelves and add to 2d array
        {
            var posx = Random.Range(0, layoutWidth);
            var posy = Random.Range(0, layoutHeight);

            while (_oneDLayouts[0, individual, posy * layoutWidth + posx] < 200000
            ) //If the position generated is another shelf (i.e under 200k, generate a new pos)
            {
                posx = Random.Range(0, layoutWidth);
                posy = Random.Range(0, layoutHeight);
            }

            _oneDLayouts[0, individual, posy * layoutWidth + posx] = 10000 + x;
        }

        //fixing shelf and path numbering

        for (var individual = 0; individual < populationSize; individual++)
        {
            int shelves = 0;
            int paths = 0;

            for (int y = 0; y < layoutHeight; y++)
            {
                for (int x = 0; x < layoutWidth; x++)
                {
                    //Debug.Log(_layouts[0, individual, x, y]);
                    if (_oneDLayouts[0, individual, y * layoutWidth + x] >= 200000)
                    {
                        _oneDLayouts[0, individual, y * layoutWidth + x] = 200000 + paths;
                        paths++;
                        //Debug.Log("paths: " + paths);
                    }
                    else if (_oneDLayouts[0, individual, y * layoutWidth + x] < 200000)
                    {
                        _oneDLayouts[0, individual, y * layoutWidth + x] = 10000 + shelves;
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
            _fitnessScores[generation, individual] = 0;
            DistinctPathSectionsCheck(0, individual);
            ShelfFitness(0, individual);
        }
    }

    /// <summary>
    /// Displays the specified layout in unity
    /// </summary>
    /// <param name="individual">Layout Number</param>
    /// <param name="generation">Generation Number</param>
    private void DisplayLayout(int individual, int generation)
    {
        var level = new GameObject();
        level.transform.position = new Vector3(0, 0, 0);
        level.name = individual.ToString();
        
        //Place walls
        for (var x = -1; x < layoutWidth + 1; x++)
        {
            for (var z = -1; z < layoutHeight + 1; z++)
            {
                if (x == -1 || x == layoutWidth || z == -1 || z == layoutHeight)
                {
                    var shelf = Instantiate(wall,
                        new Vector3(x + generation * (layoutHeight + 10), 0.5f, z + individual * (layoutWidth + 10)),
                        Quaternion.identity);
                    shelf.transform.parent = GameObject.Find(individual.ToString()).transform;
                }
            }
        }
        
        
        for (var x = 0; x < layoutWidth; x++)
        {
            //Place Paths and Shelves Appropriately
            for (var z = 0; z < layoutHeight; z++)
            {
                if (_oneDLayouts[generation, individual, x + z * layoutWidth] < 200000
                ) //shelves are denoted by <200,000 values
                {
                    var shelf = Instantiate(this.shelf,
                        new Vector3(x + generation * (layoutHeight + 10), 0.5f, z + individual * (layoutWidth + 10)),
                        Quaternion.identity);
                    shelf.name = _oneDLayouts[generation, individual, x + z * layoutWidth].ToString();
                    shelf.transform.parent = GameObject.Find(individual.ToString()).transform;
                }


            }
        }

    }

    /// <summary>
    /// Roulette Wheel Style Selection for parents of next generation
    /// </summary>
    /// <param name="generation"></param>
    /// <param name="individual"></param>
    /// <param name="parent"></param>
    private void RouletteWheelSelection(int generation, int individual, int parent)
    {
        var generationFitness = 0F;
        var parentNum = -1;
        var scores = new Dictionary<int, float>();
        Dictionary<int, int> sortedScores = new Dictionary<int, int>();
        
        //chance of selection is inversely proportional to fitness score
        for (var ind = 0; ind < populationSize; ind++)
        {
            float temp = (float) _fitnessScores[generation - 1, ind];
            if (temp == 0)
            {
                temp = 0.001F;
            }

            generationFitness += 1 / temp;

            scores.Add(ind, 1 / temp);
        }

        var fitnessThreshold = Random.Range(0, generationFitness);
        var fitnessCount = 0F;


        //selects parent as first layout that takes the fitness score over the threshold
        foreach (KeyValuePair<int, float> fitness in scores.OrderByDescending(key => key.Value))
        {
            fitnessCount += fitness.Value;
            if (fitnessCount > fitnessThreshold && parentNum == -1)
            {
                parentNum = fitness.Key;
            }
        }
        
        _parentIndexes[generation, individual, parent] = parentNum;
    }

    /// <summary>
    /// Truncated Selection style parent selection
    /// </summary>
    /// <param name="generation">Current Generation</param>
    private void TruncatedSelection(int generation)
    {
        var generationFitness = 0F;
        var scores = new Dictionary<int, float>();
        List<int> sortedScores = new List<int> ();
        for (var ind = 0; ind < populationSize; ind++)
        {
            scores.Add(ind, (float) _fitnessScores[_generation - 1, ind]);
        }

        var fitnessThreshold = Random.Range(0, generationFitness);
        var fitnessCount = 0F;
        float parentCount = parentSelectionPercentage * populationSize;

        foreach (KeyValuePair<int, float> fitness in scores.OrderBy(key => key.Value))
        {
            //Debug.Log(Fitness.Key);     //this is the layout num
            //Debug.Log(Fitness.Value);   //its fitness value
            
            if (fitnessCount < parentCount)
            {
                sortedScores.Add(fitness.Key);
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
            _parentIndexes[_generation, index, 0] = sortedScores[index % sortedScores.Count];
            _parentIndexes[_generation, index, 1] = sortedScores[index * 2 % sortedScores.Count];
            //Debug.Log(_parentIndexes[generation, index, 0] + "          " + _parentIndexes[generation, index, 1]);
            //Debug.Log("FITNESS SCORES:     " + FitnessScores[generation-1,_parentIndexes[generation, index, 0]] + "          " + FitnessScores[generation-1,_parentIndexes[generation, index, 1]]);
        }

    }

    /// <summary>
    /// Parents are randomly selected for next generation
    /// </summary>
    /// <param name="generation"></param>
    void RandomSelection(int generation)
    {
        for (int index = 0; index < populationSize / 2; index++)
        {
            _parentIndexes[generation, index, 0] = Random.Range(0, populationSize);
            _parentIndexes[generation, index, 1] = Random.Range(0, populationSize);
        }
        
    }

    /// <summary>
    /// Single point Crossover
    /// </summary>
    /// <param name="parent1Num">First Parent Individual</param>
    /// <param name="parent2Num">Second Parent Individual</param>
    /// <param name="generation">Generation of child</param>
    /// <param name="individual">Child's layout number</param>
    private void OnePointCrossover(int parent1Num, int parent2Num, int generation, int individual)
    {
        var parent1 = new int[layoutWidth * layoutHeight];
        var parent2 = new int[layoutWidth * layoutHeight];
        int[] child1 = new int[layoutWidth * layoutHeight];
        int[] child2 = new int[layoutWidth * layoutHeight];

        for (var x = 0; x < layoutWidth * layoutHeight; x++)
        {
            parent1[x] = _oneDLayouts[generation - 1, parent1Num, x];
            parent2[x] = _oneDLayouts[generation - 1, parent2Num, x];
            child1[x] = 0;
            child2[x] = 0;
        }

        int crossoverpoint1 = Random.Range(0, layoutWidth * layoutHeight);

        for (int x = 0; x < crossoverpoint1; x++)
        {
            child1[x] = parent2[x];
            child2[x] = parent1[x];
        }

        for (int x = crossoverpoint1; x < layoutWidth * layoutHeight; x++)
        {
            child1[x] = parent2[x];
            child2[x] = parent1[x];
        }

        for (int x = 0; x < layoutWidth * layoutHeight; x++)
        {
            _oneDLayouts[generation, individual, x] = child1[x];
        }
    }


    /// <summary>
    /// Partially mapped crossover technique
    /// </summary>
    /// <param name="parent1Num">First Parent Individual</param>
    /// <param name="parent2Num">Second Parent Individual</param>
    /// <param name="generation">Generation of child</param>
    /// <param name="individual">Child's layout number</param>
    private void Pmx(int parent1Num, int parent2Num, int generation, int individual) //Partial-Mapped Crossover
    {
        //Debug.Log("PMX BEGAN  " + individual);
        var parent1 = new int[layoutWidth * layoutHeight];
        var parent2 = new int[layoutWidth * layoutHeight];

        int[] child1 = new int[layoutWidth * layoutHeight];
        int[] child2 = new int[layoutWidth * layoutHeight];

        Dictionary<int, int> relationshipsOneAsKey = new Dictionary<int, int>();
        Dictionary<int, int> relationshipsTwoAsKey = new Dictionary<int, int>();

        //Debug.Log("------------------------Start of Parents ------------------------------");


        for (var x = 0; x < layoutWidth * layoutHeight; x++)
        {
            parent1[x] = _oneDLayouts[generation - 1, parent1Num, x];
            parent2[x] = _oneDLayouts[generation - 1, parent2Num, x];

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

        //Debug.Log("----------------------Start of Crossover----------------------");

        for (int x = crossOverPoint1; x < crossOverPoint2; ++x)
        {
            child1[x] = parent2[x];
            child2[x] = parent1[x];

            relationshipsOneAsKey.Add(parent1[x], parent2[x]);
            relationshipsTwoAsKey.Add(parent2[x], parent1[x]);
        }

        for (int x = 0; x < layoutWidth * layoutHeight; x++)
        {
            if (x < crossOverPoint2 && x >= crossOverPoint1)
            {
            }
            else
            {
                if (child1.Contains(parent1[x]))
                {
                    int key = parent1[x];

                    while (child1.Contains(key))
                    {
                        key = relationshipsTwoAsKey[
                            key];
                    }
                    child1[x] = key;
                }
                else
                {
                    child1[x] = parent1[x];
                }
            }
        }
        //Debug.Log("-------------------------Start of Child -------------------------------");
        for (int x = 0; x < layoutWidth * layoutHeight; x++)
        {
            _oneDLayouts[generation, individual, x] = child1[x];
        }
        //Debug.Log("-------------------------End of Child -------------------------------");
    }

    /// <summary>
    /// Simple swap mutation of two positions in layout
    /// </summary>
    /// <param name="generation">generation of individual</param>
    /// <param name="individual">individual to be mutated</param>
    void SwapMutation(int generation, int individual)
    {

        int x = Random.Range(0, layoutWidth * layoutHeight);
        int y = Random.Range(0, layoutWidth * layoutHeight);

        int temp = _oneDLayouts[generation, individual, x];
        
        _oneDLayouts[generation, individual, x] = _oneDLayouts[generation, individual, y];
        _oneDLayouts[generation, individual, y] = temp;
    }
    
    /// <summary>
    /// Clears Layouts from unity
    /// </summary>
    void ClearLayouts()
    {
        for (int x = 0; x < populationSize; x++)
        {
            var layout = GameObject.Find(x.ToString());
            layout.name = "done";
            Destroy(layout);
        }
    }


    /// <summary>
    /// Displays best individual so far
    /// </summary>
    /// <param name="generation">generation of best layout</param>
    /// <param name="individual">id of best layout</param>
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


        for (var x = -1; x < layoutWidth + 1; x++)
        {

            for (var z = -1; z < layoutHeight + 1; z++)
            {
                if (x == -1 || x == layoutWidth || z == -1 || z == layoutHeight)
                {
                    var shelf = Instantiate(wall,
                        new Vector3(x - 100, 0.5f, z - 100),
                        Quaternion.identity);
                    shelf.transform.parent = GameObject.Find("Best").transform;

                }
            }
        }

        for (var x = 0; x < layoutWidth; x++) //Place Paths and Shelves Appropriately
        for (var z = 0; z < layoutHeight; z++)
            if (_oneDLayouts[generation, individual, x + z * layoutWidth] >= 200000) //paths are denoted by 200,000+ values
            {
                var path = Instantiate(bestLayoutPath, new Vector3(x + -100, 0, z + -100),
                    Quaternion.identity);
                path.transform.parent = GameObject.Find("Best").transform;
                path.name = pathcount.ToString();
                pathcount += 1;
            }
            else if (_oneDLayouts[generation, individual, x + z * layoutWidth] < 200000
            ) //shelves are denoted by <200,000 values
            {
                var shelf = Instantiate(bestLayoutShelf, new Vector3(x + -100, 0.5f, z + -100),
                    Quaternion.identity);
                shelf.transform.parent = GameObject.Find("Best").transform;
                shelf.name = _oneDLayouts[_bestLayoutGeneration, _bestLayoutIndividual, x + z * layoutWidth].ToString();

                //Shelf.transform.parent.transform.position = new Vector3(individual * 20, 0, 0);
            }
    }

    /// <summary>
    /// Produces next generation following stages of a Genetic Algorithm
    /// </summary>
    /// <param name="generation">generation to be created</param>
    public void Run(int generation)
    {
        //Debug.Log("A NEW GENERATION");
        float averageFitness = 0;
        if (generation > 0)
        {
            for (int individual = 0; individual < populationSize; individual++)
            {


                averageFitness += _fitnessScores[generation - 1, individual];
                if (_fitnessScores[generation - 1, individual] < _bestLayout)
                {
                    _bestLayout = _fitnessScores[generation - 1, individual];
                    _bestLayoutGeneration = generation - 1;
                    _bestLayoutIndividual = individual;
                    Debug.Log(_bestLayout + "gen:" + generation + "  indv:" + individual);

                }
            }
        }
        else
        {
            averageFitness = 100f;
        }
        
        Debug.Log("average fitness:         " + averageFitness/populationSize + "     best fitness:" + _bestLayout);

        if (_bestLayout == 0)
        {
            //Perfect Layout found
        }
        else
        {
            if (rouletteWheelSelection == true)
            {

                for (int individual = 0; individual < populationSize; individual++) //2 parents for each individual
                {

                    RouletteWheelSelection(generation, individual, 0);

                    RouletteWheelSelection(generation, individual, 1);
                }
            }
            else if (truncatedSelection == true)
            {
                //Debug.Log("truncated Selection");
                TruncatedSelection(generation);
            }
            else if (randomSelection == true)
            {
                RandomSelection(generation);
            }

            for (int individual = 0; individual < populationSize; individual++) //2 parents for each individual
            {
                int parent1Num = _parentIndexes[generation, individual, 0];
                int parent2Num = _parentIndexes[generation, individual, 1];
                //Debug.Log(_parentIndexes[generation, individual] + "" + _parentIndexes[generation, individual+populationSize]);
                if (partiallyMappedCrossover == true)
                {
                    Pmx(parent1Num, parent2Num, generation, individual);
                }
                else if (singlePointCrossOver == true)
                {
                    OnePointCrossover(parent1Num, parent2Num, generation, individual);
                }

                //Check for mutation
                int mutationRoll = Random.Range(0, 1);
                if (mutationRoll <= mutationChance)
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
        while (pathssearched < layoutWidth * layoutHeight - numberOfShelves)
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
                    _checkPosition = x;
                    visited.Add(_checkPosition);
                    //Debug.Log(checkPosition);
                    pathsconnected += 1;

                }
                x++;
            }

            int starterposition = _checkPosition;

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
                    if (pathsconnected == layoutWidth * layoutHeight - _totalShelves)
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
        _fitnessScores[generation, individual] += (int) ((numberOfPathSections - 1)* pathFitnessMult); 
    }

    /// <summary>
    /// Backtrack through list of previously explored paths
    /// </summary>
    /// <param name="individual"></param>
    /// <param name="visited"></param>
    /// <param name="starterpos"></param>
    /// <returns></returns>
    bool Back(int individual, List<int> visited, int starterpos)
    {
        if (_checkPosition != starterpos)
        {

            int index = visited.IndexOf(_checkPosition);
            index -= 1;
            _checkPosition = visited[index];
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
        if (_checkPosition % layoutWidth != layoutWidth - 1)
        {
            if (_checkPosition + 1 <= layoutWidth * layoutHeight)
            {
                if (_oneDLayouts[_generation, individual, _checkPosition + 1] >= 200000) //if its a path to the right
                {

                    //if that path isnt on the list
                    if (!visited.Contains(_checkPosition + 1))
                        
                    {
                        _checkPosition += 1;
                        visited.Add(_checkPosition); //add to the list of visited places
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
        if (_checkPosition % layoutWidth != 0)
        {
            if (_checkPosition - 1 >= 0)
            {
                if (_oneDLayouts[_generation, individual, _checkPosition - 1] >= 200000)
                {
                    if (!visited.Contains(_checkPosition - 1))
                    {
                        _checkPosition -= 1;
                        visited.Add(_checkPosition);
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
        if (_checkPosition + layoutWidth < layoutWidth * layoutHeight
        ) //if moving up one block wouldnt go beyond array limits of width * height
        {
            if (_oneDLayouts[_generation, individual, _checkPosition + layoutWidth] >= 200000)
            {
                if (!visited.Contains(_checkPosition + layoutWidth))
                {
                    _checkPosition += layoutWidth;
                    visited.Add(_checkPosition);
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
        if (_checkPosition >= layoutWidth) //if the current position is not on the bottom row. i.e all values up to width
        {
            if (_oneDLayouts[_generation, individual, _checkPosition - layoutWidth] >= 200000)
            {
                if (!visited.Contains(_checkPosition - layoutWidth))
                {
                    _checkPosition -= layoutWidth;
                    visited.Add(_checkPosition);
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
        for (int x = 0; x < layoutWidth * layoutHeight; x++)
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
                for (int distance = 1; distance <= socialDistance; distance++)
                {
                    //layout boundary checks
                    if (x + distance < layoutWidth * layoutHeight && layoutWidth > distance + (x % layoutWidth))
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x + distance] < 200000)) 
                        {
                            adjacentpathsR += 1;
                        }
                    }

                    //layout boundary checks
                    if (x - distance >= 0 && x % layoutWidth >= distance)
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x - distance] < 200000)) 
                        {
                            adjacentpathsL += 1;
                        }
                    }

                    //layout boundary checks
                    if (x + layoutWidth * distance < layoutWidth * layoutHeight)
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x + layoutWidth * distance] < 200000)) 
                        {
                            adjacentpathsU += 1;
                        }
                    }

                    //layout boundary checks
                    if (x - layoutWidth * distance >= 0)
                    {
                        //if a path
                        if (!(_oneDLayouts[generation, individual, x - layoutWidth * distance] < 200000)) //is a shelf
                        {
                            adjacentpathsD += 1;
                        }
                    }
                    
                    if (pathWidthCheck == true)
                    {

                        //if a side is open but the gap is less than the distance
                        if (adjacentpathsL < socialDistance && adjacentpathsL > 0
                        ) 
                        {
                            _fitnessScores[generation, individual] += pathWidthMult;
                        }

                        if (adjacentpathsR < socialDistance && adjacentpathsR > 0)
                        {
                            _fitnessScores[generation, individual] += pathWidthMult;
                        }

                        if (adjacentpathsU < socialDistance && adjacentpathsU > 0)
                        {
                            _fitnessScores[generation, individual] += pathWidthMult;
                        }

                        if (adjacentpathsD < socialDistance && adjacentpathsD > 0)
                        {
                            _fitnessScores[generation, individual] += pathWidthMult;
                        }
                    }
                }
                // if no sides are open
                if ((adjacentpathsL < socialDistance) && (adjacentpathsR < socialDistance) && (adjacentpathsU < socialDistance) && (adjacentpathsD < socialDistance)) 
                {
                    _fitnessScores[generation, individual] += 1;
                }
            }
        }
    }
}

