using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


public class GeneticAlgorithm : MonoBehaviour
{
    public static int[] shelvesChecked;
    public float MutationChance;
    public int width;
    public float bestLayout = Mathf.Infinity;
    public GameObject Camera;
    public GameObject PerimeterShelf;
    public int framerate;
    public bool SinglePointCrossOver;
    public bool PartiallyMappedCrossover;
    private int bestLayoutIndividual;
    private int bestLayoutGeneration;
    
    [FormerlySerializedAs("Height")] public int height;

    [FormerlySerializedAs("OneLengthShelvesCount")]
    public int oneLengthShelvesCount;

    [FormerlySerializedAs("TotalShelves")] public int totalShelves;

    [FormerlySerializedAs("PopulationSize")]
    public int populationSize;

    [FormerlySerializedAs("MaxGenerations")]
    public int maxGenerations;

    [FormerlySerializedAs("OneShelf")] public GameObject oneShelf;
    public GameObject path;
    public GameObject camShelf;
    public GameObject camPath;
    public int generation;
    
    private static int[,,,] _layouts;
    private static int[,,] _oneDLayouts;
    private static int[,,] _parentIndexes;
    public static int[,] FitnessScores;

    public int checkPosition = 0;
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    private void Start()
    {
        
        Application.targetFrameRate = framerate;
        //Debug.Log(width + "WIDTHHHHHHHHHHHHHHHHHH");
        //Debug.Log(shelvesChecked[0]);
        totalShelves = populationSize * oneLengthShelvesCount;
        _layouts = new int[maxGenerations, populationSize, width, height];
        _oneDLayouts = new int[maxGenerations, populationSize, width * height];
        _parentIndexes = new int[maxGenerations, populationSize,2];
        FitnessScores = new int[maxGenerations, populationSize];
        shelvesChecked = new int[maxGenerations];

        var level = new GameObject();
        level.transform.position = new Vector3(-100, 0, 0);
        level.name = "Best";
        generation = 0;
        
        //CreateFitnessScores(FitnessScores);
        
        GenerateNewLayout();
        FindBestLayout(_layouts, _oneDLayouts, 0);
        
        
    }


    private void Update()
    {
        //Debug.Log(FitnessScores[3, 2]);
        //Debug.Log(shelvesChecked[2]);
        
         //Debug.Log("shelves checked :  " + shelvesChecked[generation]);

         if ( generation < maxGenerations-1 && bestLayout>0)
            {
                //Debug.Log("run");
                generation++;
                //Debug.Log("gen:  " + generation);

                Run(generation);
                
            }
            else if (generation == maxGenerations)
            {
                Debug.Log("HALT THERE!!");
            }
            else if (bestLayout == 0)
            {
                Debug.Log("VICTORY");
            }
         GenerateBest(bestLayoutGeneration, bestLayoutIndividual);
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
                _parentIndexes[generation, individual, 0] = 0;
                _parentIndexes[generation, individual, 1] = 0;
                for (var x = 0;
                    x < width;
                    x++) //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
                for (var y = 0; y < height; y++)
                {
                    _layouts[generation, individual, x, y] = 200000 + x + y * width;
                    _oneDLayouts[generation, individual, y + x * height] = 0;
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


    private void FindBestLayout(int[,,,] layouts, int[,,] oneDLayouts, int generation)
    {
        for (var individual = 0; individual < populationSize; individual++)
        {
            
            //Debug.Log("gen" + generation);
            FitnessScores[generation, individual] = 0;


            ConvertToFlatArray(layouts, oneDLayouts, individual, generation);
            //DisplayLayout(individual, generation);
            ShelfFitness(generation,individual);
            CheckValidity(0,individual);
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
        Dictionary<int,int> sortedScores = new Dictionary<int, int>();
        for (var ind = 0; ind < populationSize; ind++)
        {
            //Debug.Log(generation + "  " + ind);
            float temp = (float) FitnessScores[generation-1, ind];
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
        //Debug.Log(_parentIndexes[generation, individual, Parent]);
        //Debug.Log(generation + "ind  " + individual + "parent:   " + Parent + "PARENTNNUM:  " + parentNum);
        _parentIndexes[generation, individual, Parent] = parentNum;
    }

    private void OnePointCrossover(int parent1Num, int parent2Num, int generation, int individual)
    {
        //("onpoint");
        var parent1 = new int[width * height];
        var parent2 = new int[width * height];
        
        int[] child1 = new int[width * height];
        int[] child2 = new int[width * height];
        
        for (var x = 0; x < width*height; x++)
        {
            //Debug.Log(x + "," + generation + "," + parent1Num);
            parent1[x] = _oneDLayouts[generation-1, parent1Num, x];
            parent2[x] = _oneDLayouts[generation-1, parent2Num, x];
            
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
        
        for (int x = crossoverpoint1; x < width*height; x++)
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
        
        
        for (var x = 0; x < width*height; x++)
        {
            //Debug.Log(x + "," + generation + "," + parent1Num);
            parent1[x] = _oneDLayouts[generation-1, parent1Num, x];
            parent2[x] = _oneDLayouts[generation-1, parent2Num, x];
            
            //Debug.Log(generation-1 + "         " + parent1Num +  "       " + x + "parent1[x]:" + parent1[x]);
            
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
            
           // Debug.Log(parent1[x]);
           // Debug.Log(parent2[x]);

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
                        new Vector3(x -100 , 0.5f, z - 100),
                        Quaternion.identity);
                    shelf.transform.parent = GameObject.Find("Best").transform;
                }
            }
        }
        
        for (var x = 0; x < width; x++) //Place Paths and Shelves Appropriately
        for (var z = 0; z < height; z++) 
            if (_oneDLayouts[generation, individual, x + z*width] >= 200000)    //paths are denoted by 200,000+ values
            {
                var path = Instantiate(camPath, new Vector3(x + -100, 0, z + -100),
                    Quaternion.identity);
                path.transform.parent = GameObject.Find("Best").transform;
            }
            else if (_oneDLayouts[generation, individual, x + z*width] < 200000) //shelves are denoted by <200,000 values
            {
                var shelf = Instantiate(camShelf, new Vector3(x + -100, 0.5f, z + -100),
                    Quaternion.identity);
                shelf.transform.parent = GameObject.Find("Best").transform;
                
                //Shelf.transform.parent.transform.position = new Vector3(individual * 20, 0, 0);
            }
    }
    public void Run(int generation)
    {
        //Debug.Log("A NEW GENERATION");
        
        //ClearLayouts();
          
        
        int averageFitness = 0;
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
            averageFitness = 100;
        }
        
        
        
        //Debug.Log(averageFitness);

        if (bestLayout == 0)
        {
            //Debug.Log("I DID IT!!!!!!!!!!!!!!!!!!1");
        }
        else
        {
            
            for (int individual = 0; individual < populationSize; individual++) //2 parents for each individual
            {
                
                ChooseParents( generation, individual, 0);
                
                ChooseParents( generation, individual, 1);
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
                else if(SinglePointCrossOver == true)
                {
                    OnePointCrossover(parent1Num, parent2Num, generation, individual);
                }

                  
                
                
                int MutationRoll = Random.Range(0, 1);
                if (MutationRoll <= MutationChance)
                {
                    //Debug.Log("mutate");
                    SwapMutation(generation, individual);
                }
                
                
                CheckValidity(generation,individual);
                ShelfFitness(generation, individual);
                //DisplayLayout(individual, generation);
                
                
            }
            
            
        }
        
            
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

    void CheckValidity(int generation, int individual)
    {

        //Debug.Log("validy checked of gen, individual" + generation + ", " + individual);
        List<int> allvisited = new List<int>();


        int pathssearched = 0;
        int pathsections = 0;
        //Debug.Log(pathssearched<width*height-totalShelves);
        while(pathssearched<width*height-oneLengthShelvesCount){
            //Debug.Log("here");
            List<int> visited = new List<int>();
            bool pathfound = false;
            int x = 0;
            int pathsconnected = 0;
            
            bool searchedall = false;
            
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
        
        while (searchedall == false)
        {
            //Debug.Log(checkPosition);
            if (Right(individual, visited))
            { //Debug.Log("right     " );
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
            else if (Back(individual, visited, starterposition))
            {
                //Debug.Log("Back   " );
            }
            else
            {
                searchedall = true;
                if (pathsconnected == width * height - totalShelves)
                {
                    //Debug.Log("CONNECTED");
                }
                else
                {
                    pathssearched += pathsconnected;
                    //Debug.Log("Disconnected" + pathsconnected);
                    
                    pathsections += 1;
                    foreach(int path in visited){allvisited.Add(path);}
                    
                }

                //Debug.Log("searched");
            }
        }
        }
        //Debug.Log(pathsections);
        FitnessScores[generation, individual] += pathsections-1; //Only want to add for each section beyond the minimum of 1.
    }


   
    bool Back(int individual, List<int> visited, int starterpos)
    {
        if (checkPosition != starterpos)
        {
            
            int index = visited.IndexOf(checkPosition);
            //Debug.Log(checkPosition + "------------------------" + starterpos + "----------------" + index + "--------------------" + visited.IndexOf(checkPosition));
            index -= 1;
            checkPosition = visited[index];
            return true;
        }

        return false;
        //go back one on list
    }
    
    bool Right( int individual, List<int> visited)
    {
        if (checkPosition % width != width-1)
        {
            if (checkPosition + 1 <= width * height)
            {
                if (_oneDLayouts[generation, individual, checkPosition + 1] >= 200000) //if its a path to the right
                {

                    if (!visited.Contains(checkPosition + 1))
                        //if that path isnt on the list
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
        
    bool Up(int individual, List<int> visited)
    {
        if (checkPosition + height <= width * height-1)
        {
            if (_oneDLayouts[generation, individual, checkPosition + height] >= 200000)
            {
                if (!visited.Contains(checkPosition+height))
                {
                    checkPosition += height;
                    visited.Add(checkPosition);
                    return true;
                }
            }
        }

        return false;
    }
        
    bool Down( int individual, List<int> visited)
    {
        if (checkPosition >= width)
        {
            if (_oneDLayouts[generation, individual, checkPosition - height] >= 200000)
            {
                if (!visited.Contains(checkPosition-height))
                {
                    checkPosition -= height;
                    visited.Add(checkPosition);
                    return true;
                }
            }
        }

        return false;
    }
    
    void ShelfFitness(int generation, int individual)
    {
        
        for (int x =  0; x < width*height; x++)
        {
            int adjacentpaths = 0;
            //Debug.Log(x);
            if (_oneDLayouts[generation, individual, x] < 200000)       //is a shelf
            {
                if (x + 1 < width*height && x % width!= width-1)
                {
                    if (!(_oneDLayouts[generation, individual, x + 1] < 200000))  //is a shelf
                    {
                        adjacentpaths += 1;
                        //Debug.Log("r");
                    }
                }
                if (x - 1 >= 0 && x % width !=0)
                {
                    if (!(_oneDLayouts[generation, individual, x - 1] < 200000))  //is a shelf
                    {
                        adjacentpaths += 1;
                        //Debug.Log("l");
                    }
                }
                if (x + width < width*height)
                {
                    //Debug.Log(x+width);
                    if (!(_oneDLayouts[generation, individual, x + width] < 200000))  //is a shelf
                    {
                        adjacentpaths += 1;
                        //Debug.Log("up");
                    }
                }
                if (x - width >= 0)
                {
                    if (!(_oneDLayouts[generation, individual, x - width] < 200000))  //is a shelf
                    {
                        adjacentpaths += 1;
                        //Debug.Log("down");
                    }
                }

                if (adjacentpaths == 0)
                {
                    FitnessScores[generation, individual] += 1;
                }
            }
        }
        //Debug.Log(FitnessScores[generation,individual]);
    }
}

