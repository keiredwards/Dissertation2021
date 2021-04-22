using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShelfPlacementCheckRight : MonoBehaviour
{
    public static int SocialDistance;

    private static int LeftShelves;
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name != "Testing")
        {
            LeftShelves = LayerMask.GetMask("LeftShelves");
            SocialDistance = GameObject.Find("ScriptHolder").GetComponent<BruteForce>().SocialDistance;
            int PerimeterShelves = LayerMask.GetMask("PerimeterShelves");
            int NotPerimeterShelves = ~PerimeterShelves;
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(transform.position.x + 1, transform.position.y, transform.position.z),
                Vector3.right, out hit, SocialDistance - 1, NotPerimeterShelves))
            {
                Destroy(gameObject);
            }


            if (Physics.Raycast(transform.position, Vector3.right, out hit, SocialDistance, PerimeterShelves))
            {
                Destroy(gameObject);
            }

            if (Physics.Raycast(transform.position, Vector3.forward, out hit, SocialDistance, PerimeterShelves))
            {
                Destroy(gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Testing")
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.left, out hit, SocialDistance, LeftShelves))
            {
                Debug.Log("lefthit");
                Destroy(gameObject);
            }
        }
    }
}
