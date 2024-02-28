using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CraftingManager : MonoBehaviour
{

    public GameObject[] LV1;
    CraftItemCheck nowCraftItem;
    public TextMeshProUGUI name;
    public TextMeshProUGUI type;
    public TextMeshProUGUI description;
    public TextMeshProUGUI price;
    public TextMeshProUGUI weight;
    public TextMeshProUGUI acqPath;
    public TextMeshProUGUI effectOb;
    public TextMeshProUGUI effectPow;
    public TextMeshProUGUI equipArea;
    public Image image;
    // Start is called before the first frame update
    void Start()
    {
        nowCraftItem = LV1[0].transform.GetChild(0).GetComponent<CraftItemCheck>();
        SetDetail();
    }

    void SetDetail()
    {
        name.text = nowCraftItem.name;
        type.text = nowCraftItem.type;
        description.text = nowCraftItem.description;
        price.text =(nowCraftItem.price).ToString();
        weight.text = nowCraftItem.weight.ToString();
        acqPath.text = nowCraftItem.acqPath;
        image.sprite = nowCraftItem.image.sprite;
        if (type.text == "Consum")
        {
          //  effectOb.text = nowCraftItem.effectOb;
         //   effectPow.text = nowCraftItem.effectPow.ToString();
        }
        if (type.text == "Equip")
        {
          //  equipArea.text = nowCraftItem.equipArea;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
