using System;
using System.Collections.Generic;
using System.Linq;

namespace DelaunayVoronoi
{
    public class DelaunayTriangulator
    {
        private double MaxX { get; set; }
        private double MaxY { get; set; }
        private IEnumerable<Triangle> border;

        public IEnumerable<Point> GeneratePoints(int amount, double maxX, double maxY)
        {
            MaxX = maxX;
            MaxY = maxY;

            // TODO make more beautiful
            Point point0 = new Point(0, 0);
            Point point1 = new Point(0, MaxY);
            Point point2 = new Point(MaxX, MaxY);
            Point point3 = new Point(MaxX, 0);
            List<Point> points = new List<Point>() { point0, point1, point2, point3 };
            border = new List<Triangle>() { };



            Random random = new Random();
            for (int i = 0; i < amount - 4; i++)
            {
                double pointX = random.NextDouble() * MaxX;
                double pointY = random.NextDouble() * MaxY;
                points.Add(new Point(pointX, pointY));
            }

            return points;
        }

        public IEnumerable<Triangle> BowyerWatson(IEnumerable<Point> points)
        {
            //var supraTriangle = GenerateSupraTriangle();
            HashSet<Triangle> triangulation = new HashSet<Triangle>(border);

            foreach (Point point in points)
            {
                ISet<Triangle> badTriangles = FindBadTriangles(point, triangulation);
                List<Edge> polygon = FindHoleBoundaries(badTriangles);

                foreach (Triangle triangle in badTriangles)
                {
                    foreach (Point vertex in triangle.Vertices)
                    {
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }
                triangulation.RemoveWhere(o => badTriangles.Contains(o));

                foreach (Edge edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
                {
                    Triangle triangle = new Triangle(point, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }

            //triangulation.RemoveWhere(o => o.Vertices.Any(v => supraTriangle.Vertices.Contains(v)));
            return triangulation;
        }

        private List<Edge> FindHoleBoundaries(ISet<Triangle> badTriangles)
        {
            List<Edge> edges = new List<Edge>();
            foreach (Triangle triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            IEnumerable<IGrouping<Edge, Edge>> grouped = edges.GroupBy(o => o);
            IEnumerable<Edge> boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
            return boundaryEdges.ToList();
        }

        private Triangle GenerateSupraTriangle()
        {
            //   1  -> maxX
            //  / \
            // 2---3
            // |
            // v maxY
            int margin = 500;
            Point point1 = new Point(0.5 * MaxX, -2 * MaxX - margin);
            Point point2 = new Point(-2 * MaxY - margin, 2 * MaxY + margin);
            Point point3 = new Point(2 * MaxX + MaxY + margin, 2 * MaxY + margin);
            return new Triangle(point1, point2, point3);
        }

        private ISet<Triangle> FindBadTriangles(Point point, HashSet<Triangle> triangles)
        {
            IEnumerable<Triangle> badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle>(badTriangles);
        }
    }
}