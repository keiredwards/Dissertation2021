using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitnessOneLongShelf : MonoBehaviour
{
    public int LayoutNum;
    public GameObject ScriptHolder;
    private int Checked = 0;
    // Start is called before the first frame update
    void Start()
    {
        LayoutNum = int.Parse(transform.parent.name);
        ScriptHolder = GameObject.Find("ScriptHolder");

        if (transform.parent.position.x == 0)
        {

            //Debug.Log("we at 0 baws");
            transform.parent.position = new Vector3(0, 0, LayoutNum * 10);

        }

        if (Checked < 1)
        {

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 2f))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                AddtoFitness();
                //Debug.Log("WE gotta hit capn");
            }
            Checked++;

        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void AddtoFitness()
    {
        //Debug.Log(LayoutNum);
        ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[LayoutNum] += 1;

    }
}
