using UnityEngine;

/// <summary>
/// '아버지의 기록' 수집 로그 5편(MiniGames.md "고대 유물 발굴 구현", TowerWorld.md 세계관). 출처: GPT 초안(10-30).
/// 문지기의 정체는 확정하지 않는다(암시만). 문장은 영어 원문 = Loc 키, 한국어는 LocTerms.
/// 수집 여부는 ES3 excavation.records(int 비트마스크, 세이브 슬롯 파일, 새 키만).
/// </summary>
public static class FathersRecords
{
    public const string SaveKey = "excavation.records";

    public readonly struct Record
    {
        public readonly string Title;
        public readonly string Body;

        public Record(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }

    public static readonly Record[] All =
    {
        new Record("01. The First Ascent",
            "The tower casts its shadow against the sun, yet the stones beneath it are strangely warm. I left before she woke. I wonder if she will forgive me for that."),
        new Record("02. A Name in the Rain",
            "Her fever has followed me into my dreams. Last night, the rain whispered her name through the cracks of an empty temple. I did not answer, though every part of me wanted to."),
        new Record("03. The Four-Pointed Star",
            "The same four-pointed star is carved above every sealed doorway. The golden brooch I left with her bears that mark. I thought it a keepsake, but perhaps someone meant it to be a key."),
        new Record("04. The Price of a Wish",
            "They say the summit grants a single wish. No account speaks of those who received one, only of those who climbed. If the price is mine alone, I will pay it."),
        new Record("05. Someone at the Door",
            "There is a door in my dreams, and someone stands beyond it. Tonight, the figure spoke with a voice I almost recognized. It asked whether I would still open the door if I knew I could never leave."),
    };

    public static int LoadMask()
    {
        try
        {
            return ES3.KeyExists(SaveKey) ? ES3.Load<int>(SaveKey) : 0;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PB][Excavation] 기록 수집 읽기 실패, 빈 값 사용: " + e.Message);
            return 0;
        }
    }

    public static bool Has(int index)
    {
        return (LoadMask() & (1 << index)) != 0;
    }

    /// <summary>index 편을 수집했다고 저장</summary>
    public static void Unlock(int index)
    {
        ES3.Save(SaveKey, LoadMask() | (1 << index));
    }
}
