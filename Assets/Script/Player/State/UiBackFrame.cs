/// <summary>한 프레임의 BACK 대상과 소비 상태. null 대상은 일시정지 메뉴 진입이다.</summary>
public sealed class UiBackFrame
{
    private int _frame = -1;
    private string _owner;
    private bool _consumed;

    public void Begin(int frame, string topOwner)
    {
        if (_frame == frame)
        {
            return;
        }
        _frame = frame;
        _owner = topOwner;
        _consumed = false;
    }

    public bool TryConsume(string owner)
    {
        if (_consumed || _owner != owner)
        {
            return false;
        }
        _consumed = true;
        return true;
    }

    public void Reset()
    {
        _frame = -1;
        _owner = null;
        _consumed = false;
    }
}
