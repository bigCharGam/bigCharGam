using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class QTEIndicator : MonoBehaviour
{
    [SerializeField] private GameObject line;
    [SerializeField] private GameObject arrow1;
    [SerializeField] private GameObject arrow2;

    [SerializeField] private SkillTimeDamageGraph skillTimeDamageGraph;
    
    private float setUpTime1;
    private float setUpTime2;
    private float moveTime1;
    private float moveTime2;

    private bool skipToStage4;

    private readonly System.Collections.Generic.Dictionary<TrailRenderer, GradientAlphaKey[]> baseTrailAlphaKeys = new System.Collections.Generic.Dictionary<TrailRenderer, GradientAlphaKey[]>();

    public void SkipToStage4()
    {
        skipToStage4 = true;
    }

    // Skill2가 여러 QTE의 스폰/퍼펙트 시점을 미리 계산할 때 재사용하는 고정 공식
    public static float ComputeMoveTime1(SkillTimeDamageGraph graph)
    {
        return (graph.damageGraph[2].time + graph.damageGraph[3].time) / 2f - graph.damageGraph[1].time;
    }

    public static float ComputeLeadTime(SkillTimeDamageGraph graph)
    {
        return graph.damageGraph[1].time + ComputeMoveTime1(graph);
    }

    public static float ComputePerfectWindow(SkillTimeDamageGraph graph)
    {
        return graph.damageGraph[3].time - graph.damageGraph[2].time;
    }

    private void Start()
    {
        setUpTime1 = skillTimeDamageGraph.damageGraph[0].time;
        setUpTime2 = skillTimeDamageGraph.damageGraph[1].time - setUpTime1;
        moveTime1 = ComputeMoveTime1(skillTimeDamageGraph);
        moveTime2 = skillTimeDamageGraph.damageGraph[4].time - (skillTimeDamageGraph.damageGraph[2].time + skillTimeDamageGraph.damageGraph[3].time) / 2;
        StartCoroutine(PlayCoroutine());
    }
    private IEnumerator PlayCoroutine()
    {
        // 1단계: setUpTime1 화살표+선 페이드인
        float time = 0f;
        while (time < setUpTime1)
        {
            time += Time.deltaTime;
            setAlpha(arrow1, Mathf.Clamp01(time / setUpTime1));
            setAlpha(arrow2, Mathf.Clamp01(time / setUpTime2));
            setAlpha(line, Mathf.Clamp01(time / setUpTime2));
            yield return null;
        }

        // 2단계: setUpTime2 대기
        time = 0f;
        while (time < setUpTime2)
        {
            time += Time.deltaTime;
            yield return null;
        }

        // 3단계: moveTime1 화살표 중앙까지 이동
        Vector3 initialScale = line.transform.localScale;
        Vector3 initialPosA1 = arrow1.transform.localPosition;
        Vector3 initialPosA2 = arrow2.transform.localPosition;
        time = 0f;
        while (time < moveTime1 && !skipToStage4)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveTime1);
            float et = t * t;
            line.transform.localScale = new Vector3(initialScale.x, Mathf.Lerp(initialScale.y, 0f, et), initialScale.z);
            arrow1.transform.localPosition = new Vector3(initialPosA1.x, Mathf.Lerp(initialPosA1.y, 0f, et), initialPosA1.z);
            arrow2.transform.localPosition = new Vector3(initialPosA2.x, Mathf.Lerp(initialPosA2.y, 0f, et), initialPosA2.z);
            yield return null;
        }

        Vector3 currentPosA1 = arrow1.transform.localPosition;
        Vector3 currentPosA2 = arrow2.transform.localPosition;

        // 4단계: moveTime2 화살표 등속 이동 + 페이드아웃
        // moveTime1이 끝나는 시점(et=1)의 순간 속도를 구해서 moveTime2 동안 그 속도 그대로 등속으로 이어서 이동시킨다.
        // pos(t) = initialPos * (1 - (t/moveTime1)^2) 이므로 t=moveTime1에서의 미분값은 -2*initialPos/moveTime1.
        float speedA1 = -2f * initialPosA1.y / moveTime1;
        float speedA2 = -2f * initialPosA2.y / moveTime1;
        float speedMultiplier = 1f; // 속도 조정용 multiplier, 필요시 조정 가능
        time = 0f;
        line.SetActive(false);
        while (time < moveTime2)
        {
            time += Time.deltaTime;
            speedMultiplier = Mathf.Lerp(1f, 2f, time / moveTime2); 
            float clampedTime = Mathf.Min(time, moveTime2);
            arrow1.transform.localPosition = new Vector3(currentPosA1.x, currentPosA1.y + speedA1 * clampedTime * speedMultiplier, currentPosA1.z);
            arrow2.transform.localPosition = new Vector3(currentPosA2.x, currentPosA2.y + speedA2 * clampedTime * speedMultiplier, currentPosA2.z);
            setAlpha(arrow1, Mathf.Lerp(1f, 0f, time / moveTime2));
            setAlpha(arrow2, Mathf.Lerp(1f, 0f, time / moveTime2));
            yield return null;
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void setAlpha(GameObject target, float alpha)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }

        // setAlpha에서 TrailRenderer의 alpha는 조절하지 못하므로 별도 관리
        TrailRenderer tr = target.GetComponent<TrailRenderer>();
        if (tr != null)
        {
            if (!baseTrailAlphaKeys.TryGetValue(tr, out GradientAlphaKey[] baseKeys))
            {
                baseKeys = tr.colorGradient.alphaKeys;
                baseTrailAlphaKeys[tr] = baseKeys;
            }

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[baseKeys.Length];
            for (int i = 0; i < baseKeys.Length; i++)
            {
                alphaKeys[i] = new GradientAlphaKey(baseKeys[i].alpha * alpha, baseKeys[i].time);
            }

            Gradient gradient = tr.colorGradient;
            gradient.SetKeys(gradient.colorKeys, alphaKeys);
            tr.colorGradient = gradient;
        }
    }
}
