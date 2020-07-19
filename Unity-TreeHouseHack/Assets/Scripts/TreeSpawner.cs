using HabradorDelaunay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreehouseHack
{
    public class TreeSpawner : MonoBehaviour
    {
        public float MaxCoordinate;
        public int TreeCount;
        public AreaRange RelativeArea;
        public GameObject TreePrefab;
        public GameObject DeckObject;
        public GameObject DeckNode;

        public double MinAngle;

        private double[] check_range;

        // Start is called before the first frame update
        void Start()
        {
            GenDelaunayTreeDecks();
        }


        private void GenDelaunayTreeDecks()
        {
            System.Random rnd = new System.Random();
            Quaternion rot = new Quaternion(0, 0, 0, 0);

            List<Vector3> points = new List<Vector3>();

            for (int n = 1; n <= TreeCount; n++)
            {
                Vector3 RandomPoint = new Vector3(Convert.ToSingle(rnd.NextDouble() * MaxCoordinate),
                                                  11,
                                                  Convert.ToSingle(rnd.NextDouble() * MaxCoordinate));

                GameObject newTree = Instantiate(TreePrefab, RandomPoint, rot);

                points.Add(RandomPoint);
            }

            // Magic and beautiful Delaunay triangulation
            List<Triangle> triangulation = Delaunay.TriangulateByFlippingEdges(points);

            #region Area filter setup (small, medium, big, null, surprise)

            List<double> areas = new List<double>();


            // Here we get the areas for the triangles that PASS the angle filter
            // That way, the min area wont be 0.02 for instance
            // We check those areas against the range later
            foreach (Triangle tri in triangulation)
            {
                List<double> TriAngles = MathFilter.AngleDegrees(tri);

                if (TriAngles.TrueForAll(angle => angle >= MinAngle))
                {
                    areas.Add(MathFilter.TriangleArea(tri));
                }
            }

            double area_range = areas.Max() - areas.Min();

            double min_area = areas.Min();
            double max_area = areas.Max();
            double low_mid = min_area + (area_range / 3);
            double high_mid = min_area + ((area_range / 3) * 2);

            double[] no_range = new double[] { min_area, max_area };

            double[] low_range = new double[] { min_area, low_mid };
            double[] mid_range = new double[] { low_mid, high_mid };
            double[] high_range = new double[] { high_mid, max_area };

            switch (RelativeArea)
            {
                case AreaRange.Low:
                    check_range = low_range;
                    break;
                case AreaRange.Mid:
                    check_range = mid_range;
                    break;
                case AreaRange.High:
                    check_range = high_range;
                    break;
                case AreaRange.Surprise:
                    check_range = no_range; // Not implemented yet
                    break;
                case AreaRange.None:
                default:
                    check_range = no_range;
                    break;

            }

            #endregion


            foreach (Triangle tri in triangulation)
            {
                double area = MathFilter.TriangleArea(tri);

                // Area check
                if (check_range[0] <= area && area <= check_range[1])
                {

                    List<double> TriAngles = MathFilter.AngleDegrees(tri);

                    // Angle check
                    if (TriAngles.TrueForAll(angle => angle >= MinAngle))
                    {
                        Vector3 P1 = tri.v1.position;
                        Vector3 P2 = tri.v2.position;
                        Vector3 P3 = tri.v3.position;

                        GameObject TreeDeck = Instantiate(DeckObject);
                        Treehouse TreehouseComp = TreeDeck.AddComponent<Treehouse>();
                        TreeDeck.AddComponent<TreehouseHack.Deck>();

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
    }
}