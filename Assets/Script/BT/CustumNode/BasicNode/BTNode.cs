﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public abstract class BTNode : MonoBehaviour
{
    protected NodeState _nodeState;
    public BTBrain brain;
    public bool IsWaiting = false;
    public float waitTime = 0f; // 추가된 부분
    public NodeState nodeState { get { return _nodeState; } set { _nodeState = value; } }
    public abstract NodeState Evaluate();
    public Sequence sequence;
    public void StopTween()
    {
        sequence.Kill();
    }
}

public enum NodeState
{
    RUNNING, SUCCESS, FAILURE, WAIT,
}