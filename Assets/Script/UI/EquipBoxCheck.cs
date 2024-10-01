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
                    mainCharacter.SetMeshImage("Hat", texture);
                }
                return  2;
            case "Necklace":
                return  3;
            case "Chest":
                if (isChangeTexture)
                {
                    mainCharacter.SetMeshImage("RArm", texture);
                    mainCharacter.SetMeshImage("Rsholder", texture);
                    mainCharacter.SetMeshImage("LArm", texture);
                    mainCharacter.SetMeshImage("LSholder", texture);
                    mainCharacter.SetMeshImage("UppderBody", texture);
                    mainCharacter.SetMeshImage("DwonBody", texture);
                }
                return  5;
            case "Hand":
                if (isChangeTexture)
                {
                    mainCharacter.SetMeshImage("RHand", texture);
                    mainCharacter.SetMeshImage("LHand", texture);
                }
                return  6;
            case "Weapon":
                return  7;
            case "Leg":
                if (isChangeTexture)
                {
                    mainCharacter.SetMeshImage("RDownLeg", texture);
                    mainCharacter.SetMeshImage("RUpperLeg", texture);
                    mainCharacter.SetMeshImage("LDownLeg", texture);
                    mainCharacter.SetMeshImage("LUpperLeg", texture);
                }
                return  8;
            case "Shoes":
                if (isChangeTexture)
                {
                    mainCharacter.SetMeshImage("RFoot", texture);
                    mainCharacter.SetMeshImage("LFoot", texture);
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
        if (reciveEquipArea == "Weapon" && weapon == null) // 무기는 Weapon 스크립트를 사용하므로 예외
        {
             weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = weapon;
            attackManager.EquipMainWeaopon();
        }
        else
        {
            if (equipmentList[equipNum] == null)
            {
                equipmentList[equipNum] = equipPrefab.GetComponent<Equipment>();
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
        DatabaseManager.playerDropRate += equip.dropRate;
        DatabaseManager.playerCritRate += equip.critical;
        DatabaseManager.SpeedBuff += equip.moveSpeed;
        DatabaseManager.attackSpeedBuff += equip.attSpeed;
        DatabaseManager.playerCritDmgRate += equip.criticalDmg;
        DatabaseManager.addbasicDmg += equip.basicDmg;
        PlayerHealthManager.Instance.EquipmentActiveTrue(equip.hp);
        DatabaseManager.bleedingAddDmg += equip.bleedingDmgPer;
        DatabaseManager.incIncomingDmg += equip.addIncomingDmg;
        DatabaseManager.incDmg += equip.incDmg;
        DatabaseManager.addPoisonDmg += equip.poisonDmg;
        // 출혈관련
        if (equip.isBleeding)
        {
            DatabaseManager.bleedingEquipment.Add(equip.name, new int[] { equip.bleedingPerCent, equip.bleedingDamage, equip.bleedingDamageCount });
            DatabaseManager.bleedingEquipmentInterval.Add(equip.name, equip.bleedingDamageInterval);
        }
    }
    public void DisableEquipment(Equipment equip)
    {
        DownquipmentSkillCool(equip);
        DatabaseManager.playerDropRate -= equip.dropRate;
        DatabaseManager.playerCritRate -= equip.critical;
        DatabaseManager.SpeedBuff -= equip.moveSpeed;
        DatabaseManager.attackSpeedBuff -= equip.attSpeed;
        DatabaseManager.playerCritDmgRate -= equip.criticalDmg;
        DatabaseManager.addbasicDmg -= equip.basicDmg;
        PlayerHealthManager.Instance.EquipmentActiveFalse(equip.hp);
        DatabaseManager.bleedingAddDmg -= equip.bleedingDmgPer;
        DatabaseManager.incIncomingDmg -= equip.addIncomingDmg;
        DatabaseManager.incDmg -= equip.incDmg;
        DatabaseManager.addPoisonDmg -= equip.poisonDmg;
        if (equip.isBleeding)
        {
            DatabaseManager.bleedingEquipment.Remove(equip.name);
            DatabaseManager.bleedingEquipmentInterval.Remove(equip.name);
        }
    }

    GameObject deletChile ;
    public void DeletPrefab(ItemCheck detail, string reciveEquipArea, bool isFalse= true)
    {
        DatabaseManager.MinusSetDict(detail.itemData.tfName, 1);
        if (reciveEquipArea == "Weapon") // 무기는 예외
        {
            if (isFalse == false)
            {
                attackManager.UnEquipMainWeaopon();
                Sequence waitSequence = DOTween.Sequence()
                .AppendCallback(() => attackManager.equipWeapon = null)
                .AppendCallback(() => Destroy(weapon.gameObject))
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
                .AppendCallback(() => DisableEquipment(equipmentList[equipNum]))
                .AppendCallback(() => Destroy(equipmentList[equipNum].gameObject))
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
        if(tfName != null)
         DatabaseManager.PlusSetDict(tfName,1);
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