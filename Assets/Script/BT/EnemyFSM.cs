using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public List<BTBrain> brain;
    public List<string> state;

    public string nowState;
    [HideInInspector]
    public string beforeState;
    Rigidbody2D rd;

    // Start is called before the first frame update
    void Start()
    {
        rd = this.GetComponent<Rigidbody2D>();
        if (brain[0] != null)
        {
            nowState = state[0];
            beforeState = nowState;
            brain[0].restartEvaluate();
            brain[0].brainActive = true;
        }
    }


    public void StateChanger(string InputState)
    {
        Debug.Log("ChaneState to : "+InputState);
        nowState = InputState;

    }
    void BTBrainActiver()
    {
        for (int i = 0; i < state.Count; i++)
        {
            if (state[i] != nowState)
            {
                brain[i].StopEvaluateCoroutine();
                brain[i].brainActive = false;
            }
        }
        for (int i = 0; i < state.Count; i++)
        {
            if(state[i] == nowState)
            {
                brain[i].restartEvaluate();
                brain[i].brainActive = true;
            }
        }

    }

    public void KillBrainSequence(bool isStiff = false)
    {
        if(brain != null)
        {
            for (int i = 0; i < state.Count; i++)
            {
                if (state[i] == nowState)
                {
                    if(isStiff == false)
                    {
                        brain[i].isAttacked = true;
                    }
                    brain[i].OnlyEndNode();
                    brain[i].StopEvaluateCoroutine();
                    brain[i].KillAllTweensForObject();
                    brain[i].brainActive = false;
                }
            }
        }

    }
   public  bool isNeedReset;
    public void ReActiveBrainSequence()
    {
        isNeedReset = true;

    }

    public bool CheckBrainActive()
    {
        for (int i = 0; i < state.Count; i++)
        {
            if (brain[i].brainActive == true)
            {
                return true;
            }
        }

        return false;
    }
    // Update is called once per frame
    void Update()
    {/*
        if(beforeState != nowState)
        {
            beforeState = nowState;
         //   BTBrainActiver();
        }
        */
        if(isNeedReset == true && CheckBrainActive() == false)
        {
            isNeedReset = false;
            for (int i = 0; i < state.Count; i++)
            {
                brain[i].isAttacked = false;
                if (state[i] == nowState)
                {
                    Debug.Log("현재state는 " + state[i]);
                    Debug.Log("현재brain는 " + brain[i]);

                    brain[i].restartEvaluate();
                    brain[i].brainActive = true;
                }
            }
        }
    }
}
