using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleEdgeCollider : MonoBehaviour
{
    [SerializeField] int numPoints;
    [SerializeField] float openingAngle;
    [SerializeField] float radius;

    // Use this for initialization
    void Awake()
    {
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        Vector2[] points = new Vector2[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            float angle = (2 * Mathf.PI - openingAngle * Mathf.Deg2Rad) * i / numPoints;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        edgeCollider.points = points;
    }
}