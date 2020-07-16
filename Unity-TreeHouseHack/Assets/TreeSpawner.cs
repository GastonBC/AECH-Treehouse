using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunayVoronoi;
using System;

public class TreeSpawner : MonoBehaviour
{
    public float MaxCoordinate;
    public int TreeCount;
    public GameObject TreePrefab;

    // Start is called before the first frame update
    void Start()
    {
        GenDelaunayTreeDecks();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            GenDelaunayTreeDecks();
        }
    }

    void GenDelaunayTreeDecks()
    {
        DelaunayTriangulator delaunay = new DelaunayTriangulator();

        System.Random rnd = new System.Random();
        IEnumerable<Point> points = delaunay.GeneratePoints(TreeCount, MaxCoordinate, MaxCoordinate);

        foreach (Point point in points)
        {
            Vector3 coords = new Vector3(Convert.ToSingle(point.X),
                                         2,  // this is 3rd dimension
                                         Convert.ToSingle(point.Y));

            Quaternion rot = new Quaternion(0, 0, 0, 0);

            Instantiate(TreePrefab, coords, rot);
        }

        IEnumerable<Triangle> triangulation = delaunay.BowyerWatson(points);

    }
}
