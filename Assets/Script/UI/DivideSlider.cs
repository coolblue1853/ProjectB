using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DivideSlider : MonoBehaviour
{
    public int currentValue = 10; // 현재 가지고 있는 int 수
    public Slider divideSlider; // Unity Inspector에서 Slider를 할당
    public TextMeshProUGUI uiTxt;
    int output;
    public ItemCheck itemCheck;
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
        divideSlider.maxValue = currentValue;

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
        divideSlider.maxValue = currentValue;

        // 슬라이더의 현재 값 설정
        divideSlider.value = 1;
        output = 1;
        uiTxt.text = output.ToString() + "/" + currentValue.ToString();
    }
    public void Divide()
    {
        InventoryManager.instance.DownNowStack(output);
        InventoryManager.instance.state = "detail";
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(InventoryManager.instance.state == "divide")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Divide();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && divideSlider.value < divideSlider.maxValue)
            {
                divideSlider.value += 1;
                uiTxt.text = output.ToString() + "/" + currentValue.ToString();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && divideSlider.value>1)
            {
                divideSlider.value -= 1;
                uiTxt.text = output.ToString() + "/" + currentValue.ToString();
            }
        }
    }
}
