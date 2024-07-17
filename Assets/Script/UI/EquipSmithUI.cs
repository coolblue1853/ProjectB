using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class EquipSmithUI : MonoBehaviour
{
    KeyAction action;
    InputAction upAction;
    public CraftingManager craftWeapon;
    public bool isShopClose;
    private void OnEnable()
    {
        upAction.Enable();
    }


    private void OnDisable()
    {
        upAction.Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        upAction = action.UI.Up;
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
    }
}
