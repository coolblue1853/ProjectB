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
        TextAsset csvData = Resources.Load<TextAsset>(filePath); // Resources ������ �ִ� CSV ������ �ҷ��ɴϴ�.
        string[] lines = csvData.text.Split(new char[] { '\n' }); // CSV ���� ������ �� ������ �����մϴ�.

        // �� ������ �����͸� ó���մϴ�.
        for (int i = 2; i < lines.Length; i++) // ù ��° ���� Ÿ�� ����, �� ��° ���� �� �̸��̹Ƿ� 2���� �����մϴ�.
        {
            string[] values = lines[i].Split(','); // �� ������ �����͸� ��ǥ�� �����մϴ�.

            ItemData item = new ItemData();
            for (int j = 0; j < values.Length; j++)
            {
                // �� ���� Ÿ�Կ� ���� ������ ���·� ���� ��ȯ�Ͽ� ItemData ��ü�� �����մϴ�.
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
                        item.effectPow = int.Parse(values[j].Trim());
                        break;
                    case 10: // equipArea
                        item.equipArea = values[j].Trim();
                        break;
                        // �ʿ��� �ٸ� ���� ���� ó���� �߰��մϴ�.
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
    public int effectPow;
    public string equipArea;
}