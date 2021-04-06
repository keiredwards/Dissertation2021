using System;
using UnityEngine;
using UnityEngine.Serialization;

public class FitnessOneLongShelf : MonoBehaviour
{
    public GameObject scriptHolder;
    
    private int _generation;

    private int _layoutNum;

    // Start is called before the first frame update
    private void Start()
    {
        
        scriptHolder = GameObject.Find("ScriptHolder");
        _generation = scriptHolder.GetComponent<GeneticAlgorithm>().generation;
        //Debug.Log(_generation);
        //Debug.Log((transform.position.x) + "," + (scriptHolder.GetComponent<GeneticAlgorithm>().width));
        
        if (_generation < 0)
        {
            _generation = 0;
        }

        _layoutNum = int.Parse(transform.parent.name);


        RaycastHit hit;
        if ((Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out hit, 2f))&&(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit, 2f))&&(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 2f))&&(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out hit, 2f)))
        {

            AddtoFitness(1);
        }
        
        if(Physics.Raycast(transform.position, transform.TransformDirection(new Vector3(2,0,2)), out hit, 2f))
        {
            AddtoFitness(1);
        }
        GeneticAlgorithm.shelvesChecked[_generation] += 1;
    
    }

    // Update is called once per frame

    private void AddtoFitness(int addition)
    {
        //Debug.Log(generation + "gen");

        GeneticAlgorithm.FitnessScores[_generation, _layoutNum] += addition;
        //Debug.Log(ScriptHolder.GetComponent<GeneticAlgorithm>().FitnessScores[generation, LayoutNum] + "<< Fitness" + LayoutNum + generation);
    }
}