using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ItemDatabase : MonoBehaviour
{
    public List<ItemData> items = new List<ItemData>();

    void Start()
    {
        LoadItemsFromCSV("ItemSheet");
    }

    void LoadItemsFromCSV(string filePath)
    {
        TextAsset csvData = Resources.Load<TextAsset>(filePath); // Resources 폴더에 있는 CSV 파일을 불러옵니다.
        string[] lines = csvData.text.Split(new char[] { '\n' }); // CSV 파일 내용을 줄 단위로 분할합니다.

        // 각 라인의 데이터를 처리합니다.
        for (int i = 2; i < lines.Length; i++) // 첫 번째 줄은 타입 정의, 두 번째 줄은 열 이름이므로 2부터 시작합니다.
        {
            string[] values = lines[i].Split(','); // 각 라인의 데이터를 쉼표로 분할합니다.

            ItemData item = new ItemData();
            for (int j = 0; j < values.Length; j++)
            {
                // 각 열의 타입에 따라 적절한 형태로 값을 변환하여 ItemData 객체에 저장합니다.
                switch (j)
                {
                    case 0: // index
                        item.index = int.Parse(values[j].Trim());
                        break;
                    case 1: // name
                        item.name = values[j].Trim();
                        break;
                    case 2: // type
                        item.type = values[j].Trim();
                        break;
                    case 3: // description
                        item.description = values[j].Trim();
                        break;
                    case 4: // price
                        item.price = int.Parse(values[j].Trim());
                        break;
                    case 5: // weight
                        item.weight = int.Parse(values[j].Trim());
                        break;
                    case 6: // acqPath
                        item.acqPath = values[j].Trim();
                        break;
                    case 7: // maxStack
                        item.maxStack = int.Parse(values[j].Trim());
                        break;
                    case 8: // effectOb
                        item.effectOb = values[j].Trim();
                        break;
                    case 9: // effectPow
                        item.effectPow = (values[j].Trim());
                        break;
                    case 10: // equipArea
                        item.equipArea = values[j].Trim();
                        break;
                    case 11: // tfName
                        item.tfName = values[j].Trim();
                        break;
                    case 12: // itemName
                        item.itemNameT = values[j].Trim();
                        break;
                    case 13: // tear
                        item.tear = int.Parse(values[j].Trim());
                        break;
                    case 14: // rarity
                        item.rarity = values[j].Trim();
                        break;
                    case 15: // upgrade
                        item.upgrade = int.Parse(values[j].Trim());
                        break;
                    case 16: // upgrade
                        item.setName =(values[j].Trim());
                        break;
                        // 필요한 다른 열에 대한 처리를 추가합니다.
                }
            }

            items.Add(item);
        }
    }
}

public class ItemData
{
    public int index;
    public string name;
    public string type;
    public string description;
    public int price;
    public int weight;
    public string acqPath;
    public int maxStack;
    public string effectOb;
    public string effectPow;
    public string equipArea;
    public string tfName;
    public string itemNameT;
    public int tear;
    public string rarity;
    public int upgrade;
    public string setName;
}
