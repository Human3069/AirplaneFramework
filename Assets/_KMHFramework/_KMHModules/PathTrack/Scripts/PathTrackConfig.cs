using UnityEngine;

[CreateAssetMenu(fileName = "PathTrackConfig", menuName = "ScriptableObjects/PathTrackConfig", order = int.MaxValue)]
public class PathTrackConfig : ScriptableObject
{
    public Mesh PointMesh;
    public Color PointMeshColor = Color.red;

    [Space(10)]
    public Color PathTrackGizmoColor = Color.green;

    [Space(10)]
    public float BezierHandleSize = 0.25f;
    public Color BezierColor = Color.red;

    [Space(10)]
    public float SamplingPointSize = 0.1f;
    public Color SamplingPointColor = Color.blue;
}