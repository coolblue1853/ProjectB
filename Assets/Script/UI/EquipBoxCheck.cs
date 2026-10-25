using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using DG.Tweening;
using AnyPortrait;
public class EquipBoxCheck : MonoBehaviour
{
    public string equipArea = "";
    string reciveEquipArea;
    public GameObject equipPrefab;
    public AttackManager attackManager; // 무기 장착을 위한  AttackManager
    GameObject instantiatedPrefab;
    Weapon weapon;
    Equipment[] equipmentList = new Equipment[10];
    public GameObject nowBox;
    public Texture2D defaultOffset;
    public apPortrait mainCharacter;
    PlayerController _pc;
    /// <summary>활성 캐릭터 리그 — 방어구 외형은 리그가 처리한다(픽셀 리그는 아직 무시)</summary>
    CharacterRig Rig
    {
        get
        {
            if (_pc == null)
            {
                _pc = FindFirstObjectByType<PlayerController>();
            }
            return _pc != null ? _pc.Rig : null;
        }
    }

    public void EquipMainWeapon()
    {
        attackManager.equipWeapon = weapon;
        attackManager.EquipMainWeaopon();
    }
    int CheckEquipBoxNum(string equipArea, bool isChangeTexture = false, Texture2D texture = null)
    {
        switch (equipArea)
        {
            case "Ring":
                return  1;
            case "Head":
                if (isChangeTexture)
                {
                    Rig.SetMeshImage("Hat", texture);
                }
                return  2;
            case "Necklace":
                return  3;
            case "Chest":
                if (isChangeTexture)
                {
                    Rig.SetMeshImage("RArm", texture);
                    Rig.SetMeshImage("Rsholder", texture);
                    Rig.SetMeshImage("LArm", texture);
                    Rig.SetMeshImage("LSholder", texture);
                    Rig.SetMeshImage("UppderBody", texture);
                    Rig.SetMeshImage("DwonBody", texture);
                }
                return  5;
            case "Hand":
                if (isChangeTexture)
                {
                    Rig.SetMeshImage("RHand", texture);
                    Rig.SetMeshImage("LHand", texture);
                }
                return  6;
            case "Weapon":
                return  7;
            case "Leg":
                if (isChangeTexture)
                {
                    Rig.SetMeshImage("RDownLeg", texture);
                    Rig.SetMeshImage("RUpperLeg", texture);
                    Rig.SetMeshImage("LDownLeg", texture);
                    Rig.SetMeshImage("LUpperLeg", texture);
                }
                return  8;
            case "Shoes":
                if (isChangeTexture)
                {
                    Rig.SetMeshImage("RFoot", texture);
                    Rig.SetMeshImage("LFoot", texture);
                }
                return  9;
            default:
                return 0;
        }
    }
    public void SaveEquipItem(ItemCheck itemCheck, bool isSave) // 저장할지 삭제할지.
    {
        string[] gear = new string[] { itemCheck.itemData.name , itemCheck.itemData.tear.ToString() , itemCheck.itemData.upgrade.ToString() };
        int nowEquipNum = CheckEquipBoxNum(itemCheck.itemData.equipArea); // 현재 저장해야할 장비 번호
        if (isSave == true)
        {
            for(int i =0; i< 3; i++)
            {
                SaveManager.instance.datas.equipGear[nowEquipNum, i] = gear[i];
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                SaveManager.instance.datas.equipGear[nowEquipNum, i] = null;
            }
        }
    }
    public void ActivePrefab(string reciveEquipArea)
    {
        int equipNum = CheckEquipBoxNum(reciveEquipArea);
        if (equipPrefab == null)
        {
            GameObject ob = transform.GetChild(0).gameObject;
            ItemCheck obItemCheck = ob.GetComponent<ItemCheck>();
           LoadPrefab(obItemCheck.itemData.name, obItemCheck.itemData.equipArea, obItemCheck.itemData.tfName);
        }
        int enhance = transform.childCount > 0 && transform.GetChild(0).TryGetComponent(out ItemCheck slotItem) && slotItem.itemData != null ? slotItem.itemData.upgrade : 0;
        if (reciveEquipArea == "Weapon") // 무기는 Weapon 스크립트를 사용하므로 예외
        {
            if (weapon == null) // 이미 연결됐으면 다시 하지 않는다(두 번째 호출이 방어구 분기로 빠져 NRE 나던 문제)
            {
                weapon = equipPrefab.GetComponent<Weapon>();
                weapon.SetEnhance(enhance);
                attackManager.equipWeapon = weapon;
                attackManager.EquipMainWeaopon();
            }
        }
        else
        {
            if (equipmentList[equipNum] == null)
            {
                equipmentList[equipNum] = equipPrefab.GetComponent<Equipment>();
                equipmentList[equipNum].enhanceLevel = enhance;
                SetEquipment(equipmentList[equipNum]);
            }
        }
    }
    public void SetEquipmentSkillCool(Equipment equip)
    {
        for(int i =0; i < equip.coolDownSkill.Length; i++)
        {
            if (DatabaseManager.skillCoolDown.ContainsKey(equip.coolDownSkill[i].skillName)) // 기술이 존재하는지 확인. 존재한다면 그냥 수치 추가
            {
                DatabaseManager.skillCoolDown[equip.coolDownSkill[i].skillName] += equip.coolDownSkill[i].coolDownCount;
            }
            else
            {
                DatabaseManager.skillCoolDown.Add(equip.coolDownSkill[i].skillName, equip.coolDownSkill[i].coolDownCount);
            }
        }
    }
    public void DownquipmentSkillCool(Equipment equip)
    {
        for (int i = 0; i < equip.coolDownSkill.Length; i++)
        {
            DatabaseManager.skillCoolDown[equip.coolDownSkill[i].skillName] -= equip.coolDownSkill[i].coolDownCount;
            if(DatabaseManager.skillCoolDown[equip.coolDownSkill[i].skillName] <= 0)
            {
                DatabaseManager.skillCoolDown.Remove(equip.coolDownSkill[i].skillName);
            }
        }
    }
    public void SetEquipment(Equipment equip)
    {
        SetEquipmentSkillCool(equip);
        // 강화 배율을 적용한 값을 기록해 둔다 (HP 는 아직 레거시 경로라 해제 때 그대로 뺀다)
        equip.appliedBasicDmg = equip.Enhanced(equip.basicDmg);
        equip.appliedHp = equip.Enhanced(equip.hp);
        equip.appliedArmor = Mathf.RoundToInt(equip.Enhanced(equip.armor) * Equipment.ArmorToDef); // 방어구 방어력 반영 (B-212)
        // 정수 스탯은 이 장비 인스턴스를 출처로 하는 모디파이어 — 해제는 RemoveBySource 한 줄 (PlayerStateDesign Phase 10a)
        PlayerStats.RemoveBySource(equip); // 같은 장비가 두 번 적용돼도 중복되지 않게
        AddEquipStat(equip, StatId.DropRate, equip.dropRate);
        AddEquipStat(equip, StatId.CritRate, equip.critical);
        AddEquipStat(equip, StatId.MoveSpeed, equip.moveSpeed);
        AddEquipStat(equip, StatId.AttackSpeed, equip.attSpeed);
        AddEquipStat(equip, StatId.CritDmg, equip.criticalDmg);
        AddEquipStat(equip, StatId.BasicDmg, equip.appliedBasicDmg);
        AddEquipStat(equip, StatId.Def, equip.appliedArmor);
        AddEquipStat(equip, StatId.BleedIncDmg, equip.bleedingDmgPer);
        AddEquipStat(equip, StatId.IncomingDmg, equip.addIncomingDmg);
        AddEquipStat(equip, StatId.IncDmg, equip.incDmg);
        AddEquipStat(equip, StatId.PoisonDmg, equip.poisonDmg);
        PlayerHealthManager.Instance.EquipmentActiveTrue(equip.appliedHp);
        GameLog.Info(GameLog.Cat.Item, "장비 적용 " + equip.name + " +" + equip.enhanceLevel + " hp=" + equip.appliedHp + " def=" + equip.appliedArmor + " atk=" + equip.appliedBasicDmg);
        // 출혈관련
        if (equip.isBleeding)
        {
            // 같은 이름 장비 2개를 껴도 충돌하지 않도록 인스턴스별 키 사용 (B-107)
            DatabaseManager.bleedingEquipment[BleedKey(equip)] = new int[] { equip.bleedingPerCent, equip.bleedingDamage, equip.bleedingDamageCount };
            DatabaseManager.bleedingEquipmentInterval[BleedKey(equip)] = equip.bleedingDamageInterval;
        }
    }
    public void DisableEquipment(Equipment equip)
    {
        DownquipmentSkillCool(equip);
        PlayerStats.RemoveBySource(equip);
        PlayerHealthManager.Instance.EquipmentActiveFalse(equip.appliedHp);
        equip.appliedBasicDmg = equip.appliedHp = equip.appliedArmor = 0;
        if (equip.isBleeding)
        {
            DatabaseManager.bleedingEquipment.Remove(BleedKey(equip));
            DatabaseManager.bleedingEquipmentInterval.Remove(BleedKey(equip));
        }
    }

    static void AddEquipStat(Equipment equip, StatId id, int value)
    {
        if (value != 0)
        {
            PlayerStats.Add(new StatModifier(id, ModType.Add, value) { Source = equip, Owner = equip.name });
        }
    }

    static string BleedKey(Equipment equip) => equip.name + "#" + equip.GetHashCode();

    GameObject deletChile ;
    public void DeletPrefab(ItemCheck detail, string reciveEquipArea, bool isFalse= true)
    {
        DatabaseManager.MinusSetDict(detail.itemData.tfName, 1);
        // 활성화 전(장비창 닫힘)에 해제되면 아래 경로가 프리팹을 못 찾으므로 여기서 파괴한다. 세트 수는 위에서 이미 뺐다.
        // 파괴는 프레임 끝이라 참조를 먼저 끊어 LoadPrefab 이 '대기 중 로드분'으로 다시 세지 않게 한다 (B-128)
        if (equipPrefab != null && !IsActivated(equipPrefab))
        {
            Destroy(equipPrefab);
        }
        equipPrefab = null;
        loadedTfName = null;
        if (reciveEquipArea == "Weapon") // 무기는 예외
        {
            if (isFalse == false)
            {
                attackManager.UnEquipMainWeaopon();
                Sequence waitSequence = DOTween.Sequence()
                .AppendCallback(() => attackManager.equipWeapon = null)
                .AppendCallback(() => { if (weapon != null) { Destroy(weapon.gameObject); } }) // 장비창이 닫혀 활성화 전이면 weapon 이 없다
                .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
                .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (weapon != null)
                {
                    attackManager.UnEquipMainWeaopon();
                    attackManager.equipWeapon = null;
                    Destroy(weapon.gameObject);
                    weapon = null;
                }
            }
        }
        else
        {
            int equipNum = CheckEquipBoxNum(reciveEquipArea, true, defaultOffset);
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
                .AppendCallback(() => { if (equipmentList[equipNum] != null) { DisableEquipment(equipmentList[equipNum]); } })
                .AppendCallback(() => { if (equipmentList[equipNum] != null) { Destroy(equipmentList[equipNum].gameObject); } })
                .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
                .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (equipmentList[equipNum] != null)
                {
                    DisableEquipment(equipmentList[equipNum]);
                    Destroy(equipmentList[equipNum].gameObject);
                    equipmentList[equipNum] = null;
                }
            }
        }
    }
    private void Awake()
    {
        nowBox = this.gameObject;
    }
    public void LoadPrefab(string equipName, string equipArea, string tfName)
    {
        ReleasePendingPrefab();
        loadedTfName = tfName;
        if (!string.IsNullOrEmpty(tfName)) // 세트가 아닌 장비는 세트 카운트에 넣지 않는다 (B-112)
            DatabaseManager.PlusSetDict(tfName, 1);
        reciveEquipArea = equipArea;

        string folderPath = equipArea+"/";
        GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);        // 리소스 폴더 내의 equipName을 로드합니다.
        if(equipArea != "Weapon")
        {
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);
            int equipNum = CheckEquipBoxNum(reciveEquipArea, true, sprite);

        }
        if (prefab != null)
        {
            instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);
            equipPrefab = instantiatedPrefab;
        }
        else
        {
            Debug.LogError("Failed to load prefab: " + equipName);
        }
    }

    // LoadPrefab 이 세트 수에 더한 키. 활성화 전에 다시 로드되면 되돌린다 (B-128)
    string loadedTfName;

    /// <summary>
    /// 이전 LoadPrefab 결과가 아직 ActivePrefab 으로 활성화되지 않았으면(장비창이 닫혀 Update 가 안 돈 채 다시 장착) 파괴하고 세트 수를 되돌린다.
    /// 없으면 같은 세트 무기를 바꿔 낄 때마다 세트 스택·무기 프리팹이 쌓였다 (B-128).
    /// </summary>
    void ReleasePendingPrefab()
    {
        if (equipPrefab == null || IsActivated(equipPrefab))
        {
            return;
        }
        DatabaseManager.MinusSetDict(loadedTfName, 1);
        Destroy(equipPrefab);
        equipPrefab = null;
        loadedTfName = null;
    }

    bool IsActivated(GameObject go)
    {
        if (weapon != null && weapon.gameObject == go)
        {
            return true;
        }
        foreach (Equipment e in equipmentList)
        {
            if (e != null && e.gameObject == go)
            {
                return true;
            }
        }
        return false;
    }

    public bool isSetArray =  false;
    bool isFalse = false;
    public int siblingIndex;
    private void Update()
    {
        if (this.transform.childCount > 0 && isSetArray == false)
        {
            isSetArray = true;
            ActivePrefab(reciveEquipArea);
        }
        else if (this.transform.childCount <= 0 && isSetArray == true)
        {
            isSetArray = false;
        }
    }   
}