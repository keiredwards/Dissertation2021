using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FitnessOneLongShelf : MonoBehaviour
{
    private int LayoutNum;
    public GameObject ScriptHolder;
    private int Checked = 0;
    // Start is called before the first frame update
    void Start()
    {
        LayoutNum = int.Parse(transform.parent.name);
        Debug.Log(LayoutNum);
        ScriptHolder = GameObject.Find("ScriptHolder");

        if (transform.parent.position.x == 0)
        {

            //Debug.Log("we at 0 baws");
            transform.parent.position = new Vector3(0, 0, LayoutNum * 10);

        }

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount==1)
        {
            //Debug.Log(LayoutNum);
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 2f))
            {

                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                //Debug.Log("WE gotta hit capn" + "," + LayoutNum);
                AddtoFitness();
                
            }
            Checked++;

        }

        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[0, 0]);
    }

    void AddtoFitness()
    {

        int generation = (int) Math.Floor(transform.position.x/10);
        //Debug.Log(generation + "gen");

        ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation,LayoutNum] += 1;
        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation, LayoutNumber] + "<< Fitness" + LayoutNum + generation);

    }
}
