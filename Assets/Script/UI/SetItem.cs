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

    public string SetEffectStr(string name, int level) //외부에서 세트 아이템의 효과를 확인할 수 있는 항목.
    {
        SetData item = DatabaseManager.instance.LoadSetsData(DatabaseManager.instance.FindSetsDataIndex(name + "_" + level));
        if (item != null)
        {
            return item.effect;
        }
        return "";
    }
    string skillName = "";
    public void ActiveSetEffect(string effect, bool isEffect)
    {
        string[] effectList = effect.Split("/");

        foreach(var value in effectList)
        {

            string[] sprateStr = value.Split(")");
            string sign = sprateStr[0].Substring(1,1);
            string[] effectSub = sprateStr[1].Split(" ");
            if(effectSub[1] == "skillHitCount" || effectSub[1] == "bulletCount" || effectSub[1] == "coolDown") // 스킬 이름이 필요한 경우 여기에 추ㄱ해 줘야함
            {
                 skillName = effectSub[2].Replace("_", " ");
            }
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
                    case ("skillHitCount"): // 특정 스킬의 생성 숫자 증가
                        if (sign == "+")
                        {

                            if (DatabaseManager.skillHitCount.ContainsKey(skillName))
                            {
                                DatabaseManager.skillHitCount[skillName] += int.Parse(effectSub[0]);
                            }
                            else
                            {
                                DatabaseManager.skillHitCount.Add(skillName, int.Parse(effectSub[0]));
                            }
                        }
                        break;
                    case ("coolDown"): // 특정 스킬의 생성 숫자 증가
                        if (sign == "+")
                        {
                            if (DatabaseManager.skillCoolDown.ContainsKey(skillName))
                            {
                                DatabaseManager.skillCoolDown[skillName] += int.Parse(effectSub[0]);
                            }
                            else
                            {
                                DatabaseManager.skillCoolDown.Add(skillName, int.Parse(effectSub[0]));
                            }
                        }
                        break;
                    case ("bulletCount"): // 특정 스킬의 생성 숫자 증가, 스킬 이름이 필요한 경우는 위에 ||에 해당 파트를 추가해 줘야함
                        if (sign == "+")
                        {
                            
                            if (DatabaseManager.skillBulletCount.ContainsKey(skillName))
                            {
                                DatabaseManager.skillBulletCount[skillName] += int.Parse(effectSub[0]);
                            }
                            else
                            {
                                DatabaseManager.skillBulletCount.Add(skillName, int.Parse(effectSub[0]));
                            }
                        }
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
                    case ("skillHitCount"): // 특정 스킬의 생성 숫자 증가
                        if (sign == "+")
                        {
                            DatabaseManager.skillHitCount[skillName] -= int.Parse(effectSub[0]);
                            if (DatabaseManager.skillHitCount[skillName] <= 0)
                            {
                                DatabaseManager.skillHitCount.Remove(skillName);
                            }
                        }
                        break;
                    case ("bulletCount"): // 특정 스킬의 생성 숫자 증가
                        if (sign == "+")
                        {
                            DatabaseManager.skillBulletCount[skillName] -= int.Parse(effectSub[0]);
                            if (DatabaseManager.skillBulletCount[skillName] <= 0)
                            {
                                DatabaseManager.skillBulletCount.Remove(skillName);
                            }
                        }
                        break;
                    case ("coolDown"): // 특정 스킬의 생성 숫자 증가
                        if (sign == "+")
                        {
                            DatabaseManager.skillCoolDown[skillName] -= int.Parse(effectSub[0]);
                            if (DatabaseManager.skillCoolDown[skillName] <= 0)
                            {
                                DatabaseManager.skillCoolDown.Remove(skillName);
                            }
                        }
                        break;
                }
            }

        }
    }






    void LoadSetFromCSV(string filePath)
    {
        TextAsset csvData = Resources.Load<TextAsset>(filePath); // Resources 폴더에 있는 CSV 파일을 불러옵니다.
        string[] lines = csvData.text.Split(new char[] { '\n' }); // CSV 파일 내용을 줄 단위로 분할합니다.

        // 각 라인의 데이터를 처리합니다.
        for (int i = 2; i < lines.Length; i++) // 첫 번째 줄은 타입 정의, 두 번째 줄은 열 이름이므로 2부터 시작합니다.
        {
            string[] values = lines[i].Split(','); // 각 라인의 데이터를 쉼표로 분할합니다.

            SetData setitem = new SetData();
            for (int j = 0; j < values.Length; j++)
            {
                // 각 열의 타입에 따라 적절한 형태로 값을 변환하여 ItemData 객체에 저장합니다.
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

