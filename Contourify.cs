using IS = SixLabors.ImageSharp;

namespace MCAsset2Font
{
    internal class Contourify
    {
        private readonly double mxA;
        private readonly double mxB;
        private readonly double mxC;
        private readonly double mxD;
        private readonly double mxE;
        private readonly double mxF;

        private short xMin;
        private short xMax;
        private IS.Image<IS.PixelFormats.A8>? image;
        private List<IS.Point>? fillQueue;
        private List<IS.Point>? edgeQueue;
        private List<IS.Point>? walkQueue;
        private List<OTF.Glyph.Contour>? buffer;
        public Contourify(double mxA, double mxB, double mxC, double mxD, double mxE, double mxF)
        {
            this.mxA = mxA;
            this.mxB = mxB;
            this.mxC = mxC;
            this.mxD = mxD;
            this.mxE = mxE;
            this.mxF = mxF;
        }
        public List<OTF.Glyph.Contour> Bitmap2Contours(IS.Image<IS.PixelFormats.A8> image, out short xMin, out short xMax)
        {
            this.buffer = new List<OTF.Glyph.Contour>();
            this.xMin = short.MaxValue;
            this.xMax = short.MinValue;
            this.image = image;
            this.fillQueue = new List<IS.Point>(this.image.Height * this.image.Width);
            for (int x = 0; x < this.image.Width; x++) for (int y = 0; y < this.image.Height; y++) this.fillQueue.Add(new IS.Point(x, y));
            this.edgeQueue = new List<IS.Point>();
            this.walkQueue = new List<IS.Point>();
            // Reduce every directly connected area to one point
            while (this.fillQueue.Count > 0)
            {
                IS.Point q = this.fillQueue[0];
                if (Threshold(this.image, q))
                {
                    this.FillBlack(q);
                }
                else
                {
                    this.FillHole(q);
                }
                this.walkQueue.Add(q);
            }
            // Remove every hole point whose area touches the edge
            while (this.edgeQueue.Count > 0)
            {
                IS.Point q = this.edgeQueue[0];
                if (Threshold(this.image, q))
                {
                    this.edgeQueue.Remove(q);
                }
                else
                {
                    HashSet<IS.Point> area = new HashSet<IS.Point>();
                    if (this.EdgeHole(q, ref area))
                    {
                        foreach (IS.Point p in area)
                        {
                            this.walkQueue.Remove(p);
                        }
                    }
                }
            }
            // Trace the contour starting from every area point
            while (this.walkQueue.Count > 0)
            {
                IS.Point q = this.walkQueue[0];
                if (Threshold(this.image, q))
                {
                    this.WalkBlack(q);
                }
                else
                {
                    this.WalkHole(q);
                }
            }
            xMin = this.xMin;
            xMax = this.xMax;
            return buffer;
        }
        public short XMin(IS.Image<IS.PixelFormats.A8> image)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = image.Height - 1; y >= 0; y--)
                {
                    if (Threshold(image, new IS.Point(x, y)))
                    {
                        return this.Bm2EmX(x - 0.5, y + 0.5);
                    }
                }
            }
            return this.Bm2EmX(image.Width + 0.5, -0.5);
        }
        public short XMax(IS.Image<IS.PixelFormats.A8> image)
        {
            for (int x = image.Width - 1; x >= 0; x--)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (Threshold(image, new IS.Point(x, y)))
                    {
                        return this.Bm2EmX(x + 0.5, y - 0.5);
                    }
                }
            }
            return this.Bm2EmX(-0.5, image.Height - 0.5);
        }
        private void FillBlack(IS.Point point) // Dequeue everything in a shape (recursive)
        {
            this.fillQueue!.Remove(point);
            IS.Point right = WalkDir.Right.Apply(point);
            IS.Point down = WalkDir.Down.Apply(point);
            IS.Point left = WalkDir.Left.Apply(point);
            IS.Point up = WalkDir.Up.Apply(point);
            if (this.fillQueue.Contains(right) && this.BlackAt(right)) FillBlack(right);
            if (this.fillQueue.Contains(down) && this.BlackAt(down)) FillBlack(down);
            if (this.fillQueue.Contains(left) && this.BlackAt(left)) FillBlack(left);
            if (this.fillQueue.Contains(up) && this.BlackAt(up)) FillBlack(up);
        }
        private void FillHole(IS.Point point) // Dequeue everything in a hole (recursive)
        {
            this.fillQueue!.Remove(point);
            this.edgeQueue!.Add(point);
            IS.Point right = WalkDir.Right.Apply(point);
            IS.Point down = WalkDir.Down.Apply(point);
            IS.Point left = WalkDir.Left.Apply(point);
            IS.Point up = WalkDir.Up.Apply(point);
            if (this.fillQueue.Contains(right) && this.HoleAt(right)) FillHole(right);
            if (this.fillQueue.Contains(down) && this.HoleAt(down)) FillHole(down);
            if (this.fillQueue.Contains(left) && this.HoleAt(left)) FillHole(left);
            if (this.fillQueue.Contains(up) && this.HoleAt(up)) FillHole(up);
        }
        private bool EdgeHole(IS.Point point, ref HashSet<IS.Point> area) // Remove everything in a hole that hits the edge (recursive)
        {
            this.edgeQueue!.Remove(point);
            area.Add(point);
            IS.Point right = WalkDir.Right.Apply(point);
            IS.Point rightDown = WalkDir.Down.Apply(right);
            IS.Point down = WalkDir.Down.Apply(point);
            IS.Point downLeft = WalkDir.Left.Apply(down);
            IS.Point left = WalkDir.Left.Apply(point);
            IS.Point leftUp = WalkDir.Up.Apply(left);
            IS.Point up = WalkDir.Up.Apply(point);
            IS.Point upRight = WalkDir.Right.Apply(up);
            return (this.edgeQueue!.Contains(right) && this.HoleAt(right) && this.EdgeHole(right, ref area)) |
                (this.edgeQueue!.Contains(rightDown) && this.HoleAt(rightDown) && this.EdgeHole(rightDown, ref area)) |
                (this.edgeQueue!.Contains(down) && this.HoleAt(down) && this.EdgeHole(down, ref area)) |
                (this.edgeQueue!.Contains(downLeft) && this.HoleAt(downLeft) && this.EdgeHole(downLeft, ref area)) |
                (this.edgeQueue!.Contains(left) && this.HoleAt(left) && this.EdgeHole(left, ref area)) |
                (this.edgeQueue!.Contains(leftUp) && this.HoleAt(leftUp) && this.EdgeHole(leftUp, ref area)) |
                (this.edgeQueue!.Contains(up) && this.HoleAt(up) && this.EdgeHole(up, ref area)) |
                (this.edgeQueue!.Contains(upRight) && this.HoleAt(upRight) && this.EdgeHole(upRight, ref area)) ||
                point.X == 0 || point.X == this.image!.Width - 1 || point.Y == 0 || point.Y == this.image!.Height - 1;
        }
        private void WalkBlack(IS.Point startPoint) // Enclose the shape (iterative)
        {
            OTF.Glyph.Contour contour = new OTF.Glyph.Contour();
            WalkDir dir = WalkDir.Right;
            IS.Point point = startPoint;
            do
            {
                this.walkQueue!.Remove(point);
                if (this.BlackAt(dir.TurnLeft().Apply(point)))
                {
                    dir = dir.TurnLeft();
                    contour.Extend(this.BlackBm2Em(point, dir, ref this.xMin, ref this.xMax));
                    point = dir.Apply(point);
                }
                else if (this.BlackAt(dir.Apply(point)))
                {
                    point = dir.Apply(point);
                }
                else
                {
                    contour.Extend(this.BlackBm2Em(point, dir, ref this.xMin, ref this.xMax));
                    dir = dir.TurnRight();
                }
            }
            while (dir != WalkDir.Right || point != startPoint);
            this.buffer!.Add(contour);
        }
        private void WalkHole(IS.Point startPoint) // Enclose the hole (iterative)
        {
            OTF.Glyph.Contour contour = new OTF.Glyph.Contour();
            WalkDir dir = WalkDir.Down;
            IS.Point point = startPoint;
            do
            {
                this.walkQueue!.Remove(point);
                if (this.HoleAt(dir.TurnRight().Apply(point)))
                {
                    dir = dir.TurnRight();
                    contour.Extend(this.HoleBm2Em(point, dir));
                    point = dir.Apply(point);
                }
                else if (this.HoleAt(dir.Apply(point)))
                {
                    point = dir.Apply(point);
                }
                else
                {
                    contour.Extend(this.HoleBm2Em(point, dir));
                    dir = dir.TurnLeft();
                }
            }
            while (dir != WalkDir.Down || point != startPoint);
            this.buffer!.Add(contour);
        }
        private bool BlackAt(IS.Point p)
        {
            return this.image!.Bounds.Contains(p) && Threshold(this.image, p);
        }
        private bool HoleAt(IS.Point p)
        {
            return this.image!.Bounds.Contains(p) && !Threshold(this.image, p);
        }
        private static bool Threshold(in IS.Image<IS.PixelFormats.A8> image, IS.Point p)
        {
            return image[p.X, p.Y].PackedValue >= 0x80;
        }
        private OTF.Glyph.Contour.Point BlackBm2Em(IS.Point p, WalkDir dir, ref short xMin, ref short xMax) // Convert black position to glyph space
        {
            double bmX = p.X + dir.x * 0.5 + dir.y * 0.5;
            double bmY = p.Y - dir.x * 0.5 + dir.y * 0.5;
            short emX = this.Bm2EmX(bmX, bmY);
            short emY = this.Bm2EmY(bmX, bmY);
            if (emX < xMin) xMin = emX;
            if (emX > xMax) xMax = emX;
            return new OTF.Glyph.Contour.Point { absX = emX, absY = emY };
        }
        private OTF.Glyph.Contour.Point HoleBm2Em(IS.Point p, WalkDir dir) // Convert hole position to glyph space
        {
            double bmX = p.X + dir.x * 0.5 - dir.y * 0.5;
            double bmY = p.Y + dir.x * 0.5 + dir.y * 0.5;
            return new OTF.Glyph.Contour.Point { absX = this.Bm2EmX(bmX, bmY), absY = this.Bm2EmY(bmX, bmY) };
        }
        private short Bm2EmX(double x, double y)
        {
            return (short)Math.Round(x * this.mxA + y * this.mxB + this.mxC, MidpointRounding.ToPositiveInfinity);
        }
        private short Bm2EmY(double x, double y)
        {
            return (short)Math.Round(x * this.mxD + y * this.mxE + this.mxF, MidpointRounding.ToPositiveInfinity);
        }
        public struct WalkDir
        {
            public static readonly WalkDir Right = new(0, 1, 0), Down = new(1, 0, 1), Left = new(2, -1, 0), Up = new(3, 0, -1);
            private static readonly WalkDir[] turn = new WalkDir[] { Right, Down, Left, Up };
            private readonly byte index;
            public readonly int x;
            public readonly int y;
            private WalkDir(byte id, int x, int y)
            {
                this.index = id;
                this.x = x;
                this.y = y;
            }
            public WalkDir TurnRight()
            {
                return turn[(this.index + 1) % 4];
            }
            public WalkDir TurnLeft()
            {
                return turn[(this.index + 3) % 4];
            }
            public IS.Point Apply(IS.Point p) => new IS.Point(p.X + this.x, p.Y + this.y);
            public override bool Equals(object? o)
            {
                return o is WalkDir w && this.index == w.index;
            }
            public override int GetHashCode()
            {
                return this.index.GetHashCode();
            }
            public static bool operator ==(WalkDir left, WalkDir right)
            {
                return left.index == right.index;
            }
            public static bool operator !=(WalkDir left, WalkDir right)
            {
                return left.index != right.index;
            }
        }
    }
}
