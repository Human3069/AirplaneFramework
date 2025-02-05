using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Physics/Trajectory Predictor")]
public class TrajectoryPredictor : MonoBehaviour {

    public enum lineMode {
        DrawRayEditorOnly = 1,
        LineRendererBoth = 2
    };
    public enum predictionMode {
        Prediction2D = 2,
        Prediction3D = 3
    };

    [Header("Prediction Settings")]
    [Range(0.7f, 0.9999f)]
    [Tooltip("The accuracy of the prediction. This controls the distance between steps in calculation.")]
    public float accuracy = 0.98f;

    [Tooltip("Limit on how many steps the prediction can take before stopping.")]
    public int iterationLimit = 150;

    [Tooltip("Whether the prediction should be a 2D or 3D line.")]
    public predictionMode predictionType = predictionMode.Prediction3D;

    [Tooltip("The layer mask to use for raycasting when calculating the prediction. This setting only matters when checkForCollision is on.")]
    public LayerMask raycastMask = -1;

    [Tooltip("Stop the prediction where the line hits an object? This check works by using raycasts, so you can use the mask and putting objects on different layers to make them not be checked for collision.")]
    public bool checkForCollision = true;

    // bounceOnCollision is commented out because the physically interaction between two objects in unity is somewhat too difficult to simulate 
    // properly in a simple fashion. The current implementation is somewhat inaccurate because it does not take into account things like 
    // static and dynamic friction of the two bodies and combine factors of physics materials among other factors.

    //[Tooltip("If checkForCollision is set to true this will perform a bounce at the impact location using the objects physics material.")]
    //public bool bounceOnCollision = true
    private static bool bounceOnCollision = false;

    [Header("Line Settings")]
    [Tooltip("The type of line to draw for debug stuff. DrawRay: uses built in Debug.DrawRay only visible in editor. LineRenderer: uses a line renderer on a separate created GameObject to draw the line, is visble in editor and play mode")]
    public lineMode debugLineMode = lineMode.LineRendererBoth;

    [Tooltip("Draw a debug line on object start? (Requires a rigidbody or rigidbody2D)")]
    public bool drawDebugOnStart = false;

    [Tooltip("Draw a debug line on object update? (Requires a rigidbody or rigidbody2D)")]
    public bool isDrawDebugOnUpdate = false;

    [Tooltip("Draw a debug line when predicting the trajectory")]
    public bool drawDebugOnPrediction = false;

    [Tooltip("Duration the prediction line lasts for. When predicting every frame its a good idea to update this value to Time.unscaledDeltaTime every frame. (This is done automatically if you use the drawDebugOnUpdate option)")]
    public float debugLineDuration = 4f;

    [Tooltip("Number of frames that pass before the line is refreshed. Increasing this number could significantly improve performance with a large amount of lines being predicted at once. (Only used if drawDebugOnUpdate is enabled.)")]
    [Range(1, 10)]
    public int debugLineUpdateRate = 1;

    [Tooltip("If using the linerenderer, will reuse the gameobject and line renderer components instead of destroying and recreating them every time. " +
        "This option improves performance for multiple succesive predictions DON'T use this for one-off predictions, " +
        "as it will not take line duration into account and the line will stick around forever until the component is destroyed. " +
        "NOTE: this option is automatically used by drawDebugOnUpdate and does not need to be enabled here for that to work.")]
    public bool reuseLine = false;

    [Tooltip("The name of the layer the line is drawn on. Only checked once on start")]
    public string lineSortingLayerName;
    private int lineSortingLayer = 0;

    [Tooltip("The order in the sorting layer the line is drawn.")]
    public int lineSortingOrder = 0;

    [Header("Line Appearance")]
    [Tooltip("Thickness of the debug line when using the line renderer mode.")]
    public float lineWidth = 0.05f;

    [Tooltip("Start color of the debug line")]
    public Color lineStartColor = Color.white;

    [Tooltip("End color of the debug line")]
    public Color lineEndColor = Color.white;

    [Tooltip("If provided, this shader will be used on the LineRenderer. (Recommended is the particles section of shaders)")]
    public Shader lineShader;
  
    [Tooltip("If provided, this texture will be added to the material of the LineRenderer. (A couple textures come packaged with the script)")]
    public Texture lineTexture;
   
    [Tooltip("Value to scale the tiling of the line texture by.")]
    public float textureTilingMult = 1f;

    [HideInInspector]
    public RaycastHit hitInfo3D;
  
    [HideInInspector]
    public RaycastHit2D hitInfo2D;
 
    [HideInInspector]
    public List<Vector3> predictionPoints = new List<Vector3>();

    private Rigidbody _rigidbody;
    private Rigidbody2D _rigidbody2D;

    protected void Awake()
    {
        if (predictionType == predictionMode.Prediction2D)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }
        else
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        lineSortingLayer = SortingLayer.NameToID(lineSortingLayerName);
    }

    protected void Start()
    {
        if (drawDebugOnStart == true)
        {
            if (_rigidbody != null || _rigidbody2D != null)
            {
                bool wasB = drawDebugOnPrediction;
                drawDebugOnPrediction = true;

                if (predictionType == predictionMode.Prediction2D)
                {
                    Predict2D(_rigidbody2D);
                }
                else
                {
                    Predict3D(_rigidbody);
                }
                drawDebugOnPrediction = wasB;

            }
            else
            {
                Debug.LogWarning("Debug on object start requires a rigibody on the object.");
            }
        }
        else if (isDrawDebugOnUpdate)
        {
            if (_rigidbody == null && _rigidbody2D == null)
            {
                Debug.LogWarning("Debug on object update requires a rigibody on the object.");
                isDrawDebugOnUpdate = false;
            }
        }
    }

    private Material _lineMat;
    private bool triedGetMat = false;
    private Material lineMat
    {
        get
        {
            if (triedGetMat == false)
            {
                triedGetMat = true;
                Shader spriteDefault;
                if (lineShader != null)
                {
                    spriteDefault = lineShader;
                }
                else
                {
                    spriteDefault = Shader.Find("Particles/Alpha Blended");
                }

                if (spriteDefault == null)
                {
                    spriteDefault = Shader.Find("Mobile/Particles/Alpha Blended");
                }

                if (spriteDefault != null)
                {
                    _lineMat = new Material(spriteDefault);
                }
            }

            return _lineMat;
        }
    }

    private int frameCount = 0;

    protected void Update()
    {
        if (isDrawDebugOnUpdate == true)
        {
            if (_rigidbody != null || _rigidbody2D != null)
            {
                frameCount++;
                if (frameCount % debugLineUpdateRate == 0)
                {
                    frameCount = 0;
                    debugLineDuration = Time.unscaledDeltaTime * debugLineUpdateRate;

                    bool wasB = drawDebugOnPrediction;
                    bool wasReuse = reuseLine;

                    drawDebugOnPrediction = true;
                    reuseLine = true;

                    if (predictionType == predictionMode.Prediction2D)
                    {
                        if (_rigidbody2D.linearVelocity.sqrMagnitude > 0.05f)
                        {
                            Predict2D(_rigidbody2D);
                        }
                    }
                    else
                    {
                        if (_rigidbody.linearVelocity.sqrMagnitude > 0.05f)
                        {
                            Predict3D(_rigidbody);
                        }
                    }

                    drawDebugOnPrediction = wasB;
                    reuseLine = wasReuse;
                }
            }
        }
    }

    private GameObject debugLineObj;
    [HideInInspector]
    public LineRenderer debugLine;
    
    private void LineDebug(List<Vector3> pointList)
    {
        if (debugLineObj == null || (reuseLine == false && debugLineObj == true))
        {
            StopAllCoroutines();
            Destroy(debugLineObj);

            debugLineObj = new GameObject();
            debugLineObj.name = "Debug Line";

            if (this.transform != null)
            {
                debugLineObj.transform.SetParent(transform);
            }
            debugLine = debugLineObj.AddComponent<LineRenderer>();

            debugLine.useWorldSpace = true;
            debugLine.receiveShadows = false;
            debugLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            debugLine.sortingOrder = lineSortingOrder;
            debugLine.sortingLayerID = lineSortingLayer;
            debugLine.sharedMaterial = lineMat;

            if (lineTexture != null)
            {
                debugLine.sharedMaterial.mainTexture = lineTexture;                
            }
        }

#if UNITY_5_5_OR_NEWER
        debugLine.startColor = lineStartColor;
        debugLine.endColor = lineEndColor;
        debugLine.startWidth = lineWidth;
        debugLine.endWidth = lineWidth;
        debugLine.positionCount = pointList.Count;
#else
        debugLine.SetColors(lineStartColor, lineEndColor);
        debugLine.SetWidth(lineWidth, lineWidth);
        debugLine.SetVertexCount(pointList.Count);
#endif

        if (lineTexture != null)
        {
            debugLine.sharedMaterial.mainTextureScale = new Vector2(lineDistance / lineWidth / 2f * textureTilingMult, 1f);
        }

        debugLine.SetPositions(pointList.ToArray());

        if (reuseLine == false)
        {
            StartCoroutine(KillLineDelay(debugLineDuration));
        }
    }

    protected void OnDestroy()
    {
        if (debugLineObj != null)
        {
            Destroy(debugLineObj);
        }
    }

    private IEnumerator KillLineDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (debugLineObj != null)
        {
            Destroy(debugLineObj);
        }

        debugLineObj = null;
        debugLine = null;
    }

    private float _linearDrag;
    private Vector3 _velocity;
    private Vector3 _startPos;
    private Vector3 _gravity;

    ///<summary>
    ///Given these values, perform velocity prediction in 3D. 
    ///Results can be found in the instance of the class' hitInfo and predictionPoints variables.
    ///</summary>
    public void Predict3D(Vector3 startPos, Vector3 velocity, Vector3? gravity = null, float linearDrag = 0f)
    {
        _linearDrag = linearDrag;
        _velocity = velocity;
        _startPos = startPos;
        _gravity = gravity == null ? Physics.gravity : gravity.Value;

        PerformPrediction();
    }

    ///<summary>
    ///Given these values, perform velocity prediction in 2D. 
    ///Results can be found in the instance of the class' hitInfo and predictionPoints variables.
    ///</summary>
    public void Predict2D(Vector3 startPos, Vector2 velocity, Vector2 gravity, float linearDrag = 0f)
    {
        _linearDrag = linearDrag;
        _velocity = velocity;
        _startPos = startPos;
        _gravity = gravity;

        PerformPrediction();
    }

    ///<summary>
    ///Given a RigidBody, perform velocity prediction in 3D.
    ///Results can be found in the instance of the class' hitInfo and predictionPoints variables.
    ///</summary>
    public void Predict3D(Rigidbody _rigidbody)
    {
        _linearDrag = _rigidbody.linearDamping;
        _velocity = _rigidbody.linearVelocity;
        _startPos = _rigidbody.position;        
        _gravity = _rigidbody.useGravity ? Physics.gravity : Vector3.zero;

        PerformPrediction();
    }

    ///<summary>
    ///Given a RigidBody2D, perform velocity prediction in 2D.
    ///Results can be found in the instance of the class' hitInfo and predictionPoints variables.
    ///</summary>
    public void Predict2D(Rigidbody2D _rigidbody)
    {
        _linearDrag = _rigidbody.linearDamping;
        _velocity = _rigidbody.linearVelocity;
        _startPos = _rigidbody.position;
        _gravity = _rigidbody.gravityScale * Physics2D.gravity;

        PerformPrediction();
    }

    ///<summary>
    ///Given these values, get an array of points representing the trajectory in 3D without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static Vector3[] GetPoints3D(Vector3 startPos, Vector3 velocity, Vector3? gravity = null, float linearDrag = 0f, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision;
        predictor.Predict3D(startPos, velocity, gravity, linearDrag);

        Destroy(newObj);

        return predictor.predictionPoints.ToArray();
    }

    ///<summary>
    ///Given these values, get an array of points representing the trajectory in 2D without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static Vector3[] GetPoints2D(Vector3 startPos, Vector2 velocity, Vector2 gravity, float linearDrag = 0f, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision; predictor.predictionType = predictionMode.Prediction2D;
        predictor.Predict2D(startPos, velocity, gravity, linearDrag);

        Destroy(newObj);

        return predictor.predictionPoints.ToArray();
    }

    ///<summary>
    ///Given these values, get an array of points representing the trajectory in 3D without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static Vector3[] GetPoints3D(Rigidbody rb, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision;
        predictor.Predict3D(rb);

        Destroy(newObj);

        return predictor.predictionPoints.ToArray();
    }

    ///<summary>
    ///Given these values, get an array of points representing the trajectory in 2D without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static Vector3[] GetPoints2D(Rigidbody2D rb, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision; predictor.predictionType = predictionMode.Prediction2D;
        predictor.Predict2D(rb);

        Destroy(newObj);

        return predictor.predictionPoints.ToArray();
    }

    ///<summary>
    ///Given these values, get a RaycastHit of where the trajectory collides with an object,
    /// without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static RaycastHit GetHitInfo3D(Vector3 startPos, Vector3 velocity, Vector3? gravity = null, float linearDrag = 0f, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision;
        predictor.Predict3D(startPos, velocity, gravity, linearDrag);

        Destroy(newObj);

        return predictor.hitInfo3D;
    }

    ///<summary>
    ///Given these values, get a RaycastHit of where the trajectory collides with an object,
    /// without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static RaycastHit GetHitInfo3D(Rigidbody _rigidbody, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision;
        predictor.Predict3D(_rigidbody);

        Destroy(newObj);

        return predictor.hitInfo3D;
    }

    ///<summary>
    ///Given these values, get a RaycastHit2D of where the trajectory collides with an object,
    /// without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static RaycastHit2D GetHitInfo2D(Vector3 startPos, Vector2 velocity, Vector2 gravity, float linearDrag = 0f, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision; predictor.predictionType = predictionMode.Prediction2D;
        predictor.Predict2D(startPos, velocity, gravity, linearDrag);

        Destroy(newObj);

        return predictor.hitInfo2D;
    }

    ///<summary>
    ///Given these values, get a RaycastHit2D of where the trajectory collides with an object,
    /// without needing to create an instance of the class or use it as a component.
    ///</summary>
    public static RaycastHit2D GetHitInfo2D(Rigidbody2D rb, float accuracy = 0.985f, int iterationLimit = 150, bool stopOnCollision = true, int rayCastMask = -1)
    {
        GameObject newObj = new GameObject();
        newObj.name = "TrajectoryPredictionObj";

        TrajectoryPredictor predictor = newObj.AddComponent<TrajectoryPredictor>();
        predictor.raycastMask = rayCastMask;
        predictor.accuracy = accuracy; predictor.iterationLimit = iterationLimit; predictor.checkForCollision = stopOnCollision; predictor.predictionType = predictionMode.Prediction2D;
        predictor.Predict2D(rb);

        Destroy(newObj);

        return predictor.hitInfo2D;
    }

    //last prediction line total distance travelled
    [HideInInspector]
    public float lineDistance = 0f;

    /// <summary>
    /// An event that is called at every iteration of the prediction so you can write custom code for custom physics like objects orbiting a planet.
    /// </summary>
    /// <param name="currentIterationVel">Modify this value to apply your custom velocity to be taken into account in the prediction at each interval.</param>
    /// <param name="currentIterationPos">Current world position of the prediction at this interval.</param>
    /// <param name="tpInstance">Reference to the instance of the trajectory predictor doing the calculations.</param>
    public delegate void PredictionIterationEvent(ref Vector3 currentIterationVel, Vector3 currentIterationPos, TrajectoryPredictor tpInstance);

    /// <summary>
    /// An event that is called at every iteration of the prediction so you can write custom code for custom physics like objects orbiting a planet.
    /// </summary>
    public PredictionIterationEvent OnPredictionIterationStep;

    private void PerformPrediction()
    {
        Vector3 directionWithLength = Vector3.zero;
        Vector3 toPos;

        bool isDone = false;

        int iterated = 0;
        lineDistance = 0f;

        float compAcc = 1f - accuracy;
        Vector3 gravAdd = _gravity * compAcc;
        float dragMult = Mathf.Clamp01(1f - _linearDrag * compAcc);
        predictionPoints.Clear();
        while (isDone == false && iterated < iterationLimit)
        {
            if (OnPredictionIterationStep != null)
            {
                Vector3 iteratedPos = Vector3.zero;
                OnPredictionIterationStep(ref iteratedPos, _startPos, this);
                _velocity += iteratedPos * compAcc;
            }

            _velocity += gravAdd;
            _velocity *= dragMult;

            toPos = _startPos + _velocity * compAcc;
            directionWithLength = toPos - _startPos;
            predictionPoints.Add(_startPos);

            float distance = Vector3.Distance(_startPos, toPos);
            lineDistance += distance;
            if (checkForCollision == true)
            {
                if (predictionType == predictionMode.Prediction2D)
                {
                    RaycastHit2D raycastHit = Physics2D.Raycast(_startPos, directionWithLength, distance, raycastMask);
                    if (raycastHit != default)
                    {
                        if (raycastHit.collider.transform != null)
                        {
                            if (raycastHit.collider.transform != this.transform)
                            {
                                hitInfo2D = raycastHit;
                                isDone = true;

                                predictionPoints.Add(raycastHit.point);
                            }
                        }
                    }
                }
                else
                {
                    Ray ray = new Ray(_startPos, directionWithLength);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, distance, raycastMask) == true)
                    {
                        hitInfo3D = hit;
                        predictionPoints.Add(hit.point);
                        if (bounceOnCollision == true)
                        {
                            Collider col = _rigidbody.GetComponent<Collider>();
                            if (col != null)
                            {
                                PhysicsMaterial physicsMaterial = col.sharedMaterial;
                                if (physicsMaterial != null)
                                {
                                    toPos = hit.point;
                                    _velocity = Vector3.Reflect(_velocity, hit.normal) * physicsMaterial.bounciness;
                                }
                            }
                        }
                        else
                        {
                            isDone = true;
                        }
                    }
                }
            }

            if (drawDebugOnPrediction == true && debugLineMode == lineMode.DrawRayEditorOnly)
            {
                Debug.DrawRay(_startPos, directionWithLength, lineStartColor, debugLineDuration);
            }

            _startPos = toPos;
            iterated++;
        }

        if (drawDebugOnPrediction == true && debugLineMode == lineMode.LineRendererBoth)
        {
            LineDebug(predictionPoints);
        }
    }
}
