using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetItem : MonoBehaviour
{
    public List<SetData> items = new List<SetData>();
    public string setName;
    public int level;
    public string effect;

    private void Awake()
    {

    }
    void Start()
    {

        LoadSetFromCSV("SetItem");
    }


    public void CheckSetEffect(string name, int level)
    {
        SetData item = DatabaseManager.instance.LoadSetsData(DatabaseManager.instance.FindSetsDataIndex(name+"_"+level));
        if(item != null)
        {
            ActiveSetEffect(item.effect, true);
        }
    }
    public void CheckSetDisEffect(string name, int level)
    {
        SetData item = DatabaseManager.instance.LoadSetsData(DatabaseManager.instance.FindSetsDataIndex(name + "_" + level));
        if (item != null)
        {
            ActiveSetEffect(item.effect,false);
        }
    }

    public string SetEffectStr(string name, int level) //�ܺο��� ��Ʈ �������� ȿ���� Ȯ���� �� �ִ� �׸�.
    {
        SetData item = DatabaseManager.instance.LoadSetsData(DatabaseManager.instance.FindSetsDataIndex(name + "_" + level));
        if (item != null)
        {
            return item.effect;
        }
        return "";
    }
    public void ActiveSetEffect(string effect, bool isEffect)
    {
        string[] effectList = effect.Split("/");

        foreach(var value in effectList)
        {

            string[] sprateStr = value.Split(")");
            string sign = sprateStr[0].Substring(1,1);
            string[] effectSub = sprateStr[1].Split(" ");
            if(isEffect == true)
            {
                switch (effectSub[1])
                {
                    case ("Def"):
                        if (sign == "+") DatabaseManager.playerDef += int.Parse(effectSub[0]);
                        break;
                    case ("Hp"):
                        if (sign == "+") PlayerHealthManager.Instance.EquipmentActiveTrue(int.Parse(effectSub[0]));
                        break;
                }
            }
            else
            {
                switch (effectSub[1])
                {
                    case ("Def"):
                        if (sign == "+") DatabaseManager.playerDef -= int.Parse(effectSub[0]);
                        break;
                    case ("Hp"):
                        if (sign == "+") PlayerHealthManager.Instance.EquipmentActiveFalse(int.Parse(effectSub[0]));
                        break;
                }
            }

        }
    }






    void LoadSetFromCSV(string filePath)
    {
        TextAsset csvData = Resources.Load<TextAsset>(filePath); // Resources ������ �ִ� CSV ������ �ҷ��ɴϴ�.
        string[] lines = csvData.text.Split(new char[] { '\n' }); // CSV ���� ������ �� ������ �����մϴ�.

        // �� ������ �����͸� ó���մϴ�.
        for (int i = 2; i < lines.Length; i++) // ù ��° ���� Ÿ�� ����, �� ��° ���� �� �̸��̹Ƿ� 2���� �����մϴ�.
        {
            string[] values = lines[i].Split(','); // �� ������ �����͸� ��ǥ�� �����մϴ�.

            SetData setitem = new SetData();
            for (int j = 0; j < values.Length; j++)
            {
                // �� ���� Ÿ�Կ� ���� ������ ���·� ���� ��ȯ�Ͽ� ItemData ��ü�� �����մϴ�.
                switch (j)
                {
                    case 0: // index
                        setitem.setName = values[j].Trim();
                        break;
                    case 1: // name
                        setitem.effect = values[j].Trim();
                        break;
                }
            }

            items.Add(setitem);
        }
    }
}

public class SetData
{
    public string setName;
    public string effect;

}

