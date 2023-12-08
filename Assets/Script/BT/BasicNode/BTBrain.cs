using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTBrain : MonoBehaviour
{
    public bool IsWaiting, isEnd, isAttacked, inRecognize;
    public bool nearPlayer;
   // [SerializeField]
   // public BTNode[] testNode;

    BTSequence test;
    public List<BTNode> node;
    public Coroutine evaluateCoroutine;
    IEnumerator StartEvaluate()
    {
       test.Evaluate();
        yield return null;

    }
    void Start()
    {
          ConstructBehaviourTree();
           evaluateCoroutine = StartCoroutine(StartEvaluate());
    }

    void Update()
    {

        if(isEnd == true)
        {
            isEnd = false;
            evaluateCoroutine = StartCoroutine(StartEvaluate());

        }
    }

    public void StartEvaluateCoroutine()
    {

        evaluateCoroutine = StartCoroutine(StartEvaluate());
    }
    public void StopEvaluateCoroutine()
    {
        // aIPath.enabled = false;
        StopCoroutine(evaluateCoroutine);
    }
    private void ConstructBehaviourTree()
    {
        for (int i = 0; i < node.Count; i++)
        {
            node[i].brain = this.transform.GetComponent<BTBrain>();
        }
        test = new BTSequence(node);

    }

    public void EndNode()
    {
        StopCoroutine(evaluateCoroutine);
        for(int i = 0; i < node.Count; i++)
        {
            node[i].IsWaiting = false;
        }
        isEnd = true;

    }

    public void restartEvaluate()
    {
        evaluateCoroutine = StartCoroutine(StartEvaluate());

    }
}
