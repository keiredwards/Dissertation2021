using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BruteForce : MonoBehaviour
{
    public static int[][] _layout;
    public int width;
    public int SocialDistance;

    public GameObject Path;
    public GameObject Shelf;

    public GameObject LeftShelf;

    public GameObject RightShelf;
    // Start is called before the first frame update
    void Start()
    {
        _layout = new int[width][];
        for (int x = 0; x < width; x++)
        {
            _layout[x] = new int[10];
        }

        BuildPerimeter();

        
    }

    // Update is called once per frame
    void Update()
    {
        BuildLeftShelves(Time.frameCount);
        BuildRightShelves(Time.frameCount);
    }

    void BuildPerimeter()
    {
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < _layout[z].Length; x++)
            {
                if (z == 0 || z == width-1 || x == 0 || x == _layout[z].Length-1)
                {
                    Instantiate(Shelf, new Vector3(z, 0, x), Quaternion.identity);
                }
                else
                {
                    Instantiate(Path, new Vector3(z, -0.5f, x), Quaternion.identity);
                }
            }
        }
    }

    void BuildLeftShelves(int z)
    {
        if(z< width/2){
            for (int x = 0; x < _layout[z].Length; x++)
            {
                Instantiate(LeftShelf, new Vector3(z, 0, x), Quaternion.identity);
            }
            
        }
    }
    
    void BuildRightShelves(int z)
    {
        Debug.Log(Math.Ceiling(width/2d));
        if(z< (Math.Ceiling(width+1/2d))){
            for (int x = 0; x < _layout[width-z].Length; x++)
            {
                Instantiate(RightShelf, new Vector3(width-z, 0, x), Quaternion.identity);
            }
            
        }
    }
}
