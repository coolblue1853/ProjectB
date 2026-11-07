using System;

/// <summary>
/// 타입별 숙련 서비스 (WeaponSkillProgression.md V.1·V.6). 타입 누적 EXP 보관·저장·구세이브 이행.
/// ES3 신규 키만 쓴다: typeMastery(TypeMasterySave), typeMasteryMigrated(bool). 기존 weaponMastery 는 읽기만 한다.
/// 이행은 아이템 DB 가 로드돼야 타입을 알 수 있어, DB 가 아직이면 보류했다가 첫 사용 때 한다.
/// </summary>
public static class TypeMasteryService
{
    public const string KeySave = "typeMastery";
    public const string KeyMigrated = "typeMasteryMigrated";

    public static TypeMasteryData Data { get; private set; }

    private static bool _dirty;
    private static bool _pending;

    /// <summary>이행이 끝났거나 필요 없는가(스킬트리 검증은 이후에만)</summary>
    public static bool Ready
    {
        get { return Data != null && !_pending; }
    }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Data = null;
        _dirty = false;
        _pending = false;
    }

    /// <summary>세이브 로드(SaveManager.DataLoad, WeaponMasteryService.Load 뒤). hadSaveFile=false 면 새 게임이라 이행 없음</summary>
    public static void Load(bool hadSaveFile)
    {
        Data = new TypeMasteryData();
        _pending = false;
        try
        {
            bool hasKey = ES3.KeyExists(KeySave);
            if (hasKey)
            {
                Data.LoadFrom(ES3.Load<TypeMasterySave>(KeySave));
            }
            bool migrated = ES3.KeyExists(KeyMigrated) && ES3.Load<bool>(KeyMigrated);
            if (migrated)
            {
                return;
            }
            if (!hadSaveFile || hasKey)
            {
                MarkMigrated();
            }
            else
            {
                _pending = true;
                EnsureMigrated();
            }
        }
        catch (Exception e)
        {
            // 실패 상태로 저장·스킬트리 검증이 돌면 이행이 영구 소실되거나 트리가 비워진다 → Data 를 비워 둔다(= Ready false, 해금 열림)
            _pending = false;
            Data = null;
            GameLog.Warn(GameLog.Cat.Save, "타입 숙련 로드 실패, 이번 세션은 비활성: " + e.Message);
        }
    }

    private static void MarkMigrated()
    {
        _dirty = true;
        Save(); // 데이터 먼저, 플래그는 나중(데이터 저장 실패 시 이행이 영구 생략되지 않게)
        if (!_dirty)
        {
            ES3.Save(KeyMigrated, true);
        }
    }

    /// <summary>구 weaponMastery → 타입별 최대 EXP 시드. DB 미로드면 보류(다음 호출에 재시도)</summary>
    public static void EnsureMigrated()
    {
        if (!_pending || Data == null || !WeaponMasteryService.DbReady)
        {
            return;
        }
        _pending = false;
        try
        {
            WeaponMasterySave old = WeaponMasteryService.Data != null ? WeaponMasteryService.Data.ToSave() : null;
            int unresolved = old != null ? Data.SeedFromWeapons(old.keys, old.exps, WeaponMasteryService.KindOf) : 0;
            MarkMigrated();
            GameLog.Info(GameLog.Cat.Save, "타입 숙련 이행 " + Data.Count + "종 (타입 미확인 무기 " + unresolved + ")");
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "타입 숙련 이행 실패: " + e.Message);
        }
    }

    /// <summary>SaveManager.DataSave 가 부른다. 바뀐 게 있을 때만 쓴다</summary>
    public static void Save()
    {
        if (Data == null || !_dirty || _pending)
        {
            return; // 이행 보류 중에는 쓰지 않는다(부분 키가 이행을 막는다)
        }
        try
        {
            ES3.Save(KeySave, Data.ToSave());
            _dirty = false;
        }
        catch (Exception e)
        {
            GameLog.Warn(GameLog.Cat.Save, "타입 숙련 저장 실패: " + e.Message);
        }
    }

    /// <summary>타입 숙련 레벨. 데이터 없음·타입 없음은 최대(= 해금 열림, 기존 방침)</summary>
    public static int LevelOf(WeaponKind kind)
    {
        EnsureMigrated();
        if (Data == null || _pending || kind == WeaponKind.None)
        {
            return TypeMasteryRules.MaxLevel; // 이행 보류 중에는 열림(구세이브 스킬 잠김 방지)
        }
        return Data.GetLevel(kind.ToString());
    }

    public static int ExpOf(WeaponKind kind)
    {
        EnsureMigrated();
        return Data != null && !_pending && kind != WeaponKind.None ? Data.GetExp(kind.ToString()) : 0;
    }

    /// <summary>타입에 EXP 를 쌓는다. 반환 = 레벨이 올랐으면 새 레벨, 아니면 0</summary>
    public static int AddExp(WeaponKind kind, int amount)
    {
        EnsureMigrated();
        if (Data == null || _pending || kind == WeaponKind.None || amount <= 0)
        {
            return 0;
        }
        bool up = Data.Add(kind.ToString(), amount);
        _dirty = true;
        return up ? Data.GetLevel(kind.ToString()) : 0;
    }
}
