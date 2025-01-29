using UnityEngine;

public class LineRendererHelper : MonoBehaviour
{
    [SerializeField] private LineRenderer lr;
    private Transform[] points;

    // Start is called before the first frame update
    void Start()
    {
        lr.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (lr.positionCount > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].position == null || !points[i].gameObject.activeInHierarchy) return;
                lr.SetPosition(i, points[i].position);
            }
        }

    }

    public void SetUpLine(Transform[] points)
    {
        lr.positionCount = points.Length;
        this.points = points;
    }

    public void EraseLines()
    {
        lr.positionCount = 0;
    }
}
