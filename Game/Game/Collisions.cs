using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Game
{
    // Semi-complex collision functions should go here.
    // Because linear algebra is scary. OMG.

    /// <summary>
    /// Stores an edge (as in a line between two points)
    /// Do not confuse with the deprecated Edgez class which is an ugly hack and will be removed
    /// </summary>
    class Edge
    {
        public Edge(Vector2 av, Vector2 bv)
        {
            a = av;
            b = bv;
        }
        public Vector2 a;
        public Vector2 b;
    }
    class Algebra
    {
        /// <summary>
        /// Returns edge of intercept; dot difference 
        /// </summary>
        /// <param name="a">The first Polygon</param>
        /// <param name="b">The Second Polygon</param>
        /// <returns>surface edge iff polygons are intersecting, else (0,0)</returns>
        public static Tuple<Vector2, float> Intersects(List<Vector2> a, List<Vector2> b)
        {
            List<Edge> edges = GetEdges(a);
            Tuple<Edge, float> minInterval = new Tuple<Edge, float>(new Edge(new Vector2(), new Vector2()), float.MaxValue);
            // checks each edge as a possible separating line. 
            // If any regions of the polygons along a separating line does not overlap, the polygons cannot intersect, and the loop terminates
            foreach (Edge e in edges)
            {
                float interval = ComponentIntersects(e, a, b);
                if (interval == 0)
                {
                    return new Tuple<Vector2, float>(new Vector2(), 0);
                }
                else if (interval < minInterval.Item2)
                {
                    minInterval = new Tuple<Edge, float>(e, interval);
                }
            }
            // all of the regions seem to intersect, so the polygons themselves must be intersecting
            return new Tuple<Vector2, float>(minInterval.Item1.a - minInterval.Item1.b, minInterval.Item2);
        }
        public static void DrawLine(SpriteBatch s, Vector2 origin, Vector2 line){
                //TODO: remove from actual game.
                Texture2D pixel = new Texture2D(s.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                pixel.SetData(new[]{Color.White});
                // stretch the pixel between the two vectors
                
                float angle = (float)Math.Atan2(line.Y, line.X);
                float length = line.Length();

                bool DNE = false;
                try { s.Begin(); }
                catch (Exception) { DNE = true; }
                s.Draw(pixel, origin, null, Color.White,
                           angle, Vector2.Zero, new Vector2(length, 1),
                           SpriteEffects.None, 0);
                if (!DNE)
                {
                    s.End();
                }
            }
        /// <summary>
        /// Checks whether two convex polygons overlap with respect to a separating line e
        /// </summary>
        /// <param name="e">the seperating line</param>
        /// <param name="a">first convex polygon</param>
        /// <param name="b">second convex polygon</param>
        /// <returns>If the polygons overlap along the separating axis, the amount that the two shapes overlap on the interval, else 0</returns>
        private static float ComponentIntersects(Edge e, List<Vector2> a, List<Vector2> b)
        {
            // calculate the separating axis vector - 90^ to an edge
            Vector2 axis = Perp(e.b - e.a);

            List<float> acomp = new List<float>();
            List<float> bcomp = new List<float>();
            //Finds the intervals of all the dot products of each polygon.
            // if these intervals overlap, then so do the polygons
            foreach (Vector2 v in a)
            {
                acomp.Add(Vector2.Dot(v, axis));
            }
            foreach (Vector2 v in b)
            {
                bcomp.Add(Vector2.Dot(v, axis));
            }
            if (bcomp.Min() < acomp.Max() && acomp.Min() < bcomp.Max())
            {
                //regions overlap
                return (new List<float>() { acomp.Max(), bcomp.Max() }.Min()) - 
                    (new List<float>() { acomp.Min(), bcomp.Min() }.Max());
            }
            else return 0;
        }
        /// <summary>
        /// Turns a list of points into a list of edges
        /// </summary>
        /// <param name="p">the list of points</param>
        /// <returns>the list of edges</returns>
        public static List<Edge> GetEdges(List<Vector2> p)
        {
            List<Edge> o = new List<Edge>();
            for (byte n = 0; n + 1 < p.Count; n++)
            {
                o.Add(new Edge(p[n], p[n + 1]));
            }
            o.Add(new Edge(p[p.Count - 1], p[0]));
            return o;
        }

        /// <summary>
        /// finds a vector perpendicular (at 90°) to a vector in 2D space
        /// </summary>
        /// <param name="v">a vector</param>
        /// <returns>a perpendicular vector</returns>
        public static Vector2 Perp(Vector2 v)
        {
            //grade 11 analytical geometry
            return new Vector2(v.Y, -v.X);
        }
        /// <summary>
        /// finds the minimum AABB of a polygon. Useful for drawing pre-baked sprites of shapes
        /// </summary>
        /// <param name="s">the polygon</param>
        /// <returns>the polygon's minimum AABB</returns>
        public static Rectangle Span(List<Vector2> s)
        {
            float minx, miny, maxx, maxy;
            minx = miny = float.MaxValue;
            maxx = maxy = float.MinValue;
            foreach (Vector2 v in s)
            {
                maxx = v.X > maxx ? v.X : maxx;
                maxy = v.Y > maxy ? v.Y : maxy;
                minx = v.X < minx ? v.X : minx;
                miny = v.Y < miny ? v.Y : miny;
            }
            return new Rectangle((int)minx, (int)miny, (int)(maxx - minx), (int)(maxy - miny));
        }
        /// <summary>
        /// project a onto b
        /// </summary>
        /// <param name="a">the subject vector</param>
        /// <param name="b">the surface vector</param>
        /// <returns></returns>
        public static Vector2 project(Vector2 a, Vector2 b)
        {
            return (Vector2.Dot(a, b) / b.LengthSquared()) * b;
        }

    }

}
