﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DelaunayVoronoi;

internal class xyz
{
    double x;
    double y;
    double z;

    internal xyz(double X, double Y, double Z)
    {
        x = X;
        y = Y;
        z = Z;
    }
}

public class RandomTreeGen : MonoBehaviour
{
    public float MaxCoordinate;
    public int TreeCount;
    public GameObject TreeObj;

    internal List<GameObject> TreeCollection;

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

            GameObject Tree = Instantiate(TreeObj, coords, rot);
        }

        IEnumerable<Triangle> triangulation = delaunay.BowyerWatson(points);
    }

        void GenRandomTreeLayout()
    {
        System.Random rnd = new System.Random();

        for (int ctr = 1; ctr <= TreeCount; ctr++)
        {

            Vector3 coords = new Vector3(Convert.ToSingle(rnd.NextDouble() * MaxCoordinate),
                                         2,
                                         Convert.ToSingle(rnd.NextDouble() * MaxCoordinate));

            Quaternion rot = new Quaternion(0, 0, 0, 0);

            GameObject Tree = Instantiate(TreeObj, coords, rot);
        }
    }
}
