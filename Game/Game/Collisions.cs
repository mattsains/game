using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

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
        public static void DrawLine(SpriteBatch s, Vector2 origin, Vector2 line, bool startBatch = false)
        {
            //TODO: remove from actual game.
            Texture2D pixel = new Texture2D(s.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White });
            // stretch the pixel between the two vectors

            float angle = (float)Math.Atan2(line.Y, line.X);
            float length = line.Length();


            if (startBatch) { s.Begin(); }
            s.Draw(pixel, origin, null, Color.White,
                       angle, Vector2.Zero, new Vector2(length, 1),
                       SpriteEffects.None, 0);

            if (startBatch) { s.End(); }

        }
        /// <summary>
        /// ought to return an escape vector
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 Intersects(List<Vector2> a, List<Vector2> b)
        {
            float LeastDist = float.MaxValue;
            Vector2 leastEdge=new Vector2();
            float temp;
            for (int n = 0; n < a.Count-1; n++)
            {
                
                if ((temp = flatten(a, b, Algebra.Perp(a[n] - a[n + 1]))) <= LeastDist && temp!=-1)
                {
                    LeastDist = temp;
                    leastEdge = a[n] - a[n + 1];
                }
            }
            if ((temp = flatten(a, b, Algebra.Perp(a[a.Count-1] - a[0]))) <= LeastDist && temp!=-1)
            {
                LeastDist = temp;
                leastEdge = a[a.Count-1] - a[0];
            }

            if (LeastDist == float.MaxValue)//no intersect
                return new Vector2();
            else
            {
                leastEdge.Normalize();
                return leastEdge * LeastDist;
            }
        }

        public static float flatten(List<Vector2> a, List<Vector2> b, Vector2 axis)
        {
            float Amin = float.MaxValue;
            float Amax=0;
            float Bmin = float.MaxValue;
            float Bmax=0;
            //project along axis.
            foreach (Vector2 va in a)
            {
                float l = Algebra.project(va, axis).LengthSquared();
                if (l <= Amin) Amin = l;
                if (l >= Amax) Amax = l;
            }
            foreach (Vector2 vb in b)
            {
                float l = Algebra.project(vb, axis).LengthSquared();
                if (l <= Bmin) Bmin = l;
                if (l >= Bmax) Bmax = l;
            }
            if (Bmax < Amin || Bmin > Amax) { return -1; } //no intersection.

            float Min, Max;
            Min = Amin < Bmin ? Amin : Bmin;
            Max = Amax > Bmax ? Amax : Bmax;
            
            return (float)(Math.Sqrt(Max) - Math.Sqrt(Min)); //return the length of intersection.

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
