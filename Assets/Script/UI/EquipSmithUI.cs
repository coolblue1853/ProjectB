using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class EquipSmithUI : MonoBehaviour
{
    KeyAction action;
    InputAction upAction, downAction;
    public CraftingManager craftWeapon;
    public bool isShopClose;
    private void OnEnable()
    {
        upAction.Enable();
        downAction.Enable();
    }


    private void OnDisable()
    {
        upAction.Disable();
        downAction.Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        upAction = action.UI.Up;
        downAction = action.UI.DownInventory; // ↓ = 강화 창 (ProgressionDesign 3절)
    }
    // Start is called before the first frame update

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            isShopClose = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            isShopClose = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isShopClose == true && upAction.triggered && DatabaseManager.isOpenUI == false)
        {
            craftWeapon.EquipOpen();
        }
        else if (isShopClose == true && downAction.triggered && DatabaseManager.isOpenUI == false)
        {
            EnhanceWindow.Open();
        }
    }
}
