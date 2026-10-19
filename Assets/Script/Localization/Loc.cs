using I2.Loc;

using UnityEngine;



/// <summary>

/// 한/영 현지화 (AssetIntegrationPlan 6절 10, I2 Localization, Q-407).

/// - 키는 영어 문장 자체다: Loc.T("RESUME"). I2 에 번역이 없으면 영어를 그대로 돌려줘 누락이 있어도 화면이 깨지지 않는다.

/// - 번역 표는 LocTerms(코드) → Tools/probes/sync_i2_terms.cs 로 Resources/I2Languages.asset 에 동기화한다.

/// - 한글 글리프는 ThaleahFat 의 TMP 폴백 Galmuri11 Bold 가 그린다(폰트 교체 불필요).

/// - 언어는 슬롯과 무관한 공용 설정 파일("settings.lang")에 저장, 기본 한국어(Q-407 가정).

/// </summary>

public static class Loc

{

    public const string English = "English";

    public const string Korean = "Korean";

    const string PrefKey = "settings.lang";



    /// <summary>언어가 바뀌면 호출 — 코드로 만든 UI 는 이 때 다시 그린다</summary>

    public static event System.Action Changed;



    public static string Current { get; private set; } = Korean;

    public static bool IsKorean => Current == Korean;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]

    static void Init()

    {

        Changed = null;

        string saved = ES3.KeyExists(PrefKey, SaveSlots.SharedFile) ? ES3.Load<string>(PrefKey, SaveSlots.SharedFile) : Korean;

        Apply(saved, false);

    }



    public static void Set(string language)

    {

        Apply(language, true);

        ES3.Save(PrefKey, Current, SaveSlots.SharedFile);

    }



    public static void Toggle()

    {

        Set(IsKorean ? English : Korean);

    }



    static void Apply(string language, bool notify)

    {

        Current = language == English ? English : Korean;

        if (LocalizationManager.HasLanguage(Current))

        {

            LocalizationManager.CurrentLanguage = Current;

        }

        if (notify)

        {

            Changed?.Invoke();

        }

    }



    /// <summary>영어 문장 → 현재 언어. 번역이 없으면 영어 그대로</summary>

    public static string T(string english)

    {

        if (string.IsNullOrEmpty(english) || Current == English)

        {

            return english;

        }

        return LocalizationManager.TryGetTranslation(Term(english), out string t) && !string.IsNullOrEmpty(t) ? t : english;

    }



    /// <summary>I2 용어 이름: I2 가 저장할 때 쓰는 정규화(특수문자 제거·공백 합치기)를 그대로 적용해 조회 키를 맞춘다</summary>

    public static string Term(string english)

    {

        string t = I2Utils.GetValidTermName(english.Replace('/', ' '));
        return System.Text.RegularExpressions.Regex.Replace(t, @"\s+", " ").Trim();

    }

}

