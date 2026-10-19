/// <summary>
/// 아이템 표시 이름(ItemSheet.csv itemNameT)·세트 이름 → 한국어. ItemDatabase 가 불러올 때 Loc.T 로 바꾼다(언어 변경은 다음 실행부터).
/// sync_i2_terms.cs 가 LocTerms 와 함께 I2 로 옮긴다.
/// </summary>
public static class LocItems
{
    public static readonly (string en, string ko)[] Entries =
    {
        // ── 세트 ──
        ("Basic Gear", "기본 장비"), ("Nature's Reward", "자연의 보답"), ("Cliffwalker's Vow", "절벽 순례자의 맹세"),
        ("Pack Hunter", "무리 사냥꾼"), ("Night Lantern", "밤의 등불"), ("Ancient Grove", "고대 숲"), ("Will-o'-Fae", "요정 도깨비불"),
        ("Bog Stalker", "늪 추적자"), ("Gloom Swarm", "음울한 떼"), ("Outlaw", "무법자"), ("Iron Raider", "강철 약탈자"),
        ("Stoneborn", "돌에서 깨어난 자"), ("Rampart Guard", "성벽 수비대"), ("Skybound", "하늘을 향한 자"),

        // ── 기본·재료·소모품 ──
        ("Rock", "돌멩이"), ("Wood", "나무"), ("PickAxe", "곡괭이"), ("Potion", "물약"), ("Food", "음식"),
        ("Cap", "모자"), ("Armor", "갑옷"), ("HighArmor", "상급 갑옷"), ("LegArmor", "다리 갑옷"), ("HandArmor", "장갑"),
        ("Necklace", "목걸이"), ("Ring", "반지"), ("ToxicPickAxe", "독 곡괭이"), ("Shoes", "신발"),
        ("Leather Knuckles", "가죽 너클"), ("Wood Spear", "나무 창"),
        ("LeafCap", "잎사귀 모자"), ("LeafArmor", "잎사귀 갑옷"), ("LeafLegArmor", "잎사귀 다리 갑옷"), ("LeafHandArmor", "잎사귀 장갑"),
        ("LeafShoes", "잎사귀 신발"), ("LeafStaff", "잎사귀 지팡이"), ("LeafAdae", "잎사귀 단검"),
        ("RatCap", "쥐 두건"), ("RatArmor", "쥐 갑옷"), ("RatLegArmor", "쥐 다리 갑옷"), ("RatHandArmor", "쥐 장갑"), ("RatShoes", "쥐 신발"),
        ("DoubleDagger", "쌍단검"), ("PoisonSword", "독검"), ("RatCheifDagger", "쥐 두목의 단검"),
        ("ScareCap", "허수아비 모자"), ("ScareArmor", "허수아비 갑옷"), ("ScareLegArmor", "허수아비 다리 갑옷"), ("ScareHandArmor", "허수아비 장갑"),
        ("ScareShoes", "허수아비 신발"), ("SacreGreatSword", "허수아비 대검"), ("ScareStaff", "허수아비 지팡이"), ("ScareSide", "허수아비 낫"),
        ("ScareClow", "허수아비 발톱"), ("RustedSword", "녹슨 검"), ("Rusted Sword and Shield", "녹슨 검과 방패"),
        ("Four-Leaf Clover Ring", "네잎클로버 반지"), ("Rabbit's Foot", "토끼 발"), ("Rabbit Meat Stew", "토끼고기 스튜"), ("Rabbit Meat", "토끼고기"),
        ("Small Bone", "작은 뼈"), ("Rabbit Hide", "토끼 가죽"), ("Fine Rabbit Hide", "고운 토끼 가죽"), ("Round Bone", "둥근 뼈"),
        ("Green Hurb", "초록 약초"), ("Pristine Low-Grade Ore", "깨끗한 하급 광석"), ("Green Wreath", "초록 화관"), ("Small Tube", "작은 관"),
        ("Fresh Green Leaves", "싱싱한 초록 잎"), ("Sturdy Roots", "튼튼한 뿌리"), ("Herbal Salad", "약초 샐러드"),
        ("Gentle Spring Guidance", "봄바람의 인도"), ("Sharp Petal Ring", "날카로운 꽃잎 반지"), ("Sturdy Leaf", "튼튼한 잎"), ("Sharp Petal", "날카로운 꽃잎"),
        ("Nasty Rat Ring", "고약한 쥐 반지"), ("Disgusting Rat Meat", "역겨운 쥐고기"), ("Rat Thief's Stash", "쥐 도적의 장물"),
        ("Filthy Rat Hide", "더러운 쥐 가죽"), ("Stinky Rat Meat Stew", "냄새나는 쥐고기 스튜"),
        ("Necklace of Spreading Corruption", "번지는 부패의 목걸이"), ("Necklace of the Sly Plunderer", "교활한 약탈자의 목걸이"),
        ("Ring Finger that Tears Wounds", "상처를 찢는 약지"), ("Rat Cheif 's Loot", "쥐 두목의 전리품"), ("Tough Rat Hide", "질긴 쥐 가죽"),
        ("Shabby Straw Ring", "허름한 짚 반지"), ("Ominous Black Liquid", "불길한 검은 액체"), ("Blackened Branch", "검게 탄 가지"),
        ("Eerie Cloth Scrap", "으스스한 천 조각"), ("Malice-Infused Branch", "악의 서린 가지"), ("Gloomy Cloth Scrap", "음울한 천 조각"),
        ("Necklace of Silent Screams", "소리 없는 비명의 목걸이"), ("Torch", "횃불"), ("Emergency Rations", "비상 식량"),
        ("Cliff Crystal Ore", "절벽 수정 광석"), ("Cliff Fox Pelt", "절벽 여우 가죽"), ("Fox Meat", "여우고기"), ("Glowing Spore", "빛나는 포자"),
        ("Spongy Mushroom Cap", "말랑한 버섯갓"), ("Goblin Fang", "고블린 송곳니"), ("Tarnished Goblin Coin", "녹슨 고블린 동전"),
        ("Membrane Wing", "막 날개"), ("Bloodshot Lens", "충혈된 수정체"), ("Ancient Bone", "고대의 뼈"), ("Cracked Shield Shard", "금 간 방패 조각"),
        ("Death Sigil", "죽음의 인장"), ("Abyssal Core", "심연의 핵"), ("Roasted Fox Skewer", "여우 꼬치구이"),

        // ── 2층계 절벽 ──
        ("Cliffwalker Hood", "절벽 순례자 두건"), ("Cliffwalker Mail", "절벽 순례자 갑옷"), ("Cliffwalker Greaves", "절벽 순례자 각반"),
        ("Cliffwalker Gloves", "절벽 순례자 장갑"), ("Cliffwalker Boots", "절벽 순례자 장화"), ("Cliffwind Staff", "절벽바람 지팡이"),
        ("Cliffwind Blade", "절벽바람 검"), ("Abyssal Wizard Staff", "심연 마법사의 지팡이"), ("Eye of the Cliffs", "절벽의 눈"),
        ("Bringer's Sigil Ring", "인도자의 인장 반지"), ("Floor Crystal", "층계 결정"),

        // ── 1층계 숲·깊은 숲 ──
        ("Fang Dagger", "송곳니 단검"), ("Wolf Fang", "늑대 송곳니"), ("Boar Tusk", "멧돼지 엄니"), ("Wisp Ember", "도깨비불 불씨"),
        ("Snail Shell", "달팽이 껍데기"), ("Tusk Knuckles", "엄니 너클"), ("Wisp Lantern", "도깨비불 등불"), ("Shell Shield", "껍데기 방패"),
        ("Pack Hunter Hood", "무리 사냥꾼 두건"), ("Pack Hunter Coat", "무리 사냥꾼 외투"), ("Pack Hunter Greaves", "무리 사냥꾼 각반"),
        ("Pack Hunter Gloves", "무리 사냥꾼 장갑"), ("Pack Hunter Boots", "무리 사냥꾼 장화"),
        ("Night Lantern Hood", "밤의 등불 두건"), ("Night Lantern Coat", "밤의 등불 외투"), ("Night Lantern Greaves", "밤의 등불 각반"),
        ("Night Lantern Gloves", "밤의 등불 장갑"), ("Night Lantern Boots", "밤의 등불 장화"),
        ("Bear Claw", "곰 발톱"), ("Root Staff", "뿌리 지팡이"), ("Heartwood Greatsword", "심재 대검"), ("Bear Pelt", "곰 가죽"),
        ("Heartwood Chip", "심재 조각"), ("Ectoplasm", "엑토플라즘"), ("Fairy Dust", "요정 가루"), ("Phantom Scythe", "망령의 낫"), ("Fae Wand", "요정 지팡이"),
        ("Ancient Grove Hood", "고대 숲 두건"), ("Ancient Grove Coat", "고대 숲 외투"), ("Ancient Grove Greaves", "고대 숲 각반"),
        ("Ancient Grove Gloves", "고대 숲 장갑"), ("Ancient Grove Boots", "고대 숲 장화"),
        ("Will-o'-Fae Hood", "요정 도깨비불 두건"), ("Will-o'-Fae Coat", "요정 도깨비불 외투"), ("Will-o'-Fae Greaves", "요정 도깨비불 각반"),
        ("Will-o'-Fae Gloves", "요정 도깨비불 장갑"), ("Will-o'-Fae Boots", "요정 도깨비불 장화"),

        // ── 늪 ──
        ("Frog Tongue Whip", "개구리 혀 채찍"), ("Croc Jaw Axe", "악어 턱 도끼"), ("Petal Pincer", "꽃잎 집게"), ("Bloodsting Rapier", "피침 레이피어"),
        ("Slime Mace", "슬라임 철퇴"), ("Primordial Gel", "태초의 젤"), ("Frog Skin", "개구리 가죽"), ("Crab Pincer", "게 집게발"),
        ("Croc Tooth", "악어 이빨"), ("Blood Sting", "피의 침"), ("Slime Gel", "슬라임 젤"),
        ("Bog Stalker Hood", "늪 추적자 두건"), ("Bog Stalker Coat", "늪 추적자 외투"), ("Bog Stalker Greaves", "늪 추적자 각반"),
        ("Bog Stalker Gloves", "늪 추적자 장갑"), ("Bog Stalker Boots", "늪 추적자 장화"),
        ("Gloom Swarm Hood", "음울한 떼 두건"), ("Gloom Swarm Coat", "음울한 떼 외투"), ("Gloom Swarm Greaves", "음울한 떼 각반"),
        ("Gloom Swarm Gloves", "음울한 떼 장갑"), ("Gloom Swarm Boots", "음울한 떼 장화"),

        // ── 산적 성채 ──
        ("Cutthroat Dagger", "멱따기 단검"), ("Bandit Shortbow", "산적 단궁"), ("Raider Buckler", "약탈자 버클러"), ("Chief's War Banner", "두목의 군기"),
        ("Bandit Bandana", "산적 두건 천"), ("Broken Arrow", "부러진 화살"), ("Iron Rivet", "쇠 리벳"), ("Pitch Torch", "송진 횃불"),
        ("Outlaw Hood", "무법자 두건"), ("Outlaw Vest", "무법자 조끼"), ("Outlaw Breeches", "무법자 바지"), ("Outlaw Gloves", "무법자 장갑"),
        ("Outlaw Boots", "무법자 장화"), ("Iron Raider Helm", "강철 약탈자 투구"), ("Iron Raider Mail", "강철 약탈자 갑옷"),
        ("Iron Raider Greaves", "강철 약탈자 각반"), ("Iron Raider Gauntlets", "강철 약탈자 건틀릿"), ("Iron Raider Boots", "강철 약탈자 장화"),

        // ── 탑 근교 ──
        ("Pebble Gauntlet", "조약돌 건틀릿"), ("Quakestone Maul", "진동석 망치"), ("Monolith Shield", "석주 방패"), ("Gatekeeper's Oath", "문지기의 서약"),
        ("Golem Pebble", "골렘 조약돌"), ("Quake Core", "진동 핵"), ("Monolith Shard", "석주 파편"), ("Ruin Mote", "폐허의 티끌"),
        ("Stoneborn Helm", "돌에서 깨어난 자의 투구"), ("Stoneborn Plate", "돌에서 깨어난 자의 판금"), ("Stoneborn Greaves", "돌에서 깨어난 자의 각반"),
        ("Stoneborn Gauntlets", "돌에서 깨어난 자의 건틀릿"), ("Stoneborn Boots", "돌에서 깨어난 자의 장화"),

        // ── 2층계 무너진 성벽 ──
        ("Rampart Sword", "성벽 검"), ("Sentry Halberd", "보초 미늘창"), ("Gargoyle Wing", "가고일 날개"), ("Skyfall Talon", "하늘낙하 발톱"),
        ("Rampart Shard", "성벽 파편"), ("Halberd Fragment", "미늘창 조각"), ("Gargoyle Feather", "가고일 깃털"), ("Storm Wisp", "폭풍 도깨비불"),
        ("Rampart Guard Helm", "성벽 수비대 투구"), ("Rampart Guard Plate", "성벽 수비대 판금"), ("Rampart Guard Greaves", "성벽 수비대 각반"),
        ("Rampart Guard Gauntlets", "성벽 수비대 건틀릿"), ("Rampart Guard Sabatons", "성벽 수비대 철장화"),
        ("Skybound Cowl", "하늘을 향한 자의 두건"), ("Skybound Vest", "하늘을 향한 자의 조끼"), ("Skybound Leggings", "하늘을 향한 자의 바지"),
        ("Skybound Gloves", "하늘을 향한 자의 장갑"), ("Skybound Boots", "하늘을 향한 자의 장화"),
    };
}
