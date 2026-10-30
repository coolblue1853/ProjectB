/// <summary>
/// 게임 UI(HUD·강화·도감·지도·인벤토리·상점·제작·알림)와 몬스터·지역 이름 번역 표. 조합 문장은 Loc.F 형식 문자열({0}).
/// sync_i2_terms.cs 가 LocTerms·LocItems 와 함께 I2 로 옮긴다.
/// </summary>
public static class LocUi
{
    public static readonly (string en, string ko)[] Entries =
    {
        // ── 알림·HUD ──
        ("UP  TALK", "위  대화"),
        ("Elite: ", "정예: "),
        ("POISON", "독"), ("BLEED", "출혈"), ("IMMUNE", "무효"), ("BLOCK", "막음"),
        ("Artifact: ", "아티팩트: "),
        ("Healing Dew", "치유의 이슬"), ("Guardian Shell", "수호 껍질"), ("Hourglass", "모래시계"), ("Star Burst", "별의 폭발"),
        ("Rank up: {0} Explorer", "승급: {0} 탐험가"),
        ("LOAD", "부하"), ("RES", "저항"), ("DMG", "피해"),
        ("COMBO", "콤보"),
        ("Inventory Full", "인벤토리 가득 참"), ("Not enough gold", "골드 부족"),
        ("Only consumables go in quick slots", "퀵슬롯에는 소비 아이템만 넣을 수 있다"),
        ("PICKUP", "줍기"), ("ALL", "전부"), ("SKIP COMMON MAT.", "일반 재료 제외"),
        ("Path sealed by the Tower.", "탑이 길을 봉인했다."),
        ("Tower Gate", "탑의 문"), ("The way to the 2nd floor is open", "2층계로 가는 길이 열렸다"),
        ("ENRAGED — Calls the gang!", "격노 — 패거리를 부른다!"),
        ("Discovered: ", "발견: "),
        ("SLOT {0}", "슬롯 {0}"), ("(restart)", "(재시작)"), ("Restarting...", "재시작 중..."),
        ("EMPTY", "비어 있음"), ("DAMAGED", "손상됨"),

        // ── 강화 ──
        ("ENHANCE", "강화"),
        ("SUCCESS  +{0}", "성공  +{0}"),
        ("FAILED  +{0} → +{1}", "실패  +{0} → +{1}"),
        ("FAILED", "실패"),
        ("MAX LEVEL", "최대 단계"), ("NOT ENOUGH CRYSTALS", "결정 부족"), ("NOT ENOUGH GOLD", "골드 부족"), ("CANNOT ENHANCE", "강화 불가"),
        ("MAX ENHANCED", "최대 강화 완료"),
        ("Damage", "공격력"), ("Stats", "능력치"),
        ("Success", "성공률"), ("100% (CHANCE!)", "100% (찬스!)"),
        ("On fail", "실패 시"), ("-1 level", "-1 단계"), ("keep level", "단계 유지"),
        ("Crystals", "층계 결정"), ("Gold", "골드"),
        ("UP/DOWN select    SELECT enhance    BACK close", "위/아래 선택    선택 강화    뒤로 닫기"),
        ("No equipment in inventory", "인벤토리에 장비가 없다"),

        // ── 도감 ──
        ("BOSS", "보스"), ("ELITE", "정예"),
        ("Region", "지역"), ("Defeated", "처치"), ("HP", "체력"), ("DEF", "방어"),
        ("Stats: defeat {0} more", "능력치: {0}마리 더 처치"),
        ("Drops", "드롭"), ("Drops: defeat {0} more", "드롭: {0}마리 더 처치"),
        ("ARROWS select    B / BACK close", "방향키 선택    B / 뒤로 닫기"),
        ("Forest Road", "숲길"), ("Forest Side Road", "숲 샛길"), ("Magic Cliffs", "마법 절벽"), ("Cliff Altar", "절벽 제단"),
        ("night", "밤"), ("boss", "보스"),

        // ── 지도 ──
        ("VILLAGE", "마을"), ("(HERE)", "(현재 위치)"),
        ("Ascent load", "상승 부하"), ("Your resistance", "내 저항"),
        ("Deficit {0}  (-{1}% damage)", "부족 {0}  (-{1}% 피해)"),
        ("No ascent penalty", "상승 페널티 없음"),
        ("SELECT  travel here", "선택  이곳으로 이동"),
        ("Undiscovered.", "미발견."), ("Reach it on foot to unlock the waypoint.", "직접 걸어서 도착하면 웨이포인트가 열린다."),
        ("UP/DOWN select    SELECT travel    M / BACK close", "위/아래 선택    선택 이동    M / 뒤로 닫기"),

        // ── 인벤토리·상점·제작 ──
        ("Sell {0} {1}EA,  Price : {2}", "{0} {1}개 판매,  가격 : {2}"),
        ("Buy {0} {1}EA,  Price : {2}", "{0} {1}개 구매,  가격 : {2}"),
        ("Price : ", "가격 : "),
        ("Misc", "재료"), ("Consum", "소모품"), ("Equip", "장비"),
        ("Weapon", "무기"), ("Head", "머리"), ("Chest", "몸통"), ("Leg", "다리"), ("Hand", "손"),
        ("Common", "일반"), ("Rare", "희귀"), ("Unique", "고유"),
        ("Crafting material", "제작 재료"), ("Consumable", "소모품"), ("Equipment", "장비"), ("set", "세트"),
        ("A crystal born of the tower. Fed to a core at the smith to raise its enhancement.", "탑에서 태어난 결정. 대장간에서 장비에 먹여 강화 단계를 올린다."),
        ("on the light", "어둠을 밝힌다."),
        ("Buy", "상점 구매"), ("Craft", "제작"), ("cook", "요리"),
        ("Dmg", "피해"), ("Critical Chance", "치명타 확률"), ("Critical Dmg", "치명타 피해"), ("Dmg Increase", "피해 증가"), ("Dmg Decrease", "피해 감소"),
        ("Armor Penetration", "방어 관통"), ("Skill Dmg", "스킬 피해"), ("Additional Dmg", "추가 피해"), ("Basic Dmg", "기본 피해"),
        ("Def", "방어"), ("Hp", "체력"), ("Critical Rate", "치명타율"), ("Drop Rate", "드롭률"), ("Movement Speed", "이동 속도"),
        ("Attack Speed", "공격 속도"), ("Cooldown", "재사용 대기"), ("CoolDown", "재사용 대기"), ("Incoming Dmg", "받는 피해"),
        ("count", "회"),
        ("Chance to Cause Bleeding", "출혈 확률"), ("Dmg to Bleeding Enemies", "출혈 적 피해"), ("Poison Dmg", "독 피해"), ("Sec", "초"),

        // ── 몬스터 (MonsterSheet displayName) ──
        ("Abyss Wizard", "심연의 마법사"), ("Cliff Goblin", "절벽 고블린"), ("Cliff Skeleton", "절벽 해골"), ("Death Bringer", "죽음을 부르는 자"),
        ("Flower Mole", "꽃 두더지"), ("Gaze Eye", "응시하는 눈"), ("Leaf Mole", "잎 두더지"), ("Passive Terror", "잠잠한 공포"),
        ("Forest Rabbit", "숲 토끼"), ("Rat Chief", "쥐 두목"), ("Rat Thief", "쥐 도적"), ("Scarecrow", "허수아비"),
        ("Spore Shroom", "포자 버섯"), ("Stone Rabbit", "돌 토끼"), ("Forest Wolf", "숲 늑대"), ("Wild Boar", "멧돼지"),
        ("Wisp", "도깨비불"), ("Forest Snail", "숲 달팽이"), ("Forest Bear", "숲 곰"), ("Ent", "엔트"), ("Ghost", "유령"),
        ("Fairy", "요정"), ("Ancient Ent", "고대 엔트"), ("Swamp Frog", "늪 개구리"), ("Marsh Crab", "늪지 게"), ("Bog Croc", "늪 악어"),
        ("Swamp Mosquito", "늪 모기"), ("Bog Slime", "늪 슬라임"), ("Giant Slime", "거대 슬라임"), ("Bandit Cutthroat", "산적 단검병"),
        ("Bandit Archer", "산적 궁수"), ("Bandit Shieldman", "산적 방패병"), ("Bandit Torchbearer", "산적 횃불 순찰병"), ("Bandit Chief", "산적 두목"),
        ("Small Golem", "작은 골렘"), ("Medium Golem", "중형 골렘"), ("Large Golem", "거대 골렘"), ("Ruin Spirit", "폐허의 정령"),
        ("Rampart Knight", "성벽 기사"), ("Halberd Sentry", "미늘창 보초"), ("Gargoyle", "가고일"), ("Cloud Wisp", "구름 도깨비불"),
        ("Gargoyle Lord", "가고일 군주"),
    };
}
