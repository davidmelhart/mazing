 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {

    public int value = 0;

	public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDistanceTreshold;

    

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    private bool _targetDetected;

    public bool targetDetected {
        get {
            return _targetDetected;
        }
        set {
            _targetDetected = value;
        }
    }

    private void Start() {
        StartCoroutine("FindTargets");
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    IEnumerator FindTargets () {
        while(true) {
            yield return null;
            FindVisibleTargets();
        }
    }

    private void LateUpdate() {
        DrawFoV();
    }

    void FindVisibleTargets() {
        visibleTargets.Clear();
        targetDetected = false;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++) {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.up, directionToTarget) < viewAngle /2) {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)) {
                    visibleTargets.Add(target);
                    targetDetected = true;
                }
            }
        }
    }
    

    void DrawFoV() {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngle = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++) {
            float angle = -transform.eulerAngles.z - viewAngle / 2 + stepAngle * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0) {
                bool edgeDistanceTresholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceTreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceTresholdExceeded)) {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero) {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++) {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceTresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceTreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceTresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCast.point;
            } else {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    public Vector3 DirFromAngle (float angleInDegrees, bool isGlobal) {
        if (!isGlobal) {
            angleInDegrees -= transform.eulerAngles.z;
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    ViewCastInfo ViewCast(float globalAngle) {
        Vector3 direction = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, direction, out hit, viewRadius, obstacleMask + targetMask)) {
            //Debug.DrawLine(transform.position, hit.point, new Color(1, 1, 1, 0.1f));
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        } else {
            //Debug.DrawLine(transform.position, transform.position + direction * viewRadius, new Color(1, 1, 1, 0.1f));
            return new ViewCastInfo(true, transform.position + direction * viewRadius, viewRadius, globalAngle);
        }
    } 

    public struct ViewCastInfo {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo (bool _hit, Vector3 _point, float _distance, float _angle) {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    public struct EdgeInfo {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
