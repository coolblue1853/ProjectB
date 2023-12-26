using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class BTBrain : MonoBehaviour
{
    public bool IsWaiting, isEnd, isAttacked, inRecognize;
    public bool nearPlayer;
   // [SerializeField]
   // public BTNode[] testNode;

    BTSequence test;
    public List<BTNode> node;
    public Coroutine evaluateCoroutine;
    public Vector2 originPosition;
    IEnumerator StartEvaluate()
    {
       test.Evaluate();
        yield return null;

    }
    void Start()
    {
        originPosition = transform.position;
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



    public void KillAllTweensForObject() // ��� Ʈ�� ����, �� ���� ����
    {
        foreach (var currentNode in node)
        {
            // ��尡 StopMoveX �Լ��� ������ �ִ��� Ȯ�� �� ȣ��
            if (currentNode != null)
            {
                BTNode bTNode = currentNode.GetComponent<BTNode>();
                if (bTNode != null)
                {
                    bTNode.StopTween();
                }
            }
        }
    }

}
