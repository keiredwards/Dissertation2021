using System;
using System.Collections;
using System.Collections.Generic;
using SWS;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class SocialDistanceCheck : MonoBehaviour
{
    private GameObject ScriptHolder;

    // Start is called before the first frame update
    void Start()
    {
        ScriptHolder = GameObject.Find("ScriptHolder");
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount == 100)
        {
            ScriptHolder.GetComponent<Counter>().SocialDistanceCount = 0;
            //Debug.Log(this.name);
            if (this.name.Contains("1_3 Shopper"))
            {
                this.GetComponent<NavMeshAgent>().avoidancePriority =
                    ScriptHolder.GetComponent<GeneratePath>().Priority;
                ScriptHolder.GetComponent<GeneratePath>().Priority += 1;
                this.GetComponent<navMove>().startPoint = Random.Range(0,GameObject.Find("1/3").transform.childCount);
                this.GetComponent<navMove>().pathContainer = GameObject.Find("1/3").GetComponent<PathManager>();
            }
            else if (this.name.Contains("1_5 Shopper"))
            {
                this.GetComponent<NavMeshAgent>().avoidancePriority =
                    ScriptHolder.GetComponent<GeneratePath>().Priority;
                ScriptHolder.GetComponent<GeneratePath>().Priority += 1;
                this.GetComponent<navMove>().startPoint = Random.Range(0,GameObject.Find("1/5").transform.childCount);
                this.GetComponent<navMove>().pathContainer = GameObject.Find("1/5").GetComponent<PathManager>();
            }

            Debug.Log(this.GetComponent<navMove>().startPoint);
            this.GetComponent<navMove>().StartMove();
        }
        else
        {
            //Debug.Log(Time.frameCount);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Shopper")
        {
            Debug.Log("violation");
        }

        ScriptHolder.GetComponent<Counter>().SocialDistanceCount += Time.deltaTime;
    }
    
    void UpdatePaths()
    {
        
    }
}
