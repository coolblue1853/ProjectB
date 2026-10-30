using System.Collections.Generic;

/// <summary>
/// 상점 되사기 목록(디아식, 순수 모델, BasicFeatures.md 12절). 최근 판매 묶음을 최대 Capacity 개까지 기억하고(넘치면 가장 오래된 것부터 버림),
/// 가장 최근 것부터 되산다(LIFO). 되사기 가격 = 판매로 받은 금액 그대로(Q-438). 세션 동안만 유지한다(저장하지 않음).
/// </summary>
public sealed class BuybackModel
{
    public const int Capacity = 12;

    public struct Sold
    {
        public string Id;
        public int Count;
        public int Tier;
        public int Upgrade;
        public int Price;
    }

    private readonly List<Sold> _list = new List<Sold>();

    public int Count => _list.Count;

    /// <summary>내용이 바뀔 때마다 오른다(목록 UI 가 바뀐 때만 다시 그리게)</summary>
    public int Version { get; private set; }

    /// <summary>최근 것부터 i 번째(0 = 다음에 되살 것). 범위 밖이면 빈 값(Id null)</summary>
    public Sold PeekAt(int i)
    {
        return i >= 0 && i < _list.Count ? _list[_list.Count - 1 - i] : default(Sold);
    }

    public void Record(Sold s)
    {
        if (string.IsNullOrEmpty(s.Id) || s.Count <= 0)
        {
            return;
        }
        _list.Add(s);
        Version++;
        if (_list.Count > Capacity)
        {
            _list.RemoveAt(0);
        }
    }

    public bool TryPeek(out Sold s)
    {
        if (_list.Count == 0)
        {
            s = default(Sold);
            return false;
        }
        s = _list[_list.Count - 1];
        return true;
    }

    public void Pop()
    {
        if (_list.Count > 0)
        {
            _list.RemoveAt(_list.Count - 1);
            Version++;
        }
    }

    public void Clear()
    {
        _list.Clear();
        Version++;
    }
}
