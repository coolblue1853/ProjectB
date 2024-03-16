using UnityEngine;

public class VisibleOnlyDuringNight : MonoBehaviour
{
    LightColorController m_LightController;
    Renderer m_Renderer;

    void Awake()
    {
		m_LightController = FindObjectOfType<LightColorController>();
		m_Renderer = GetComponent<Renderer>();
    }

    void Update()
    {

    }
}
