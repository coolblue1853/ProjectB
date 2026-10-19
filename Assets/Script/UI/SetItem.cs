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
            SetEffectApplier.Apply(item.effect, true, "Set:" + name + "_" + level);
        }
    }
    public void CheckSetDisEffect(string name, int level)
    {
        SetData item = DatabaseManager.instance.LoadSetsData(DatabaseManager.instance.FindSetsDataIndex(name + "_" + level));
        if (item != null)
        {
            SetEffectApplier.Apply(item.effect, false, "Set:" + name + "_" + level);
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
    /// <summary>세트 효과 적용/해제 — SetEffectApplier로 위임 (적용·해제가 같은 경로라 비대칭 없음)</summary>
    public void ActiveSetEffect(string effect, bool isEffect)
    {
        SetEffectApplier.Apply(effect, isEffect);
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

