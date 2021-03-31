using System;
using UnityEngine;
using UnityEngine.Serialization;

public class FitnessOneLongShelf : MonoBehaviour
{
    [FormerlySerializedAs("ScriptHolder")] public GameObject scriptHolder;
    private int _generation;

    private int _layoutNum;

    // Start is called before the first frame update
    private void Start()
    {
        _generation = (int) Math.Floor(transform.position.x / 10);
        _layoutNum = int.Parse(transform.parent.name);
        //Debug.Log(LayoutNum);
        scriptHolder = GameObject.Find("ScriptHolder");
        //Debug.Log("gen:" + generation + "layout" + LayoutNum);


        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 2f))
        {
            //Debug.Log("AddtoFitness Called" + "Gen:" + generation + "Layout:" + LayoutNum);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
            //Debug.Log("WE gotta hit capn" + "," + LayoutNum);
            AddtoFitness();
        }

        
        GeneticAlgorithm.shelvesChecked[_generation] += 1;
        //Debug.Log(GeneticAlgorithm.shelvesChecked[_generation]);
        
        if (GeneticAlgorithm.shelvesChecked[_generation] == scriptHolder.GetComponent<GeneticAlgorithm>().totalShelves &&
            _generation < scriptHolder.GetComponent<GeneticAlgorithm>().maxGenerations - 1)
            
            //Debug.Log(generation);
            scriptHolder.GetComponent<GeneticAlgorithm>().Run(_generation);
        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation, 0]);
        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation, 1]);
        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation, 2]);
        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation, 3]);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.frameCount == 1)
        {
            //Debug.Log(LayoutNum);
        }

        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[0, 0]);
    }

    private void AddtoFitness()
    {
        //Debug.Log(generation + "gen");

        scriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[_generation, _layoutNum] += 1;
        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation, LayoutNum] + "<< Fitness" + LayoutNum + generation);
    }
}