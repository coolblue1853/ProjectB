using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCheck : MonoBehaviour
{
    public int itemNum;
    // Start is called before the first frame update
    void Start()
    {
        DatabaseManager.instance.LoadETCItemData(itemNum);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
