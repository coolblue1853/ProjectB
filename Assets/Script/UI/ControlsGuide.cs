using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 조작 안내 표 (순수, 11-02). 칸 문자열의 {액션이름} 자리는 현재 키 설정으로 채운다(Player 맵 재지정 액션 = "Jump", UI 맵 = "UI/OpenInventory").
/// 새 단축키를 만들면 여기에 한 줄 추가한다(조작 안내 창 F10·일시정지 CONTROLS 가 그대로 보여 준다).
/// </summary>
public static class ControlsGuide
{
    public struct Row
    {
        public string Label, Keyboard, Pad;

        public Row(string label, string keyboard, string pad)
        {
            Label = label;
            Keyboard = keyboard;
            Pad = pad;
        }
    }

    public sealed class Section
    {
        public string Title;
        public Row[] Rows;
        public bool RightColumn;
    }

    /// <summary>칸의 {이름}을 resolve(이름, 게임패드 여부)로 바꾼다. 빈 칸은 "-"</summary>
    public static string Fill(string template, bool gamepad, Func<string, bool, string> resolve)
    {
        if (string.IsNullOrEmpty(template))
        {
            return "-";
        }
        var sb = new StringBuilder(template.Length + 8);
        int i = 0;
        while (i < template.Length)
        {
            int open = template.IndexOf('{', i);
            int close = open < 0 ? -1 : template.IndexOf('}', open + 1);
            if (open < 0 || close < 0)
            {
                sb.Append(template, i, template.Length - i);
                break;
            }
            sb.Append(template, i, open - i);
            sb.Append(resolve(template.Substring(open + 1, close - open - 1), gamepad));
            i = close + 1;
        }
        return sb.ToString();
    }

    public static readonly Section[] Sections =
    {
        new Section
        {
            Title = "이동 · 전투",
            Rows = new[]
            {
                new Row("이동", "←→", "왼쪽 스틱"),
                new Row("달리기", "{Run}+←→", "{Run}+스틱"),
                new Row("점프", "{Jump}", "{Jump}"),
                new Row("발판 아래로", "↓+{Jump}", "↓+{Jump}"),
                new Row("로프", "닿아서 ↑↓", "스틱 ↑↓"),
                new Row("평타", "{Attack}", "{Attack}"),
                new Row("장비 스킬 1", "{SkillA}", "{SkillA}"),
                new Row("장비 스킬 2", "{SkillS}", "{SkillS}"),
                new Row("장비 스킬 3", "{SkillD}", "{SkillD}"),
                new Row("장비 스킬 4", "{SkillF}", "{SkillF}"),
                new Row("다운된 적 공격", "↓+{Attack}", "↓+{Attack}"),
                new Row("대시 (뒤 층계 개방)", "{Dash}", "{Dash}"),
                new Row("아티팩트 / 다음", "{Artifact} / {ArtifactNext}", "{Artifact} / {ArtifactNext}"),
            },
        },
        new Section
        {
            Title = "물약 · 상호작용",
            Rows = new[]
            {
                new Row("물약·귀환 주문서", "{Quick1} {Quick2} {Quick3} {Quick4}", "L3 / 십자 ←→ / L3 길게"),
                new Row("퀵슬롯 등록", "가방에서 Shift+1~4", "-"),
                new Row("NPC 말 걸기(선택지)·포털·채집·화로", "↑", "십자 ↑"),
                new Row("대화 넘기기", "Enter / Z", "A"),
                new Row("창 닫기 · 일시정지", "ESC", "B / START"),
            },
        },
        new Section
        {
            Title = "창 열기",
            RightColumn = true,
            Rows = new[]
            {
                new Row("가방", "{UI/OpenInventory}", "{UI/OpenInventory}"),
                new Row("제작", "{UI/OpenCraft}", "{UI/OpenCraft}"),
                new Row("퀘스트 (창 안: H 추적창 접기 · Del 포기)", "Q", "SELECT 길게 (LT 접기 · Y 포기)"),
                new Row("층계 지도", "M", "LT+START"),
                new Row("도감", "B", "LT+SELECT"),
                new Row("스킬창", "K", "LT+Y"),
                new Row("방어 (방패 장착 시)", "G", "LT"),
                new Row("미니맵 켜기·끄기", "N", "-"),
                new Row("제작 핀", "P", "-"),
                new Row("조작 안내", "F10", "-"),
                new Row("키 설정", "일시정지 > 설정", "일시정지 > 설정"),
            },
        },
        new Section
        {
            Title = "가방 · 상점 · 창고",
            RightColumn = true,
            Rows = new[]
            {
                new Row("가방 확장 / 쌓기", "N / T", "LT+X / LT+LB"),
                new Row("페이지 넘기기", "Q E · 1~5(BOX)", "LB RB"),
                new Row("버리기 (수량 창)", "Del", "상세창 RT"),
                new Row("아이템 잠금 / 해제", "L", "LT+R3"),
                new Row("정렬", "R", "R3"),
                new Row("분해 (두 번 눌러 확정)", "D", "-"),
                new Row("장비 비교", "Shift", "LT"),
                new Row("장비 프리셋 장착 / 저장", "7 8 9 / Shift+7 8 9", "SELECT"),
                new Row("상점: 되사기 / 고르기", "U / [ ]", "RT / LT+Y"),
                new Row("상점: 판매 필터", "O", "SELECT"),
                new Row("상점: 물약 충전", "H", "LT+X"),
                new Row("상점: 재료 일괄 판매", "J", "LT+L3"),
                new Row("창고: 같은 재료 / 재료 전부", "G / T", "RT / SELECT"),
            },
        },
    };
}
