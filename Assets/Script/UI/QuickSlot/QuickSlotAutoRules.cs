/// <summary>퀵슬롯 자동 등록 대상 획득 경로(REWORK-0 U15). 세이브 로드·창고 꺼내기·장비 해제는 사용자가 이미 배치를 정한 경우라 제외</summary>
public static class QuickSlotAutoRules
{
    public static bool Accepts(AddSource source)
    {
        return source == AddSource.Pickup || source == AddSource.Shop || source == AddSource.Craft || source == AddSource.Cooking
            || source == AddSource.Gathering || source == AddSource.Fishing;
    }
}
