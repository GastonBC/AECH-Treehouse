using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HabradorDelaunay;

public class TreeSpawner : MonoBehaviour
{
    public float MaxCoordinate;
    public int TreeCount;
    public GameObject TreePrefab;
    public GameObject DeckObject;
    public GameObject DeckNode;

    public double MinAngle;

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
        System.Random rnd = new System.Random();
        Quaternion rot = new Quaternion(0, 0, 0, 0);

        List<Vector3> points = new List<Vector3>();

        for (int n = 1; n <= TreeCount; n++)
        {
            Vector3 RandomPoint = new Vector3(Convert.ToSingle(rnd.NextDouble() * MaxCoordinate),
                                              0,
                                              Convert.ToSingle(rnd.NextDouble() * MaxCoordinate));

            GameObject newTree = Instantiate(TreePrefab, RandomPoint, rot);
            PineTree PineTreeComp1 = newTree.AddComponent<PineTree>();

            points.Add(RandomPoint);
        }

        List<Triangle> triangulation = Delaunay.TriangulateByFlippingEdges(points);


        foreach (Triangle tri in triangulation) // Triangulation not working
        {
            Vector3 P1 = tri.v1.position;
            Vector3 P2 = tri.v2.position;
            Vector3 P3 = tri.v3.position;

            List<double> TriAngles = MathFilter.AngleDegrees(tri);

            if (TriAngles.TrueForAll(angle => angle >= MinAngle))
            {

                GameObject TreeDeck = Instantiate(DeckObject);
                Treehouse TreehouseComp = TreeDeck.AddComponent<Treehouse>();
                TreeHouseHack.Deck DeckComp = TreeDeck.AddComponent<TreeHouseHack.Deck>();

                GameObject newTreeNode1 = Instantiate(DeckNode, P1, rot);
                GameObject newTreeNode2 = Instantiate(DeckNode, P2, rot);
                GameObject newTreeNode3 = Instantiate(DeckNode, P3, rot);

                List<GameObject> TreesTri = new List<GameObject>();
                TreesTri.Add(newTreeNode1);
                TreesTri.Add(newTreeNode2);
                TreesTri.Add(newTreeNode3);

                TreehouseComp.Trees = TreesTri;
            }
        }
    }
}
