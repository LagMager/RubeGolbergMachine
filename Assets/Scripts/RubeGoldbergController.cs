using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RubeGoldbergController : MonoBehaviour
{
    [Header("Machine References")]
    public Rigidbody starterBall;
    public Transform releaseGate;
    public Rigidbody[] dominoes;
    public Rigidbody pendulumBob;
    public Rigidbody finalBall;
    public GoalTrigger goalTrigger;

    [Header("Trigger & Timing")]
    public bool autoStartOnPlay = true;
    public float autoStartDelay = 1.25f;
    public Vector3 starterBallInitialImpulse = new Vector3(1.8f, 0f, 0f);

    [Header("Camera Settings")]
    public Camera targetCamera;
    public bool followActiveStage = true;
    public Vector3 overviewPosition = new Vector3(1.0f, 4.8f, -12.5f);
    public Vector3 overviewLookTarget = new Vector3(1.0f, 3.0f, 0f);
    public Vector3 followOffset = new Vector3(0f, 1.8f, -6.5f);
    public float cameraSmoothTime = 0.32f;

    private struct BodySnapshot
    {
        public Rigidbody Body;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool WasKinematic;
    }

    private readonly List<BodySnapshot> _snapshots = new List<BodySnapshot>();
    private Vector3 _gateClosedPos;
    private Vector3 _gateOpenPos;
    private bool _isRunning;
    private bool _goalReached;
    private bool _pendulumBoosted;
    private float _startTimer;
    private int _currentStageIndex;
    private Vector3 _camVel;
    private Vector3 _currentLookTarget;
    private Vector3 _lookVel;

    private static readonly string[] StageNames =
    {
        "Ready - Press [Space] or Wait for Auto-Launch",
        "Stage 1: Angled Ramps & Rolling Ball",
        "Stage 2: Domino Topple Cascade",
        "Stage 3: Swinging Pendulum Hammer",
        "Stage 4: Drop Chute & Funnel Target",
        "COMPLETE! Final Ball in Goal Cup!"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureMachineExistsAtRuntime()
    {
        if (FindFirstObjectByType<RubeGoldbergController>() == null)
        {
            RubeGoldbergBuilder.BuildMachineInCurrentScene();
        }
    }

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (releaseGate != null)
        {
            _gateClosedPos = releaseGate.localPosition;
            _gateOpenPos = _gateClosedPos + new Vector3(0f, 0.9f, 0f);
        }

        RecordSnapshots();
        PrepareReadyState();
    }

    public void RecordSnapshots()
    {
        _snapshots.Clear();
        Rigidbody[] allBodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in allBodies)
        {
            _snapshots.Add(new BodySnapshot
            {
                Body = rb,
                Position = rb.transform.localPosition,
                Rotation = rb.transform.localRotation,
                WasKinematic = rb.isKinematic
            });
        }
    }

    private void PrepareReadyState()
    {
        _isRunning = false;
        _goalReached = false;
        _pendulumBoosted = false;
        _startTimer = 0f;
        _currentStageIndex = 0;

        if (releaseGate != null)
        {
            releaseGate.localPosition = _gateClosedPos;
        }

        if (starterBall != null)
        {
            starterBall.isKinematic = true;
        }

        if (pendulumBob != null)
        {
            pendulumBob.Sleep();
        }

        if (finalBall != null)
        {
            finalBall.Sleep();
        }

        if (goalTrigger != null)
        {
            goalTrigger.ResetGoal();
        }

        _currentLookTarget = starterBall != null ? starterBall.position : overviewLookTarget;
    }

    public void StartMachine()
    {
        if (_isRunning) return;
        _isRunning = true;
        _currentStageIndex = 1;

        if (starterBall != null)
        {
            starterBall.isKinematic = false;
            starterBall.WakeUp();
            starterBall.linearVelocity = starterBallInitialImpulse;
        }
    }

    public void ResetMachine()
    {
        if (_snapshots.Count == 0)
        {
            RecordSnapshots();
        }

        foreach (BodySnapshot snap in _snapshots)
        {
            if (snap.Body == null) continue;
            snap.Body.isKinematic = false;
            snap.Body.linearVelocity = Vector3.zero;
            snap.Body.angularVelocity = Vector3.zero;
            snap.Body.transform.localPosition = snap.Position;
            snap.Body.transform.localRotation = snap.Rotation;
            snap.Body.isKinematic = snap.WasKinematic;
            if (!snap.Body.isKinematic)
            {
                snap.Body.Sleep();
            }
        }

        Physics.SyncTransforms();
        PrepareReadyState();
    }

    public void OnGoalReached()
    {
        _goalReached = true;
        _currentStageIndex = 5;
    }

    private void FixedUpdate()
    {
        if (!_isRunning) return;

        // When the last domino tips far enough into the pendulum hammer, give the pendulum a clean swing impulse
        // so the energy transfer from the small domino to the heavy hammer and final ball is crisp and reliable.
        if (!_pendulumBoosted && dominoes != null && dominoes.Length > 0 && pendulumBob != null)
        {
            Rigidbody lastDomino = dominoes[dominoes.Length - 1];
            if (lastDomino != null)
            {
                float tiltAngle = Vector3.Angle(lastDomino.transform.up, Vector3.up);
                if (tiltAngle > 28f)
                {
                    _pendulumBoosted = true;
                    pendulumBob.WakeUp();
                    pendulumBob.AddForceAtPosition(
                        new Vector3(7.5f, 0.5f, 0f),
                        pendulumBob.worldCenterOfMass + new Vector3(0f, -0.55f, 0f),
                        ForceMode.Impulse
                    );
                }
            }
        }
    }

    private void Update()
    {
        HandleInput();

        if (!_isRunning && autoStartOnPlay)
        {
            _startTimer += Time.deltaTime;
            if (_startTimer >= autoStartDelay)
            {
                StartMachine();
            }
        }

        if (_isRunning && releaseGate != null)
        {
            releaseGate.localPosition = Vector3.MoveTowards(
                releaseGate.localPosition,
                _gateOpenPos,
                5.0f * Time.deltaTime
            );
        }

        UpdateStageProgress();
        UpdateCamera();
    }

    private void HandleInput()
    {
        bool launchPressed = false;
        bool resetPressed = false;
        bool camTogglePressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            launchPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            resetPressed = Keyboard.current.rKey.wasPressedThisFrame;
            camTogglePressed = Keyboard.current.cKey.wasPressedThisFrame;
        }
#else
        launchPressed = Input.GetKeyDown(KeyCode.Space);
        resetPressed = Input.GetKeyDown(KeyCode.R);
        camTogglePressed = Input.GetKeyDown(KeyCode.C);
#endif

        if (launchPressed && !_isRunning)
        {
            StartMachine();
        }

        if (resetPressed)
        {
            autoStartOnPlay = false;
            ResetMachine();
        }

        if (camTogglePressed)
        {
            followActiveStage = !followActiveStage;
        }
    }

    private void UpdateStageProgress()
    {
        if (!_isRunning)
        {
            _currentStageIndex = 0;
            return;
        }

        if (_goalReached)
        {
            _currentStageIndex = 5;
            return;
        }

        if (finalBall != null && finalBall.linearVelocity.sqrMagnitude > 0.15f)
        {
            _currentStageIndex = 4;
            return;
        }

        if (_pendulumBoosted || (pendulumBob != null && pendulumBob.angularVelocity.sqrMagnitude > 0.2f))
        {
            _currentStageIndex = Mathf.Max(_currentStageIndex, 3);
            return;
        }

        if (dominoes != null && dominoes.Length > 0)
        {
            foreach (Rigidbody dom in dominoes)
            {
                if (dom != null && dom.angularVelocity.sqrMagnitude > 0.25f)
                {
                    _currentStageIndex = Mathf.Max(_currentStageIndex, 2);
                    break;
                }
            }
        }
    }

    private Vector3 GetStageFocusPoint()
    {
        switch (_currentStageIndex)
        {
            case 0:
            case 1:
                return starterBall != null ? starterBall.position : overviewLookTarget;
            case 2:
                if (dominoes != null && dominoes.Length > 0)
                {
                    Rigidbody activeDomino = dominoes[0];
                    foreach (Rigidbody d in dominoes)
                    {
                        if (d != null && Vector3.Angle(d.transform.up, Vector3.up) > 6f)
                        {
                            activeDomino = d;
                        }
                    }
                    if (activeDomino != null) return activeDomino.position;
                }
                return starterBall != null ? starterBall.position : overviewLookTarget;
            case 3:
                return pendulumBob != null ? pendulumBob.worldCenterOfMass : overviewLookTarget;
            case 4:
            case 5:
                return finalBall != null ? finalBall.position : overviewLookTarget;
            default:
                return overviewLookTarget;
        }
    }

    private void UpdateCamera()
    {
        if (targetCamera == null) return;

        Vector3 desiredLook = followActiveStage ? GetStageFocusPoint() : overviewLookTarget;
        Vector3 desiredPos = followActiveStage ? desiredLook + followOffset : overviewPosition;

        _currentLookTarget = Vector3.SmoothDamp(_currentLookTarget, desiredLook, ref _lookVel, cameraSmoothTime);
        targetCamera.transform.position = Vector3.SmoothDamp(
            targetCamera.transform.position,
            desiredPos,
            ref _camVel,
            cameraSmoothTime
        );

        Vector3 dir = _currentLookTarget - targetCamera.transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            targetCamera.transform.rotation = Quaternion.Slerp(
                targetCamera.transform.rotation,
                targetRot,
                Time.deltaTime * 8f
            );
        }
    }

    private void OnGUI()
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14,
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(12, 12, 10, 10)
        };

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = _goalReached ? new Color(0.3f, 1f, 0.45f) : Color.white;

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13
        };
        subStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);

        GUILayout.BeginArea(new Rect(16, 16, 400, 115), boxStyle);
        GUILayout.Label("Rube Goldberg Contraption", titleStyle);
        GUILayout.Space(2);
        string statusText = StageNames[Mathf.Clamp(_currentStageIndex, 0, StageNames.Length - 1)];
        GUILayout.Label(statusText, subStyle);
        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_isRunning ? "Running..." : "Launch [Space]", GUILayout.Height(28)))
        {
            StartMachine();
        }
        if (GUILayout.Button("Reset [R]", GUILayout.Height(28)))
        {
            autoStartOnPlay = false;
            ResetMachine();
        }
        if (GUILayout.Button(followActiveStage ? "Cam: Follow [C]" : "Cam: Overview [C]", GUILayout.Height(28)))
        {
            followActiveStage = !followActiveStage;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
