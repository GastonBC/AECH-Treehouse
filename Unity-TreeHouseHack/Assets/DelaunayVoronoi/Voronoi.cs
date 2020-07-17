using System.Collections.Generic;

namespace DelaunayVoronoi
{
    public class Voronoi
    {
        public IEnumerable<Edge> GenerateEdgesFromDelaunay(IEnumerable<Triangle> triangulation)
        {
            HashSet<Edge> voronoiEdges = new HashSet<Edge>();
            foreach (Triangle triangle in triangulation)
            {
                foreach (Triangle neighbor in triangle.TrianglesWithSharedEdge)
                {
                    Edge edge = new Edge(triangle.Circumcenter, neighbor.Circumcenter);
                    voronoiEdges.Add(edge);
                }
            }

            return voronoiEdges;
        }
    }
}