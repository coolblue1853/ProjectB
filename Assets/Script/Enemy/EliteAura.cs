using TMPro;
using UnityEngine;

/// <summary>정예 표식: 머리 위 금색 ★ + 몸에서 떠오르는 금빛 입자. 몬스터에 한 번 붙이고 승격/해제 때 켜고 끈다</summary>
public class EliteAura : MonoBehaviour
{
    static readonly Color Gold = new Color(1f, 0.8f, 0.3f);

    public static EliteAura Attach(EnemyHealth enemy)
    {
        var go = new GameObject("EliteAura");
        go.transform.SetParent(enemy.transform, false);
        var aura = go.AddComponent<EliteAura>();
        aura.Build(enemy);
        return aura;
    }

    void Build(EnemyHealth enemy)
    {
        var sr = enemy.GetComponent<SpriteRenderer>();
        Bounds local = sr != null && sr.sprite != null ? sr.sprite.bounds : new Bounds(Vector3.zero, Vector3.one * 2f);
        // 부모 스케일(0.43 등)을 상쇄해 아래 값들을 월드 단위로 쓴다
        Vector3 ls = enemy.transform.lossyScale;
        transform.localScale = new Vector3(1f / Mathf.Abs(ls.x), 1f / Mathf.Abs(ls.y), 1f);
        var b = new Bounds(Vector3.Scale(local.center, Abs(ls)), Vector3.Scale(local.size, Abs(ls)));

        // ★ 표식 (월드 TMP — 부모 스케일·좌우 반전 영향을 받지 않도록 매 프레임 보정)
        var starGo = new GameObject("Star", typeof(TextMeshPro));
        starGo.transform.SetParent(transform, false);
        starGo.transform.localPosition = new Vector3(0f, b.max.y + 0.45f, 0f);
        var star = starGo.GetComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            star.font = theme.pixelFont;
        }
        star.text = "*";
        star.fontSize = 14f;
        star.alignment = TextAlignmentOptions.Center;
        star.color = Gold;
        star.outlineWidth = 0.25f;
        star.outlineColor = new Color32(60, 20, 0, 255);
        star.sortingOrder = 120;
        star.rectTransform.sizeDelta = new Vector2(2f, 1f);

        // 떠오르는 금빛 입자
        var psGo = new GameObject("Motes");
        psGo.transform.SetParent(transform, false);
        psGo.transform.localPosition = new Vector3(0f, b.center.y, 0f);
        var ps = psGo.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        main.startColor = Gold;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 30;
        var emission = ps.emission;
        emission.rateOverTime = 14f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(b.size.x * 0.8f, b.size.y * 0.8f, 0.1f);
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        vel.y = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
        var r = psGo.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = Resources.Load<Material>("FX/M_PixelFx");
        r.sortingOrder = 115;
        ps.Play();
    }

    static Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    void LateUpdate()
    {
        // 부모가 좌우 반전(scale.x 음수)해도 ★ 글자가 뒤집히지 않게
        Vector3 ls = transform.localScale;
        float sign = Mathf.Sign(transform.parent.lossyScale.x);
        if (Mathf.Sign(ls.x) != sign)
        {
            transform.localScale = new Vector3(Mathf.Abs(ls.x) * sign, ls.y, ls.z);
        }
    }
}
