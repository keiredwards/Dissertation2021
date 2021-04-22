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

    public bool Uniform;
    public int[] Heights;

    public int height;
    // Start is called before the first frame update
    void Start()
    {
        _layout = new int[width][];
        for (int x = 0; x < width; x++)
        {
            if (Uniform == false)
            {
                _layout[x] = new int[Heights[x]];
            }
            else
            {
                _layout[x] = new int[height];
            }
        }

        GameObject Layout = new GameObject();
        Layout.name = "Layout";

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
        var Layout = GameObject.Find("Layout");
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < _layout[z].Length; x++)
            {
                if (z == 0 || z == width-1 || x == 0 || x == _layout[z].Length-1)
                {
                    var ShelfObject = Instantiate(Shelf, new Vector3(z, 0, x), Quaternion.identity);
                    ShelfObject.transform.parent = Layout.transform;
                }
                else
                {
                    var PathObject = Instantiate(Path, new Vector3(z, -0.5f, x), Quaternion.identity);
                    PathObject.transform.parent = Layout.transform;
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
        //Debug.Log(Math.Ceiling(width/2d));
        if(z< (Math.Ceiling(width+1/2d))){
            for (int x = 0; x < _layout[width-z].Length; x++)
            {
                Instantiate(RightShelf, new Vector3(width-z, 0, x), Quaternion.identity);
            }
            
        }
    }
}
