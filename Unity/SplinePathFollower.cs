using GLTFast;
using UnityEngine;

public class SplinePathFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _Target;
    [SerializeField] private Transform[] _Points;

    [Header("Settings")]
    [SerializeField] private float _TimeForCycle = 5f;
    [SerializeField] private AnimationCurve _Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Spline")]
    [SerializeField] private float _Tau = 0.9f;

    [Header("Debug")]
    [SerializeField] private bool _DrawGizmos = false;
    [SerializeField] private bool _StartOnAwake = false;

    private bool _IsActive = false;
    private float _NormalizedTime = 0f;

    private const float MinimumTargetCycleTime = 0.001f;
    private const int GizmoStepsPerSegment = 10;
    private const float BaseTauFactor = 0.5f;

    private void Awake()
    {
        if (_StartOnAwake)
        {
            StartPath();
        }
    }

    private void Update()
    {
        if (!_IsActive)
        {
            return;
        }

        UpdatePathProgress();
    }

    public void StartPath()
    {
        if (_Points == null || _Points.Length < 2 || _Target == null)
        {
            return;
        }

        _IsActive = true;
        _NormalizedTime = 0f;
    }

    public void StopPath()
    {
        _IsActive = false;
    }

    public void ResumePath()
    {
        if (_Points == null || _Points.Length < 2 || _Target == null)
        {
            return;
        }

        _IsActive = true;
    }

    private void UpdatePathProgress()
    {
        float lCycleDuration = Mathf.Max(_TimeForCycle, MinimumTargetCycleTime);
        float lEvaluatedDelta = _Curve.Evaluate(Time.deltaTime / lCycleDuration);
        
        _NormalizedTime += lEvaluatedDelta;

        if (_NormalizedTime >= 1f)
        {
            _NormalizedTime = 1f;
            _IsActive = false;
        }

        _Target.position = EvaluateSpline(_NormalizedTime);
    }

    public static Vector3 Catmull(Vector3 pP0, Vector3 pP1, Vector3 pP2, Vector3 pP3, float pT, float pTau = BaseTauFactor)
    {
        float lT2 = pT * pT;
        float lT3 = lT2 * pT;

        float lW0 = -pTau * pT + 2f * pTau * lT2 - pTau * lT3;
        float lW1 = 1f + (pTau - 3f) * lT2 + (2f - pTau) * lT3;
        float lW2 = pTau * pT + (3f - 2f * pTau) * lT2 + (pTau - 2f) * lT3;
        float lW3 = -pTau * lT2 + pTau * lT3;

        return (lW0 * pP0) + (lW1 * pP1) + (lW2 * pP2) + (lW3 * pP3);
    }

    private Vector3 EvaluateSpline(float pT)
    {
        int lNumSegments = _Points.Length - 1;
        float lPathProgress = pT * lNumSegments;
        int lCurrentSegment = Mathf.FloorToInt(lPathProgress);

        if (lCurrentSegment >= lNumSegments)
        {
            lCurrentSegment = lNumSegments - 1;
            lPathProgress = lNumSegments;
        }

        Vector3 lP0 = _Points[Mathf.Max(lCurrentSegment - 1, 0)].position;
        Vector3 lP1 = _Points[lCurrentSegment].position;
        Vector3 lP2 = _Points[Mathf.Min(lCurrentSegment + 1, _Points.Length - 1)].position;
        Vector3 lP3 = _Points[Mathf.Min(lCurrentSegment + 2, _Points.Length - 1)].position;

        return Catmull(lP0, l1, lP2, lP3, lPathProgress - lCurrentSegment, _Tau);
    }

    [ContextMenu("Set Points With Children")]
    private void SetPointsWithChildren()
    {
        _Points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            _Points[i] = transform.GetChild(i);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_DrawGizmos || _Points == null || _Points.Length < 2)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Vector3 lPreviousPoint = _Points[0].position;
        int lTotalSteps = GizmoStepsPerSegment * (_Points.Length - 1);

        for (int i = 1; i <= lTotalSteps; i++)
        {
            float lT = (float)i / lTotalSteps;
            Vector3 lCurrentPoint = EvaluateSpline(lT);

            Gizmos.DrawLine(lPreviousPoint, lCurrentPoint);
            lPreviousPoint = lCurrentPoint;
        }
    }
#endif
}