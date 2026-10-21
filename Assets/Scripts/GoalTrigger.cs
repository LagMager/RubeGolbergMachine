using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Tooltip("Controller notified when the goal is reached.")]
    public RubeGoldbergController controller;

    [Tooltip("Optional renderer (e.g., goal ring or beacon) that lights up on success.")]
    public Renderer indicatorRenderer;

    [Tooltip("Optional particle system played when the ball lands in the goal cup.")]
    public ParticleSystem celebrationParticles;

    [Tooltip("Target ball transform for distance fallback detection.")]
    public Transform targetBall;

    public bool HasTriggered { get; private set; }

    private Color _initialColor;
    private Material _indicatorMat;

    private void Awake()
    {
        if (indicatorRenderer != null)
        {
            _indicatorMat = indicatorRenderer.material;
            if (_indicatorMat.HasProperty("_BaseColor"))
            {
                _initialColor = _indicatorMat.GetColor("_BaseColor");
            }
            else if (_indicatorMat.HasProperty("_Color"))
            {
                _initialColor = _indicatorMat.color;
            }
        }
    }

    private void Update()
    {
        if (!HasTriggered && targetBall != null)
        {
            float dist = Vector3.Distance(transform.position, targetBall.position);
            if (dist < 0.65f)
            {
                TriggerGoal();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HasTriggered) return;

        if (other.attachedRigidbody != null)
        {
            TriggerGoal();
        }
    }

    public void TriggerGoal()
    {
        if (HasTriggered) return;
        HasTriggered = true;

        if (_indicatorMat != null)
        {
            Color winColor = new Color(0.15f, 0.95f, 0.35f, 1f);
            if (_indicatorMat.HasProperty("_BaseColor"))
                _indicatorMat.SetColor("_BaseColor", winColor);
            if (_indicatorMat.HasProperty("_Color"))
                _indicatorMat.color = winColor;
            if (_indicatorMat.HasProperty("_EmissionColor"))
            {
                _indicatorMat.EnableKeyword("_EMISSION");
                _indicatorMat.SetColor("_EmissionColor", winColor * 2.5f);
            }
        }

        if (celebrationParticles != null)
        {
            celebrationParticles.Play();
        }

        if (controller != null)
        {
            controller.OnGoalReached();
        }
    }

    public void ResetGoal()
    {
        HasTriggered = false;

        if (_indicatorMat != null)
        {
            if (_indicatorMat.HasProperty("_BaseColor"))
                _indicatorMat.SetColor("_BaseColor", _initialColor);
            if (_indicatorMat.HasProperty("_Color"))
                _indicatorMat.color = _initialColor;
            if (_indicatorMat.HasProperty("_EmissionColor"))
                _indicatorMat.SetColor("_EmissionColor", Color.black);
        }

        if (celebrationParticles != null)
        {
            celebrationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
