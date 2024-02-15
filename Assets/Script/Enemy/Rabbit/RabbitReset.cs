using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitReset : MonoBehaviour
{
    public RabbitHole rabbitHole;

    public void ResetRabbit()
    {
        rabbitHole.ResetRabbit();
        Destroy(this.gameObject);
    }
}
