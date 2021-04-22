using System.Collections;
using System.Collections.Generic;
using SWS;
using UnityEngine;

public class GeneratePath : MonoBehaviour
{
    public bool GeneratePaths;

    public Transform[] PathPositions;

    public GameObject path2;
    public GameObject path3;
    public GameObject path5;
    public GameObject All;
    public int Priority = 0;

    private GameObject Best;
    // Start is called before the first frame update
    void Start()
    {
        Best = GameObject.Find("Best");
        Transform[] allChildren = Best.GetComponentsInChildren<Transform>();
        List<GameObject> childObjects = new List<GameObject>();

        if (GeneratePaths == true)
        {
            foreach (Transform child in allChildren)
            {
                int nameint = 0;
                int.TryParse(child.name, out nameint);
                if (nameint <10000)
                {

                    if (nameint % 2 == 0)
                    {
                        child.parent = path2.transform;
                    }
                    else if (nameint % 3 == 0)

                    {
                        child.parent = path3.transform;
                    }
                    else if (nameint % 5 == 0)

                    {
                        child.parent = path5.transform;
                    }
                }
            }
            
        }
        else
        {
            int index = 0;
            Transform[] allChildrenALL = All.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildrenALL)
            {
                
                if (index % 3 == 0)

                {
                    Instantiate(child, child.transform.position, Quaternion.identity, path3.transform);
                }
                else if (index % 5 == 0)

                {
                    Instantiate(child, child.transform.position, Quaternion.identity, path5.transform);
                }

                index++;
            }
            
            
            
        }
        CreatePath();
    }

    void CreatePath()
    {
        //path2.GetComponent<PathManager>().Create(path2.transform);
        path3.GetComponent<PathManager>().Create(path3.transform);
        path5.GetComponent<PathManager>().Create(path5.transform);
    }

    
}
