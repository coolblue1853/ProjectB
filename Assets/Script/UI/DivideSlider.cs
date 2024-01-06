using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
public class DivideSlider : MonoBehaviour
{
    public int currentValue = 10; // 현재 가지고 있는 int 수
    public Slider divideSlider; // Unity Inspector에서 Slider를 할당
    public TextMeshProUGUI uiTxt;
    int output;
    public ItemCheck itemCheck;
    bool checkRepeat = false;
    Sequence sequence;

    KeyAction action;
    InputAction leftAction;
    InputAction rightAction;
    InputAction enterAction;
    InputAction horizontalCheck;
    float horizontalInput;
    private void OnEnable()
    {
        leftAction.Enable();
        rightAction.Enable();
        enterAction.Enable();
        horizontalCheck.Enable();
    }


    private void OnDisable()
    {
        leftAction.Disable();
        rightAction.Disable();
        enterAction.Disable();
        horizontalCheck.Disable();
    }
    private void Awake()
    {
        action = new KeyAction();
        leftAction = action.UI.LeftInventory;
        rightAction = action.UI.RightInventory;
        enterAction = action.UI.Enter;
        horizontalCheck = action.UI.horizontalCheck;
    }


        void Start()
    {
        // 슬라이더 초기화
        InitializeSlider();

        // 슬라이더 값이 변경될 때마다 호출되는 이벤트 등록
        divideSlider.onValueChanged.AddListener(OnSliderValueChanged);
        uiTxt.text = "1" + "/" +currentValue.ToString();
    }

    void InitializeSlider()
    {
        // 슬라이더의 최소값과 최대값 설정
        divideSlider.minValue = 1;
        divideSlider.maxValue = currentValue-1;

        // 슬라이더의 현재 값 설정
        divideSlider.value = 1;
        output = 1;
        uiTxt.text = output.ToString() + "/" + currentValue.ToString();
    }

    void OnSliderValueChanged(float value)
    {
         output = Mathf.RoundToInt(value);
         uiTxt.text = output.ToString() + "/" + currentValue.ToString();
    }
   
    public void ResetData()
    {
        divideSlider.minValue = 1;
        divideSlider.maxValue = currentValue-1;

        // 슬라이더의 현재 값 설정
        divideSlider.value = 1;
        output = 1;
        uiTxt.text = output.ToString() + "/" + currentValue.ToString();
    }
    public void Divide()
    {
        InventoryManager.instance.DownNowStack(output);
        InventoryManager.instance.DetailOff();
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        horizontalInput = (horizontalCheck.ReadValue<float>());
        if (InventoryManager.instance.state == "divide")
        {
            if (enterAction.triggered)
            {
                Divide();
            }
            else if ((rightAction.triggered) || (checkRepeat == false && horizontalInput == 1) && divideSlider.value < divideSlider.maxValue)
            {
                RightMove();
            }
            else if ((leftAction.triggered) || (checkRepeat == false && horizontalInput == -1) && divideSlider.value>1 )
            {
                LeftMove();
            }
        }
    }
    float waitTime = 0.18f;
    void RightMove()
    {
        checkRepeat = true;
        sequence.Kill();

        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
        divideSlider.value += 1;
        uiTxt.text = output.ToString() + "/" + currentValue.ToString();
    }
    void LeftMove()
    {
        checkRepeat = true;
        sequence.Kill();

        sequence = DOTween.Sequence()
        .AppendInterval(waitTime)
        .OnComplete(() => ResetCheckRepeat());
        divideSlider.value -= 1;
        uiTxt.text = output.ToString() + "/" + currentValue.ToString();
    }
    void ResetCheckRepeat()
    {
        checkRepeat = false;
    }
}
