using System.Text;

namespace MCAsset2Font
{
    public partial class OTF
    {
        public readonly uint sfntVersion = 0x00010000;
        public readonly ushort numTables = 10;
        public ushort searchRange => SearchRange(numTables, 16);
        public ushort entrySelector => EntrySelector(numTables);
        public ushort rangeShift => RangeShift(numTables, 16);
        public HeadTable Head;
        public HHeaTable HHea;
        public MaxPTable MaxP;
        public OS_2Table OS_2;
        public HMtxTable HMtx;
        public CMapTable CMap;
        public LocaTable Loca;
        public GlyfTable Glyf;
        public NameTable Name;
        public PostTable Post;

        public bool bold;
        public bool italic;
        public bool verbose;
        public List<Glyph> glyphs;

        public OTF(bool bold = false, bool italic = false, bool verbose = false)
        {
            this.Head = new HeadTable(bold, italic);
            this.HHea = new HHeaTable();
            this.MaxP = new MaxPTable();
            this.OS_2 = new OS_2Table(bold, italic);
            this.HMtx = new HMtxTable();
            this.CMap = new CMapTable();
            this.Loca = new LocaTable();
            this.Glyf = new GlyfTable();
            this.Name = new NameTable();
            this.Post = new PostTable();
            this.bold = bold;
            this.italic = italic;
            this.verbose = verbose;
            this.glyphs = new List<Glyph>
            {
                new Glyph
                {
                    ch = new Rune(0),
                    advanceWidth = 24,
                    leftSideBearing = 2,
                    contours = new List<Glyph.Contour>
                    {
                        Glyph.Contour.From(2, -4, 2, 28, 22, 28, 22, -4),
                        Glyph.Contour.From(18, 0, 18, 24, 6, 24, 6, 0)
                    }
                } // Hardcoded standard missing glyph
            };
        }
        public void AddSpaceGlyph(Rune ch, ushort advanceWidth) // Does NOT check if the char already exists
        {
            this.glyphs.Add(new Glyph { ch = ch, advanceWidth = advanceWidth });
        }
        public void AddGlyph(Rune ch, ushort advanceWidth, short leftSideBearing, List<Glyph.Contour> contours) // Does NOT check if the char already exists
        {
            if (ch.Value == 0xFFFD) // Replacement character
            {
                this.glyphs[0] = new Glyph { ch = new Rune(0), advanceWidth = advanceWidth, leftSideBearing = leftSideBearing, contours = contours };
            }
            else
            {
                this.glyphs.Add(new Glyph { ch = ch, advanceWidth = advanceWidth, leftSideBearing = leftSideBearing, contours = contours });
            }
        }
        public void SetCreated(DateTime created)
        {
            this.Head.created = created;
        }
        public void SetModified(DateTime modified)
        {
            this.Head.modified = modified;
        }
        public void SetString(NameTable.NameString id, string str)
        {
            this.Name.nameStrings[id] = str;
        }
        public bool TryAddString(NameTable.NameString id, string str)
        {
            return this.Name.nameStrings.TryAdd(id, str);
        }
        public string GetString(NameTable.NameString id)
        {
            return this.Name.nameStrings[id];
        }
        public bool RemoveString(NameTable.NameString id)
        {
            return this.Name.nameStrings.Remove(id);
        }
        public List<byte> Serialize()
        {
            if (this.verbose) Console.WriteLine("[Serialize] UpdateGlyphData");
            this.UpdateGlyphData();

            if (this.verbose) Console.WriteLine("[Serialize] Header");
            Table[] tables = { this.Head, this.HHea, this.MaxP, this.OS_2, this.HMtx, this.CMap, this.Loca, this.Glyf, this.Name, this.Post };
            SortedDictionary<Table, List<byte>> headerTables = new SortedDictionary<Table, List<byte>>();

            int contentOffset = 12 + 16 * tables.Length;
            List<byte> headerBuffer = new List<byte>();
            headerBuffer.AddRange(this.sfntVersion.SerializeU32());
            headerBuffer.AddRange(this.numTables.SerializeU16());
            headerBuffer.AddRange(this.searchRange.SerializeU16());
            headerBuffer.AddRange(this.entrySelector.SerializeU16());
            headerBuffer.AddRange(this.rangeShift.SerializeU16());
            List<byte> contentBuffer = new List<byte>();
            for (int i = 0; i < tables.Length; i++)
            {
                if (this.verbose) Console.WriteLine($"[Serialize] Table {tables[i].Tag.Value}");

                List<byte> currentContent = tables[i].Serialize().ToList();
                int currentContentLength = currentContent.Count;

                currentContent.Pad(); // extend to 4-byte boundary

                headerTables[tables[i]] = new List<byte>();
                headerTables[tables[i]].AddRange(tables[i].Tag.SerializeT32());
                headerTables[tables[i]].AddRange(currentContent.Checksum().SerializeU32());
                headerTables[tables[i]].AddRange((contentOffset + contentBuffer.Count).SerializeU32());
                headerTables[tables[i]].AddRange(currentContentLength.SerializeU32());
                contentBuffer.AddRange(currentContent);
            }
            if (this.verbose) Console.WriteLine("[Serialize] Sort header");
            foreach (KeyValuePair<Table, List<byte>> kvp in headerTables)
            {
                headerBuffer.AddRange(kvp.Value);
            }
            int checksumIndex = headerBuffer.Count + 8;
            headerBuffer.AddRange(contentBuffer);
            uint checksumAdjustment = 0xB1B0AFBAu - headerBuffer.Checksum(); // idk the documentation says this
            headerBuffer.InsertU32(checksumIndex, checksumAdjustment);
            return headerBuffer;
        }
        private void UpdateGlyphData()
        {
            if (this.verbose) Console.WriteLine("[UpdateGlyphData] Sort glyphs");
            this.glyphs.Sort();
            if (this.verbose) Console.WriteLine("[UpdateGlyphData] Update segments");
            this.CMap.UpdateSegments(this.glyphs);
            if (this.verbose) Console.WriteLine("[UpdateGlyphData] Calculate glyph properties");
            this.OS_2.usFirstCharIndex = (ushort)this.CMap.groups[0].startCharCode;
            this.OS_2.usLastCharIndex = 0xFFFF; // Supplementary character values
            this.OS_2.sTypoAscender = this.HHea.ascender;
            this.OS_2.sTypoDescender = this.HHea.descender;
            this.OS_2.sTypoLineGap = this.HHea.lineGap;
            this.OS_2.usWinAscent = (ushort)this.HHea.ascender;
            this.OS_2.usWinDescent = (ushort)-this.HHea.descender;
            this.Head.xMin = short.MaxValue;
            this.Head.yMin = short.MaxValue;
            this.Head.xMax = short.MinValue;
            this.Head.yMax = short.MinValue;
            this.HHea.advanceWidthMax = ushort.MinValue;
            this.HHea.minLeftSideBearing = short.MaxValue;
            this.HHea.minRightSideBearing = short.MaxValue;
            this.HHea.xMaxExtent = short.MinValue;
            this.HHea.numberOfHMetrics = this.MaxP.numGlyphs = (ushort)this.glyphs.Count;
            this.MaxP.maxPoints = ushort.MinValue;
            this.MaxP.maxContours = ushort.MinValue;
            this.HMtx.cache = new List<byte>(new byte[this.glyphs.Count * 4]);
            this.Loca.cache = new List<byte>(new byte[this.glyphs.Count * 4 + 4]);
            this.Glyf.cache = new List<byte>();
            long advanceWidthSum = 0;
            ushort iAdvanceWidth;
            short iLeftSideBearing;
            short iRightSideBearing;
            short iExtent;
            byte[] iAdvanceWidthS;
            byte[] iLeftSideBearingS;
            byte[] iOffsetS;
            int i = 0;
            foreach (Glyph g in this.glyphs)
            {
                iOffsetS = this.Glyf.cache.Count.SerializeU32();
                this.Loca.cache[i * 4] = iOffsetS[0];
                this.Loca.cache[i * 4 + 1] = iOffsetS[1];
                this.Loca.cache[i * 4 + 2] = iOffsetS[2];
                this.Loca.cache[i * 4 + 3] = iOffsetS[3];
                iAdvanceWidth = g.advanceWidth;
                advanceWidthSum += iAdvanceWidth;
                if (iAdvanceWidth > this.HHea.advanceWidthMax) this.HHea.advanceWidthMax = iAdvanceWidth;
                if (g.contours.Count > 0)
                {
                    List<byte> gBytes = g.Serialize(out short iXMin, out short iYMin, out short iXMax, out short iYMax, out ushort iPoints, out ushort iContours).ToList();
                    gBytes.Pad();
                    this.Glyf.cache.AddRange(gBytes);
                    if (iXMin < this.Head.xMin) this.Head.xMin = iXMin;
                    if (iYMin < this.Head.yMin) this.Head.yMin = iYMin;
                    if (iXMax > this.Head.xMax) this.Head.xMax = iXMax;
                    if (iYMax > this.Head.yMax) this.Head.yMax = iYMax;
                    if (iPoints > this.MaxP.maxPoints) this.MaxP.maxPoints = iPoints;
                    if (iContours > this.MaxP.maxContours) this.MaxP.maxContours = iContours;
                    iLeftSideBearing = g.leftSideBearing;
                    iRightSideBearing = (short)(iAdvanceWidth - iLeftSideBearing - iXMax + iXMin);
                    iExtent = (short)(iLeftSideBearing + iXMax - iXMin);
                    if (iLeftSideBearing < this.HHea.minLeftSideBearing) this.HHea.minLeftSideBearing = iLeftSideBearing;
                    if (iRightSideBearing < this.HHea.minRightSideBearing) this.HHea.minRightSideBearing = iRightSideBearing;
                    if (iExtent > this.HHea.xMaxExtent) this.HHea.xMaxExtent = iExtent;
                }
                else
                {
                    iLeftSideBearing = 0;
                }
                iAdvanceWidthS = iAdvanceWidth.SerializeU16();
                iLeftSideBearingS = iLeftSideBearing.SerializeS16();
                this.HMtx.cache[i * 4] = iAdvanceWidthS[0];
                this.HMtx.cache[i * 4 + 1] = iAdvanceWidthS[1];
                this.HMtx.cache[i * 4 + 2] = iLeftSideBearingS[0];
                this.HMtx.cache[i * 4 + 3] = iLeftSideBearingS[1];
                i++;
            }
            this.OS_2.xAvgCharWidth = (short)(advanceWidthSum / this.glyphs.Count);
            iOffsetS = this.Glyf.cache.Count.SerializeU32();
            this.Loca.cache[^4] = iOffsetS[0];
            this.Loca.cache[^3] = iOffsetS[1];
            this.Loca.cache[^2] = iOffsetS[2];
            this.Loca.cache[^1] = iOffsetS[3];
        }
        private static ushort SearchRange(ushort numTables, byte factor)
        {
            return (ushort)(Math.Pow(2, Math.Floor(Math.Log2(numTables))) * factor);
        }
        private static ushort EntrySelector(ushort numTables)
        {
            return (ushort)Math.Floor(Math.Log2(numTables));
        }
        private static ushort RangeShift(ushort numTables, byte factor)
        {
            return (ushort)(numTables * factor - SearchRange(numTables, factor));
        }
        public class Glyph : IComparable<Glyph>
        {
            public readonly ushort instructionLength = 0;
            public Flags flags = Flags.ON_CURVE_POINT;
            // public Flags flags = Flags.ON_CURVE_POINT | Flags.REPEAT_FLAG;
            public ushort advanceWidth;
            public short leftSideBearing = 0;
            public List<Contour> contours = new List<Contour>();
            public Rune ch; // 0 = missing glyph

            public List<byte> Serialize(out short xMin, out short yMin, out short xMax, out short yMax, out ushort numPoints, out ushort numContours)
            {
                xMin = short.MaxValue;
                yMin = short.MaxValue;
                xMax = short.MinValue;
                yMax = short.MinValue;
                short numberOfContours = (short)contours.Count;
                byte[] endPtsOfContoursBuffer = new byte[numberOfContours * 2];
                List<byte> flagsBuffer = new List<byte>(); // TODO: Make use of more flag features
                List<byte> relXBuffer = new List<byte>();
                List<byte> relYBuffer = new List<byte>();
                short prevX = 0, prevY = 0;
                byte[] iEndPointsOfContours;
                ushort i = 0;
                ushort ic;
                for (ic = 0; ic < contours.Count; ic++)
                {
                    foreach (Contour.Point p in this.contours[ic].points)
                    {
                        if (p.absX < xMin) xMin = p.absX;
                        if (p.absY < yMin) yMin = p.absY;
                        if (p.absX > xMax) xMax = p.absX;
                        if (p.absY > yMax) yMax = p.absY;
                        flagsBuffer.Add((byte)this.flags);
                        /*
                        if ((i % 256) == 0)
                        {
                            flagsBuffer.Add((byte)this.flags);
                            flagsBuffer.Add(0xFF); // Change at the end
                        }
                        */
                        byte[] relX = (p.absX - prevX).SerializeS16();
                        byte[] relY = (p.absY - prevY).SerializeS16();
                        relXBuffer.Add(relX[0]);
                        relXBuffer.Add(relX[1]);
                        relYBuffer.Add(relY[0]);
                        relYBuffer.Add(relY[1]);
                        prevX = p.absX;
                        prevY = p.absY;
                        i++;
                    }
                    iEndPointsOfContours = (i - 1).SerializeU16();
                    endPtsOfContoursBuffer[ic * 2] = iEndPointsOfContours[0];
                    endPtsOfContoursBuffer[ic * 2 + 1] = iEndPointsOfContours[1];
                    //flagsBuffer[^1] = (byte)(i % 256);
                }
                numPoints = i;
                numContours = ic;
                List<byte> buffer = new List<byte>();
                buffer.AddRange(numberOfContours.SerializeS16());
                buffer.AddRange(xMin.SerializeS16());
                buffer.AddRange(yMin.SerializeS16());
                buffer.AddRange(xMax.SerializeS16());
                buffer.AddRange(yMax.SerializeS16());
                buffer.AddRange(endPtsOfContoursBuffer);
                buffer.AddRange(this.instructionLength.SerializeU16());
                buffer.AddRange(flagsBuffer);
                buffer.AddRange(relXBuffer);
                buffer.AddRange(relYBuffer);
                return buffer;
            }

            public int CompareTo(Glyph? other)
            {
                if (other == null) return 1;
                return this.ch.Value - other.ch.Value;
            }

            public class Contour
            {
                public List<Point> points = new List<Point>();
                public void Extend(Point p)
                {
                    points.Add(p);
                }
                public void Extend(short absX, short absY)
                {
                    points.Add(new Point { absX = absX, absY = absY });
                }
                public static Contour From(params short[] abs)
                {
                    if ((abs.Length & 1) == 1)
                    {
                        throw new ArgumentException("Arguments are not X-Y pairs");
                    }
                    List<Point> buffer = new List<Point>();
                    for (int i = 0; i < abs.Length; i += 2)
                    {
                        buffer.Add(new Point { absX = abs[i], absY = abs[i + 1] });
                    }
                    return new Contour { points = buffer };
                }
                public struct Point
                {
                    public short absX;
                    public short absY;
                }
            }
            public enum Flags : byte
            {
                ON_CURVE_POINT = 0x01,
                X_SHORT_VECTOR = 0x02,
                Y_SHORT_VECTOR = 0x04,
                REPEAT_FLAG = 0x08,
                X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR = 0x10, //
                Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR = 0x20, // I did not conjure up these names
                OVERLAP_SIMPLE = 0x40
            }
        }
        public abstract class Table : IComparable<Table>
        {
            public abstract Tag Tag { get; }
            public abstract int Index { get; }

            public int CompareTo(Table? other)
            {
                if (other is null) return 1;
                return StringComparer.Ordinal.Compare(this.Tag.Value, other.Tag.Value);
            }

            public abstract List<byte> Serialize();
        }
        public struct Fixed16_16
        {
            public short Upper;
            public ushort Lower;
            public static implicit operator Fixed16_16(double v)
            {
                return Parse(v);
            }
            public static Fixed16_16 Parse(double v)
            {
                if (v < -0x8000 || v >= 0x8000)
                {
                    throw new ArgumentException($"{v} is out of bounds for Fixed16.16");
                }
                else if (v >= 0.0)
                {
                    return new Fixed16_16 { Upper = (short)Math.Floor(v), Lower = (ushort)Math.Floor(v % 1 * 0x10000) };
                }
                else
                {
                    return new Fixed16_16 { Upper = (short)Math.Floor(v), Lower = (ushort)Math.Floor((v % 1 + 1) * 0x10000) };
                }
            }
            public double ToDouble()
            {
                return this.Upper + (double)this.Lower / 0x10000;
            }
            public byte[] Serialize16F16()
            {
                return new byte[4] { (byte)(this.Upper >> 8 & 0xFF), (byte)(this.Upper & 0xFF), (byte)(this.Lower >> 8 & 0xFF), (byte)(this.Lower & 0xFF) };
            }
        }
        public struct Fixed2_14
        {
            public sbyte Upper;
            public ushort Lower;
            public static implicit operator Fixed2_14(double v)
            {
                return Parse(v);
            }
            public static Fixed2_14 Parse(double v)
            {
                if (v < -2 || v >= 2)
                {
                    throw new ArgumentException($"{v} is out of bounds for Fixed2.14");
                }
                else if (v >= 0.0)
                {
                    return new Fixed2_14 { Upper = (sbyte)Math.Floor(v), Lower = (ushort)Math.Floor(v % 1 * 0x4000) };
                }
                else
                {
                    return new Fixed2_14 { Upper = (sbyte)Math.Floor(v), Lower = (ushort)Math.Floor((v % 1 + 1) * 0x4000) };
                }
            }
            public double ToDouble()
            {
                return this.Upper + (double)this.Lower / 0x4000;
            }
            public byte[] Serialize2F14()
            {
                return new byte[] { (byte)(this.Upper << 6 | this.Lower >> 8 & 0x3F), (byte)(this.Lower & 0xFF) };
            }
        }
        public struct Tag
        {
            public string Value;
            public static implicit operator Tag(string v)
            {
                if (v.Length != 4)
                {
                    throw new ArgumentException($"{v} is not the correct length for Tag");
                }
                else
                {
                    return new Tag { Value = v };
                }
            }
            public byte[] SerializeT32()
            {
                return Encoding.ASCII.GetBytes(this.Value);
            }
        }
    }
}