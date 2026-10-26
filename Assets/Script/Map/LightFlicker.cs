using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 화로·등불 같은 작은 광원의 흔들림. 펄린 노이즈로 밝기·반경을 조금씩 바꾼다(마을 대장간 화로, 상점 등불).
/// </summary>
[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float _amount = 0.25f;
    [SerializeField] private float _speed = 3f;

    private Light2D _light;
    private float _baseIntensity;
    private float _baseRadius;
    private float _seed;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
        _baseIntensity = _light.intensity;
        _baseRadius = _light.pointLightOuterRadius;
        _seed = Random.value * 100f;
    }

    private void Update()
    {
        float n = Mathf.PerlinNoise(_seed, Time.time * _speed) - 0.5f;
        _light.intensity = _baseIntensity * (1f + n * _amount * 2f);
        _light.pointLightOuterRadius = _baseRadius * (1f + n * _amount * 0.5f);
    }
}
