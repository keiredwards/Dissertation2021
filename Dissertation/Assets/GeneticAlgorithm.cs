using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public int Width;
    public int Height;
    public int[] OneDShopFloor;

    public int OneLengthShelvesCount;
    public GameObject OneShelf;
    public GameObject Path;


    void Start()    //Shop Floor is randomly generated.
    {
        int[,] ShopFloor = new int[Width, Height];
        OneDShopFloor = new int[(Width - 1) * (Height - 1)];
        

        for (int x = 0; x < Width; x++)     //Create Empty 2d Array Representing Shop Floor of only Paths where 0 is a path.
        {
            for(int y = 0; y <Height; y++)
            {
                ShopFloor[x, y] = 0;
            }
        }


        for(int x = 0; x< OneLengthShelvesCount; x++)     //Randomise Positions of Shelves and add to 2d array
        {
            int posx = Random.Range(0, Width);
            int posy = Random.Range(0, Height);

            while (ShopFloor[posx, posy] != 0)      //find a new position for shelf if not on pathv 
            {
                posx = Random.Range(0, Width);
                posy = Random.Range(0, Height);
            }

            ShopFloor[posx, posy] = 1;
        }

        for (int x = 0; x < Width; ++x)     //Make 2d array into 1d for combination
        {
            for (int y = 0; y < Height; ++y)
            {
                OneDShopFloor[x]= ShopFloor[x,y];
                Debug.Log(x +"," + y + "=" + ShopFloor[x, y]);
            }
        }

        for (int x = 0; x < Width; x++)     //Place Paths and Shelves Appropriately
        {
            for (int z = 0; z < Height; z++)
            {
                if(ShopFloor[x, z] == 0)
                {
                    Instantiate(Path, new Vector3(x, 0, z), Quaternion.identity);
                }
                else if (ShopFloor[x, z] == 1)
                {
                    Instantiate(OneShelf, new Vector3(x, 0, z), Quaternion.identity);
                }


            }
        }

    }



    void Update()
    {

    }
}
