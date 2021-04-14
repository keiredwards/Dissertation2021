using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfPlacementCheck : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        int SocialDistance = GameObject.Find("ScriptHolder").GetComponent<BruteForce>().SocialDistance;
        int PerimeterShelves =  LayerMask.GetMask("PerimeterShelves");
        int NotPerimeterShelves = ~PerimeterShelves;
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x-1, transform.position.y, transform.position.z), Vector3.left, out hit, SocialDistance-1, NotPerimeterShelves))
        {
            Destroy(gameObject);
        }
        
      
        if (Physics.Raycast(transform.position, Vector3.left, out hit, SocialDistance, PerimeterShelves))
        {
            Destroy(gameObject);
        }
        
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, SocialDistance, PerimeterShelves))
        {
            Destroy(gameObject);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
