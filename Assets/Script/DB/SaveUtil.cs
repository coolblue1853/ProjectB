/// <summary>
/// ES3 로드 공통 헬퍼(DedupAudit #1). 키 없으면 기본값. 키 이름·저장 형식·슬롯 파일 처리는 호출부 그대로(CodeStyle.md 3절).
/// </summary>
public static class SaveUtil
{
    public static T LoadOr<T>(string key, T defaultValue)
    {
        return ES3.KeyExists(key) ? ES3.Load<T>(key) : defaultValue;
    }

    /// <summary>설정(저장 위치) 지정 버전. 테스트에서 Cache 위치를 쓰기 위함.</summary>
    public static T LoadOr<T>(string key, T defaultValue, ES3Settings settings)
    {
        return ES3.KeyExists(key, settings) ? ES3.Load<T>(key, settings) : defaultValue;
    }
}
