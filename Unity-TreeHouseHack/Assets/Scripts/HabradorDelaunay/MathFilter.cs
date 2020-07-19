using System;
using System.Collections.Generic;
using UnityEngine;

namespace HabradorDelaunay
{
    internal static class MathFilter
    {
        internal static double CalculateDistance(Vector3 A, Vector3 B)
        {
            return Math.Sqrt(Math.Pow((B.x - A.x), 2) + Math.Pow((B.z - A.z), 2));
        }

        internal static double TriangleArea(Triangle tri)
        {
            Vector3 A = tri.v1.position;
            Vector3 B = tri.v2.position;
            Vector3 C = tri.v3.position;

            return Math.Abs(0.5 * (((B.x - A.x) * (C.z - A.z)) - ((C.x - A.x) * (B.y - A.y))));
        }

        internal static List<double> AngleDegrees(Triangle tri)
        {
            Vector3 A = tri.v1.position;
            Vector3 B = tri.v2.position;
            Vector3 C = tri.v3.position;

            double a = CalculateDistance(B, C);
            double b = CalculateDistance(C, A);
            double c = CalculateDistance(A, B);

            double cos_A_angle = (((Math.Pow(b, 2)) + (Math.Pow(c, 2)) - (Math.Pow(a, 2))) / (2 * b * c));
            double cos_B_angle = (((Math.Pow(c, 2)) + (Math.Pow(a, 2)) - (Math.Pow(b, 2))) / (2 * c * a));
            double cos_C_angle = (((Math.Pow(a, 2)) + (Math.Pow(b, 2)) - (Math.Pow(c, 2))) / (2 * a * b));

            double A_angle = Math.Acos(cos_A_angle) * (180 / Math.PI);
            double B_angle = Math.Acos(cos_B_angle) * (180 / Math.PI);
            double C_angle = Math.Acos(cos_C_angle) * (180 / Math.PI);

            return new List<double> { A_angle, B_angle, C_angle };

        }
    }
}
