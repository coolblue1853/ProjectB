using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ShopUI : MonoBehaviour
{
    KeyAction action;
    InputAction upAction;
    public ShopManager shopManager;
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
        bool mine = InteractArbiter.Contend(gameObject, transform.position, isShopClose);
        if (mine && upAction.triggered && DatabaseManager.isOpenUI == false && !NpcMenu.IsOpen)
        {
            NpcInteraction.Run(gameObject, shopManager.OpenShop, null, null, null);
        }
    }
}
