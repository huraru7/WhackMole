using System.Collections;
using UnityEngine;

public class MoleController : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private float hiddenY = -1.2f;
    [SerializeField] private float visibleY = 0.8f;

    [Header("Timing")]
    [SerializeField] private float minHiddenWait = 0.5f;
    [SerializeField] private float maxHiddenWait = 2.0f;
    [SerializeField] private float minVisibleDwell = 1.0f;
    [SerializeField] private float maxVisibleDwell = 3.0f;

    [Header("Animation")]
    [SerializeField] private float riseSpeed = 5f;
    [SerializeField] private float sinkSpeed = 4f;

    private bool hasBeenHit;
    private bool isUp;
    private Coroutine moleRoutine;

    private void Awake() => SetY(hiddenY);

    public void StartMole()
    {
        if (moleRoutine != null) StopCoroutine(moleRoutine);
        moleRoutine = StartCoroutine(MoleCycle());
    }

    public void StopMole()
    {
        if (moleRoutine != null) { StopCoroutine(moleRoutine); moleRoutine = null; }
        isUp = false;
        StartCoroutine(MoveTo(hiddenY, sinkSpeed));
    }

    // GameManager がレイキャストで叩いたモグラに対して呼ぶ。
    // （新 Input System のみ有効な設定では OnMouseDown が呼ばれないため、入力は GameManager 側で処理する）
    public void TryHit()
    {
        if (!isUp || hasBeenHit || !GameManager.Instance.IsGameRunning) return;
        hasBeenHit = true;
        GameManager.Instance.AddScore();
        if (moleRoutine != null) StopCoroutine(moleRoutine);
        moleRoutine = StartCoroutine(HitReaction());
    }

    private IEnumerator MoleCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minHiddenWait, maxHiddenWait));
            if (!GameManager.Instance.IsGameRunning) yield break;

            hasBeenHit = false;
            yield return StartCoroutine(MoveTo(visibleY, riseSpeed));
            isUp = true;

            yield return new WaitForSeconds(Random.Range(minVisibleDwell, maxVisibleDwell));

            isUp = false;
            yield return StartCoroutine(MoveTo(hiddenY, sinkSpeed));
        }
    }

    private IEnumerator MoveTo(float targetY, float speed)
    {
        float startY = transform.localPosition.y;
        if (Mathf.Approximately(startY, targetY)) yield break;
        float t = 0f;
        float dist = Mathf.Abs(targetY - startY);
        while (t < 1f)
        {
            t += Time.deltaTime * speed / dist;
            SetY(Mathf.Lerp(startY, targetY, Mathf.Clamp01(t)));
            yield return null;
        }
    }

    private IEnumerator HitReaction()
    {
        isUp = false;
        yield return StartCoroutine(MoveTo(hiddenY, sinkSpeed * 1.5f));
        if (!GameManager.Instance.IsGameRunning) yield break;
        moleRoutine = StartCoroutine(MoleCycle());
    }

    private void SetY(float y)
    {
        var p = transform.localPosition;
        p.y = y;
        transform.localPosition = p;
    }
}
