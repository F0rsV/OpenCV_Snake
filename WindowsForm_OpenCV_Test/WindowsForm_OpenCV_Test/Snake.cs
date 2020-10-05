using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForm_OpenCV_Test
{
    class Snake
    {
        public List<Point> PointsList { get; set; }
        public Point CurrentPoint { get; set; }
        public double LineLength { get; set; }
        public double MaxLength { get; set; }

        public bool IsFoodPlaced { get; set; }
        public Point FoodPoint { get; set; }

        public Snake()
        {
            PointsList = new List<Point>();
            PointsList.Add(new Point(320, 240));

            LineLength = 0;
            MaxLength = 100;

            IsFoodPlaced = false;
            
        }



        public void DoStep(CircleF[] circles, Image<Bgr, Byte> image)
        {
            if (circles.Length == 0) //NO DETECTION
            {                
                image.DrawPolyline(PointsList.ToArray(), false, new Bgr(25, 255, 45), 3);
                return;
            }


            CurrentPoint = new Point((int)circles[0].Center.X, (int)circles[0].Center.Y) ;


            if (!IsFoodPlaced)
                FoodPoint = GetFoodPoint();


            CheckIntersectionStep();
            CheckFoodEaten();
            

            GrowStep();
            ShrinkStep();

            image.Draw(new CircleF(FoodPoint, 25), new Bgr(255, 20, 230), 0);
            image.DrawPolyline(PointsList.ToArray(), false, new Bgr(245, 230, 15), 10);
        }



        private void GrowStep()
        {
            LineSegment2D tempLine = new LineSegment2D(PointsList[PointsList.Count - 1], CurrentPoint);
            double tempLength = tempLine.Length;

            if (tempLength >= 10)
            {
                PointsList.Add(CurrentPoint);
                LineLength += tempLength;
            }
        }

        private void ShrinkStep()
        {
            if (LineLength >= MaxLength)
            {
                LineSegment2D tempLineDel = new LineSegment2D(PointsList[0], PointsList[1]);
                LineLength -= tempLineDel.Length;
                PointsList.RemoveAt(0);

            }
        }

        private void CheckIntersectionStep()
        {
            bool lines_intersect = false;
            bool segments_intersect = false;
            PointF intersection = new PointF();
            PointF close_p1 = new PointF();
            PointF close_p2 = new PointF();

            for (int i = 1; i < PointsList.Count - 4; i++)
            {
                FindIntersection(CurrentPoint, PointsList[PointsList.Count - 1], PointsList[i - 1], PointsList[i],
                    out lines_intersect, out segments_intersect, out intersection, out close_p1, out close_p2);

                if (segments_intersect)
                {
                    double segmentLength = 0;
                    for (int j = 1; j < i + 1; j++)
                    {
                        LineSegment2D lineSegment = new LineSegment2D(PointsList[j-1], PointsList[j]);
                        segmentLength += lineSegment.Length;
                    }

                    LineLength = segmentLength;
                    MaxLength = segmentLength;

                    PointsList.RemoveRange(0, i);  

                    break;
                }
            }
        }


        private Point GetFoodPoint()
        {
            Random r = new Random();
            Point pointFood = new Point();

            //DO CHEKING FOR SNAKE

            pointFood.X = r.Next(40, 1250);  // CONSTANT
            pointFood.Y = r.Next(40, 690);

            IsFoodPlaced = true;

            return pointFood;
        }

        private void CheckFoodEaten()
        {

            if (IsLineIntersectsCircle(FoodPoint.X, FoodPoint.Y, 25, PointsList[PointsList.Count - 1],
                CurrentPoint))
            {
                IsFoodPlaced = false;
                MaxLength += 100;
            }


        }


        private static void FindIntersection(
            PointF p1, PointF p2, PointF p3, PointF p4,
            out bool lines_intersect, out bool segments_intersect,
            out PointF intersection,
            out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }



        private bool IsLineIntersectsCircle(float cx, float cy, float radius,
                                        PointF point1, PointF point2)
        {
            float dx, dy, A, B, C, det;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) + (point1.Y - cy) * (point1.Y - cy) - radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
               
                return false;
            }
            else
            {
                double t = (-B - Math.Sqrt(det)) / (2 * A);
                double t2 = (-B + Math.Sqrt(det)) / (2 * A);
                bool match = false;

                if (0.0 <= t && t <= 1.0)
                {
                    match = true;
                }
                if (0.0 <= t2 && t2 <= 1.0)
                {
                    match = true;
                }
                return match;
            }
        }

    }
}
