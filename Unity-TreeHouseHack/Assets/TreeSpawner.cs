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
    public GameObject DeckObject;

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


        
        IEnumerable<Triangle> triangulation = delaunay.BowyerWatson(points);
        

        foreach (Triangle tri in triangulation) // Triangulation not working
        {
            Debug.Log("Works here");
            Point[] three_points = tri.Vertices;

            //GameObject TreeDeck = Instantiate(DeckObject);
            //Treehouse TreehouseComp = TreeDeck.AddComponent<Treehouse>();
            //TreeHouseHack.Deck DeckComp = TreeDeck.AddComponent<TreeHouseHack.Deck>();

            foreach (Point pt in three_points)
            {
                Debug.Log(pt.X);
                Debug.Log(pt.Y);
                Vector3 coords = new Vector3(Convert.ToSingle(pt.X),
                                         2f,  // this is 3rd dimension
                                         Convert.ToSingle(pt.Y));
                Quaternion rot = new Quaternion(0, 0, 0, 0);

                GameObject newTree = Instantiate(TreePrefab, coords, rot);
                PineTree PineTreeComp = newTree.AddComponent<PineTree>();

                //TreehouseComp.Trees.Add(newTree);
            }

        }

    }
}
