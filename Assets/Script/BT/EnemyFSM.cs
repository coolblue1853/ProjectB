using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public List<BTBrain> brain;
    public List<string> state;
    [HideInInspector]
    public string nowState;
    [HideInInspector]
    public string beforeState;
    // Start is called before the first frame update
    void Start()
    {
        nowState = state[0];
        beforeState = nowState;
        brain[0].restartEvaluate();
    }

    public void StateChanger(string InputState)
    {
        nowState = InputState;
    }
    void BTBrainActiver()
    {
        for (int i = 0; i < state.Count; i++)
        {
            if (state[i] != nowState)
            {
                brain[i].StopEvaluateCoroutine();
            }
        }
        for (int i = 0; i < state.Count; i++)
        {
            if(state[i] == nowState)
            {
                brain[i].restartEvaluate();
            }
        }

    }

    public void KillBrainSequence()
    {
        for (int i = 0; i < state.Count; i++)
        {
            if (state[i] == nowState)
            {
                brain[i].isAttacked = true;
                brain[i].StopEvaluateCoroutine();
                brain[i].KillAllTweensForObject();
            }
        }
    }
    public void ReActiveBrainSequence()
    {
        for (int i = 0; i < state.Count; i++)
        {
            if (state[i] == nowState && beforeState == nowState)
            {
                brain[i].restartEvaluate();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(beforeState != nowState)
        {
            beforeState = nowState;
            BTBrainActiver();
        }
    }
}
