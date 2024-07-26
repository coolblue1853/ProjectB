using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using DG.Tweening;
using AnyPortrait;
public class EquipBoxCheck : MonoBehaviour, IPointerClickHandler
{
    public string equipArea = "";
    string reciveEquipArea;
    public GameObject equipPrefab;
    public AttackManager attackManager; // 무기 장착을 위한  AttackManager
    GameObject instantiatedPrefab;
    Weapon weapon;
    Equipment head;
    Equipment chest;
    Equipment leg;
    Equipment hand;
    Equipment necklace;
    Equipment ring;
    Equipment shoes;
    public GameObject nowBox;
    public Texture2D defaultOffset;
    public apPortrait mainCharacter;
    private void Start()
    {
    }
    public void EquipMainWeapon()
    {
       // weapon = equipPrefab.GetComponent<Weapon>();
        attackManager.equipWeapon = weapon;
        attackManager.EquipMainWeaopon();
    }
    public void SaveEquipItem(ItemCheck itemCheck, bool isSave) // 저장할지 삭제할지.
    {
        string[] gear = new string[] { itemCheck.name , itemCheck.tear.ToString() , itemCheck.upgrade.ToString() };
        if (itemCheck.equipArea == "Weapon")
        {
            if (isSave == true)
            {
                SaveManager.instance.datas.weaponGear = gear;
            }
            else
            {
                SaveManager.instance.datas.weaponGear = new string[3];
            }
        }
        else if (itemCheck.equipArea == "Head")
        {
            if(isSave == true) 
            {
                SaveManager.instance.datas.headGear = gear;
            }
            else
            {
                SaveManager.instance.datas.headGear = new string[3];
            }
        }
        else if (itemCheck.equipArea == "Chest")
        {
            if (isSave == true)
            {
                SaveManager.instance.datas.bodyGear = gear;
            }
            else
            {
                SaveManager.instance.datas.bodyGear = new string[3];
            }
        }
        else if (itemCheck.equipArea == "Leg")
        {
            if (isSave == true)
            {
                SaveManager.instance.datas.legGear = gear;
            }
            else
            {
                SaveManager.instance.datas.legGear = new string[3];
            }
        }
        else if (itemCheck.equipArea == "Hand")
        {
            if (isSave == true)
            {
                SaveManager.instance.datas.handGear = gear;
            }
            else
            {
                SaveManager.instance.datas.handGear = new string[3];
            }
        }
        else if (itemCheck.equipArea == "Necklace")
        {
            if (isSave == true)
            {
                SaveManager.instance.datas.necklesGear = gear;
            }
            else
            {
                SaveManager.instance.datas.necklesGear = new string[3];
            }
        }
        else if (itemCheck.equipArea == "Ring")
        {
            if (isSave == true)
            {
                SaveManager.instance.datas.ringGear = gear;
            }
            else
            {
                SaveManager.instance.datas.ringGear = new string[3];
            }
        } 
        else if (itemCheck.equipArea == "Shoes")
        {
            if (isSave == true)
            {
                SaveManager.instance.datas.shoseGear = gear;
            }
            else
            {
                SaveManager.instance.datas.shoseGear = new string[3];
            }
        }
    }
    public void ActivePrefab(string reciveEquipArea)
    {
        if(equipPrefab == null)
        {
            GameObject ob = transform.GetChild(0).gameObject;
            ItemCheck obItemCheck = ob.GetComponent<ItemCheck>();
           LoadPrefab(obItemCheck.name, obItemCheck.equipArea, obItemCheck.tfName);
        }
        if (reciveEquipArea == "Weapon" && weapon == null)
        {

             weapon = equipPrefab.GetComponent<Weapon>();
            attackManager.equipWeapon = weapon;
            attackManager.EquipMainWeaopon();

        }
        else if (reciveEquipArea == "Head" && head == null)
        {
            head = equipPrefab.GetComponent<Equipment>();
            SetEquipment(head);
        }
        else if (reciveEquipArea == "Chest" && chest == null)
        {
            chest = equipPrefab.GetComponent<Equipment>();
            SetEquipment(chest);
        }
        else if (reciveEquipArea == "Leg" && leg == null)
        {
            leg = equipPrefab.GetComponent<Equipment>();
            SetEquipment(leg);
        }
        else if (reciveEquipArea == "Hand" && hand == null)
        {
            hand = equipPrefab.GetComponent<Equipment>();
            SetEquipment(hand);
        }
        else if (reciveEquipArea == "Necklace" && hand == null)
        {
            necklace = equipPrefab.GetComponent<Equipment>();
            SetEquipment(necklace);
        }
        else if (reciveEquipArea == "Ring" && hand == null)
        {
            ring = equipPrefab.GetComponent<Equipment>();
            SetEquipment(ring);
        }
        else if (reciveEquipArea == "Shoes" && shoes == null)
        {
            shoes = equipPrefab.GetComponent<Equipment>();
            SetEquipment(shoes);
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
        DatabaseManager.MinusSetDict(detail.tfName, 1);
        if (reciveEquipArea == "Weapon")
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
         if (reciveEquipArea == "Head")
        {
            mainCharacter.SetMeshImage("Hat", defaultOffset);
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => DisableEquipment(head))
  .AppendCallback(() => Destroy(head.gameObject))
 .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
 .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if(head != null)
                {
                    DisableEquipment(head);
                    Destroy(head.gameObject);
                    head = null;
                }
    

            }

           

        }
         if (reciveEquipArea == "Chest")
        {
            mainCharacter.SetMeshImage("RArm", defaultOffset);
            mainCharacter.SetMeshImage("Rsholder", defaultOffset);
            mainCharacter.SetMeshImage("LArm", defaultOffset);
            mainCharacter.SetMeshImage("LSholder", defaultOffset);
            mainCharacter.SetMeshImage("UppderBody", defaultOffset);
            mainCharacter.SetMeshImage("DwonBody", defaultOffset);
            if (isFalse == false)
            {

                Sequence waitSequence = DOTween.Sequence()

.AppendCallback(() => DisableEquipment(chest))
.AppendCallback(() => Destroy(chest.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (chest != null)
                {
                    DisableEquipment(chest);
                    Destroy(chest.gameObject);
                    chest = null;
                }

            }



        }

         if (reciveEquipArea == "Leg")
        {
            mainCharacter.SetMeshImage("RDownLeg", defaultOffset);
            mainCharacter.SetMeshImage("RUpperLeg", defaultOffset);
            mainCharacter.SetMeshImage("LDownLeg", defaultOffset);
            mainCharacter.SetMeshImage("LUpperLeg", defaultOffset);
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => DisableEquipment(leg))
.AppendCallback(() => Destroy(leg.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (leg != null)
                {
                    DisableEquipment(leg);
                    Destroy(leg.gameObject);
                    leg = null;
                }

            }


        }
         if (reciveEquipArea == "Hand")
        {
            mainCharacter.SetMeshImage("RHand", defaultOffset);
            mainCharacter.SetMeshImage("LHand", defaultOffset);
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => DisableEquipment(hand))
    .AppendCallback(() => Destroy(hand.gameObject))
    .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
    .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (hand != null)
                {
                    DisableEquipment(hand);
                    Destroy(hand.gameObject);
                    hand = null;
                }

            }


        }
         if (reciveEquipArea == "Necklace")
        {
            if (isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => DisableEquipment(necklace))
    .AppendCallback(() => Destroy(necklace.gameObject))
    .AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
    .OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (necklace != null)
                {
                    DisableEquipment(necklace);
                    Destroy(necklace.gameObject);
                    necklace = null;
                }

            }

        }
         if (reciveEquipArea == "Ring")
        {
            if(isFalse == false)
            {
                Sequence waitSequence = DOTween.Sequence()

.AppendCallback(() => DisableEquipment(ring))
.AppendCallback(() => Destroy(ring.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (ring != null)
                {
                    DisableEquipment(ring);
                    Destroy(ring.gameObject);
                    ring = null;
                }

            }
        }
        if (reciveEquipArea == "Shoes")
        {
            if (isFalse == false)
            {
                mainCharacter.SetMeshImage("RFoot", defaultOffset);
                mainCharacter.SetMeshImage("LFoot", defaultOffset);
                Sequence waitSequence = DOTween.Sequence()
.AppendCallback(() => DisableEquipment(shoes))
.AppendCallback(() => Destroy(shoes.gameObject))
.AppendCallback(() => deletChile = nowBox.transform.GetChild(0).gameObject)
.OnComplete(() => Destroy(deletChile));
            }
            else
            {
                if (shoes != null)
                {
                    DisableEquipment(shoes);
                    Destroy(shoes.gameObject);
                    shoes = null;
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
        if (equipArea == "Weapon")
        {

            string folderPath = "Weapon/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }

      else if (equipArea == "Head")
        {
            string folderPath = "Head/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("Hat", sprite);
            }
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Chest")
        {
            string folderPath = "Chest/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath+ tfName);

            if(sprite != null)
            {
                mainCharacter.SetMeshImage("RArm", sprite);
                mainCharacter.SetMeshImage("Rsholder", sprite);
                mainCharacter.SetMeshImage("LArm", sprite);
                mainCharacter.SetMeshImage("LSholder", sprite);
                mainCharacter.SetMeshImage("UppderBody", sprite);
                mainCharacter.SetMeshImage("DwonBody", sprite);
            }
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Leg")
        {
            string folderPath = "Leg/";
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("RDownLeg", sprite);
                mainCharacter.SetMeshImage("RUpperLeg", sprite);
                mainCharacter.SetMeshImage("LDownLeg", sprite);
                mainCharacter.SetMeshImage("LUpperLeg", sprite);
            }
            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Hand")
        {
            string folderPath = "Hand/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("RHand", sprite);
                mainCharacter.SetMeshImage("LHand", sprite);

            }
            // 
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Necklace")
        {
            string folderPath = "Necklace/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Ring")
        {
            string folderPath = "Ring/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);

            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
        else if (equipArea == "Shoes")
        {
            string folderPath = "Shoes/";

            // 리소스 폴더 내의 equipName을 로드합니다.
            GameObject prefab = Resources.Load<GameObject>(folderPath + equipName);
            string texturePath = "Sprite/";
            Texture2D sprite = Resources.Load<Texture2D>(texturePath + tfName);

            if (sprite != null)
            {
                mainCharacter.SetMeshImage("RFoot", sprite);
                mainCharacter.SetMeshImage("LFoot", sprite);

            }
            if (prefab != null)
            {
                // Set the instantiated prefab as a child of the current GameObject
                instantiatedPrefab = Instantiate(prefab, attackManager.transform, false);

                // Now, 'equipPrefab' refers to the instantiated prefab
                equipPrefab = instantiatedPrefab;
            }
            else
            {
                Debug.LogError("Failed to load prefab: " + equipName);
            }

        }
    }



    public void OnPointerClick(PointerEventData eventData)
    {
      //  throw new System.NotImplementedException();
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