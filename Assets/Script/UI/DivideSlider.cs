using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DivideSlider : MonoBehaviour
{
    public int currentValue = 10; // ���� ������ �ִ� int ��
    public Slider divideSlider; // Unity Inspector���� Slider�� �Ҵ�
    public TextMeshProUGUI uiTxt;
    int output;
    public ItemCheck itemCheck;
    void Start()
    {
        // �����̴� �ʱ�ȭ
        InitializeSlider();

        // �����̴� ���� ����� ������ ȣ��Ǵ� �̺�Ʈ ���
        divideSlider.onValueChanged.AddListener(OnSliderValueChanged);
        uiTxt.text = "1" + "/" +currentValue.ToString();
    }

    void InitializeSlider()
    {
        // �����̴��� �ּҰ��� �ִ밪 ����
        divideSlider.minValue = 1;
        divideSlider.maxValue = currentValue;

        // �����̴��� ���� �� ����
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

        // �����̴��� ���� �� ����
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
