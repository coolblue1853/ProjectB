/// <summary>
/// ES3 로드 공통 헬퍼(DedupAudit #1). 키 없으면 기본값. 키 이름·저장 형식·슬롯 파일 처리는 호출부 그대로(CodeStyle.md 3절).
/// </summary>
public static class SaveUtil
{
    /// <summary>키 없음 또는 형식 불일치(손상·타입 변경) → 기본값 + 경고 로그. 저장 차단이 필요한 호출부는 LoadOrThrow.</summary>
    public static T LoadOr<T>(string key, T defaultValue)
    {
        try
        {
            return LoadOrThrow(key, defaultValue);
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "세이브 읽기 실패, 기본값 사용: " + key + " (" + e.Message + ")");
            return defaultValue;
        }
    }

    /// <summary>설정(저장 위치) 지정 버전. 테스트에서 Cache 위치를 쓰기 위함. 형식 불일치는 기본값.</summary>
    public static T LoadOr<T>(string key, T defaultValue, ES3Settings settings)
    {
        try
        {
            return LoadOrThrow(key, defaultValue, settings);
        }
        catch (System.Exception e)
        {
            GameLog.Warn(GameLog.Cat.Item, "세이브 읽기 실패, 기본값 사용: " + key + " (" + e.Message + ")");
            return defaultValue;
        }
    }

    /// <summary>형식 불일치 시 예외를 그대로 던진다(ExtraEquip 처럼 읽기 실패 시 저장 차단이 필요한 호출부용).</summary>
    public static T LoadOrThrow<T>(string key, T defaultValue)
    {
        return ES3.KeyExists(key) ? ES3.Load<T>(key) : defaultValue;
    }

    public static T LoadOrThrow<T>(string key, T defaultValue, ES3Settings settings)
    {
        return ES3.KeyExists(key, settings) ? ES3.Load<T>(key, settings) : defaultValue;
    }
}
