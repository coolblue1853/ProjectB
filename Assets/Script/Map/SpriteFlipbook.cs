using UnityEngine;

/// <summary>NPC·장식용 단순 프레임 반복 (Animator 없이). 프레임은 순서대로 돌고 첫 프레임에 더 오래 머물 수 있다</summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbook : MonoBehaviour
{
    [SerializeField] private Sprite[] _frames;
    [SerializeField] private float _frameTime = 0.6f;

    SpriteRenderer sr;
    float timer;
    int index;

    public Sprite[] Frames
    {
        get => _frames;
        set => _frames = value;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        timer = Random.Range(0f, _frameTime); // 여러 NPC 가 동시에 깜빡이지 않게
    }

    void Update()
    {
        if (_frames == null || _frames.Length < 2)
        {
            return;
        }
        timer -= Time.deltaTime;
        if (timer > 0f)
        {
            return;
        }
        timer = _frameTime;
        index = (index + 1) % _frames.Length;
        sr.sprite = _frames[index];
    }
}
