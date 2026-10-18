using UnityEngine;

/// <summary>
/// 판정 오브젝트가 생길 때 소환물을 함께 만든다 (슬라임 계보 Slime Mace: 내려찍기 → 튀는 슬라임).
/// 소환물은 독립 오브젝트(부모 없음)라 판정이 사라져도 남는다. 피해·수명은 소환물(SummonRunner 등)이 맡는다.
/// </summary>
public class SpawnOnStart : MonoBehaviour
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _count = 2;
    [SerializeField] private float _spacing = 0.6f;
    [SerializeField] private float _yOffset = 0f;

    private void Start()
    {
        if (_prefab == null)
        {
            return;
        }
        for (int i = 0; i < _count; i++)
        {
            float x = (i - (_count - 1) * 0.5f) * _spacing;
            Instantiate(_prefab, transform.position + new Vector3(x, _yOffset, 0f), Quaternion.identity);
        }
    }
}
