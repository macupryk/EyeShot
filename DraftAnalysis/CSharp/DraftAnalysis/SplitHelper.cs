using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Geometry;
using System.Drawing;
using devDept.Eyeshot.Entities;

namespace WpfApplication1
{
    public class SplitHelper
    {
        readonly double _angularTol = Utility.DegToRad(7);
        readonly double _smoothingAngle = Utility.DegToRad(10);     // used to determine the final sides of the draft.
        readonly Model _model1;
        readonly Block _blue, _red, _yellow;
        public Vector3D Direction;
        public int ArrowIndex;

        public SplitHelper(Model model1)
        {
            _blue = new Block("BlueBlock");
            _red = new Block("RedBlock");
            _yellow = new Block("yellowBlock");
            Direction = Vector3D.AxisY * -1;
            ArrowIndex = -1;
            _model1 = model1;
        }

        /// <summary>
        /// Splits original Entity in three sections (red, blue, yellow) using direction vector.
        /// </summary>
        public void QuickSplit(Mesh originalEntity, Vector3D direction)
        {
            int redT = 0;
            int blueT = 0;
            int yellowT = 0;

            Mesh redSection = (Mesh)originalEntity.Clone();
            Mesh blueSection = (Mesh)originalEntity.Clone();
            Mesh yellowSection = (Mesh)originalEntity.Clone();
            blueSection.Visible = true;
            redSection.Visible = true;
            yellowSection.Visible = true;

            int[] redPoints = new int[originalEntity.Vertices.Length];
            int[] yellowPoints = new int[originalEntity.Vertices.Length];
            int[] bluePoints = new int[originalEntity.Vertices.Length];

            // gets graph of originalEntity's edges
            LinkedList<SharedEdge>[] sell;
            int res = Utility.GetEdgesWithoutDuplicates(originalEntity.Triangles, originalEntity.Vertices.Count(), out sell);

            //convert original IndexTriangles to MyIndexTriangle
            MyIndexTriangle[] originalT = new MyIndexTriangle[originalEntity.Triangles.Length];
            for (int i = 0; i < originalEntity.Triangles.Length; i++)
            {
                originalT[i] = new MyIndexTriangle(originalEntity.Triangles[i].V1, originalEntity.Triangles[i].V2, originalEntity.Triangles[i].V3, false, 0, originalEntity.Vertices);
            }

            // gets a first list of triangles
            SplitTrianglesByNormal(originalEntity, direction, originalT, redSection, yellowSection, blueSection, redPoints, yellowPoints, bluePoints, 
                ref redT, ref yellowT, ref blueT);
            
            // checks yellow triangles
            ReassingYellowTriangles(originalEntity, direction, originalT, sell, redSection, yellowSection, blueSection, redPoints, yellowPoints, bluePoints, 
                ref redT, ref yellowT, ref blueT);

            // updates triangle section arrays
            IndexTriangle[] blue = new IndexTriangle[blueT];
            IndexTriangle[] red = new IndexTriangle[redT];
            IndexTriangle[] yellow = new IndexTriangle[yellowT];
            int redDestCount = 0;
            int blueDestCount = 0;
            int yellowDestCount = 0;
            for (int i = 0; i < originalEntity.Triangles.Length; i++)
            {
                if (redSection.Triangles[i] != null)
                {
                    red[redDestCount] = (IndexTriangle)redSection.Triangles[i].Clone();
                    redDestCount++;
                }
                if (blueSection.Triangles[i] != null)
                {
                    blue[blueDestCount] = (IndexTriangle)blueSection.Triangles[i].Clone();
                    blueDestCount++;
                }
                if (yellowSection.Triangles[i] != null)
                {
                    yellow[yellowDestCount] = (IndexTriangle)yellowSection.Triangles[i].Clone();
                    yellowDestCount++;
                }
            }
            redSection.Triangles = red;
            blueSection.Triangles = blue;
            yellowSection.Triangles = yellow;

            //Deletes and reorders Vertices lists
            yellowSection = DeleteUnusedVertices(yellowSection);
            redSection = DeleteUnusedVertices(redSection);
            blueSection = DeleteUnusedVertices(blueSection);

            SetBlockDefinition(blueSection, redSection, yellowSection);
            DrawNormalDirection(Point3D.Origin, originalEntity.BoxSize.Diagonal);

            _model1.Entities.Regen();
            _model1.Invalidate();
        }

        /// <summary>
        /// Splits the triangles and Vertices in red/yellow/blue sections considering angularTol.
        /// </summary>
        private void SplitTrianglesByNormal(Mesh originalEntity, Vector3D direction, MyIndexTriangle[] originalT,
            Mesh redSection, Mesh yellowSection, Mesh blueSection, int[] redPoints, int[] yellowPoints, int[] bluePoints,
            ref int redT, ref int yellowT, ref int blueT)
        {
            for (int i = 0; i < originalT.Length; i++)
            {
                MyIndexTriangle it = originalT[i];
                Triangle t = new Triangle(originalEntity.Vertices[it.V1], originalEntity.Vertices[it.V2],
                    originalEntity.Vertices[it.V3]);
                t.Regen(0.1);
                double angle = Vector3D.AngleBetween(direction, t.Normal);

                // red section
                if (Math.PI / 2 - Math.Abs(angle) > _angularTol)
                {
                    originalT[i].Found = true;
                    originalT[i].Visited = true;
                    //sets to yellow group
                    originalT[i].Group = 1;
                    // if is yellow isn't blue/red
                    blueSection.Triangles[i] = null;
                    yellowSection.Triangles[i] = null;
                    //found a new red triangle
                    redT++;

                    redPoints[it.V3] = redPoints[it.V2] = redPoints[it.V1] = 1;
                }
                // yellow section
                else if (Math.Abs(angle - Math.PI / 2) <= _angularTol)
                {
                    //sets to yellow group
                    originalT[i].Group = 0;
                    // if is yellow isn't blue/red
                    blueSection.Triangles[i] = null;
                    redSection.Triangles[i] = null;
                    //found a new yellow triangle
                    yellowT++;

                    yellowPoints[it.V3] = yellowPoints[it.V2] = yellowPoints[it.V1] = 1;
                }
                // blue section
                else
                {
                    originalT[i].Found = true;
                    //originalT[i].Visited = true;  
                    //sets to blue group
                    originalT[i].Group = -1;
                    // if is blue isn't red/yellow
                    redSection.Triangles[i] = null;
                    yellowSection.Triangles[i] = null;
                    //found a new blue triangle
                    blueT++;

                    bluePoints[it.V3] = bluePoints[it.V2] = bluePoints[it.V1] = 1;
                }
            }
        }

        /// <summary>
        /// Splits yellow triangles in red/blue section considering the smoothingAngle.
        /// </summary>
        private void ReassingYellowTriangles(Mesh originalEntity, Vector3D direction, MyIndexTriangle[] originalT, LinkedList<SharedEdge>[] sell, Mesh redSection, Mesh yellowSection, Mesh blueSection, 
            int[] redPoints, int[] yellowPoints, int[] bluePoints, ref int redT, ref int yellowT, ref int blueT)
        {
            for (int i = 0; i < originalT.Length; i++)
            {
                if (yellowSection.Triangles[i] != null)
                {
                    //if is a perfect vertical triangle from direction must be yellow
                    double angle = Vector3D.AngleBetween(direction, originalT[i].Normal);
                    if (angle != Math.PI/2)
                    {
                        IndexTriangle it = yellowSection.Triangles[i];
                        // gets group of triangle i considering his SharedEdges
                        var result = 0;
                        originalT[i].Group = result = GetFinalDraftTriangles(originalT, sell, i, originalEntity.Vertices);

                        if (result > 0)
                        {
                            //Triangle move from yellow group to red group
                            redT++;
                            yellowT--;
                            redSection.Triangles[i] = (IndexTriangle) it.Clone();
                            blueSection.Triangles[i] = null;
                            yellowSection.Triangles[i] = null;

                            redPoints[it.V3] = redPoints[it.V2] = redPoints[it.V1] = 1;

                            yellowPoints[it.V1] = yellowPoints[it.V2] = yellowPoints[it.V3] = 0;
                        }
                        else if (result < 0)
                        {
                            //Triangle move from yellow group to blue group
                            blueT++;
                            yellowT--;
                            blueSection.Triangles[i] = (IndexTriangle) it.Clone();
                            redSection.Triangles[i] = null;
                            yellowSection.Triangles[i] = null;

                            bluePoints[it.V3] = bluePoints[it.V2] = bluePoints[it.V1] = 1;

                            yellowPoints[it.V1] = yellowPoints[it.V2] = yellowPoints[it.V3] = 0;
                        }

                        // Checks if some Triangles was near to this(ReVisit = true) and sets to the same group
                        for (int j = 0; j < originalT.Length; j++)
                        {
                            if (originalT[j].Group == 0 && originalT[j].ReVisit)
                            {
                                if (originalT[i].Group != 0)
                                {
                                    originalT[j].Group = originalT[i].Group;
                                    originalT[j].ReVisit = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the correct group of triangle indexT considering neigthbor triangles groups of indexV vertex that fall in smoothingAngle.
        /// </summary>
        private int CheckTriangles(MyIndexTriangle[] its, int indexT, LinkedList<SharedEdge>[] sel, int indexV, Point3D[] vertices)
        {
            LinkedListNode<SharedEdge> se = sel[indexV].First;

            while (se != null)
            {
                if (se.Value.Dad != indexT)
                {
                    double angle2 = Vector3D.AngleBetween(its[indexT].Normal, its[se.Value.Dad].Normal);
                    if (Math.Abs(angle2) < _smoothingAngle)
                    {
                        // if Dad triangle is not yellow, indexT must has dad's group
                        if (its[se.Value.Dad].Group != 0)
                            return its[se.Value.Dad].Group;

                        if (!its[se.Value.Dad].Found && !its[se.Value.Dad].Visited)
                        {
                            its[se.Value.Dad].Group = GetFinalDraftTriangles(its, sel, se.Value.Dad, vertices);
                            if (its[se.Value.Dad].Group != 0) return its[se.Value.Dad].Group;
                        }
                        // if Dad is visiting, current Triangle need to be revisit after Dad 
                        else if (its[se.Value.Dad].Found && !its[se.Value.Dad].Visited)
                            its[indexT].ReVisit = true;
                    }
                }
                if (se.Value.Mum != indexT)
                {
                    double angle2 = Vector3D.AngleBetween(its[indexT].Normal, its[se.Value.Mum].Normal);
                    if (Math.Abs(angle2) < _smoothingAngle)
                    {
                        // if mum triangle is not yellow, it must be mum's group
                        if (its[se.Value.Mum].Group != 0)
                            return its[se.Value.Mum].Group;

                        if (!its[se.Value.Mum].Found && !its[se.Value.Mum].Visited)
                        {
                            its[se.Value.Mum].Group = GetFinalDraftTriangles(its, sel, se.Value.Mum, vertices);
                            if (its[se.Value.Mum].Group != 0) return its[se.Value.Mum].Group;
                        }
                        // if Mum's visiting, current Triangle need to be revisit after Mum 
                        else if ((its[se.Value.Mum].Found && !its[se.Value.Mum].Visited))
                            its[indexT].ReVisit = true;
                    }
                }
                se = se.Next;
            }

            // remains in yellow group
            return 0;
        }

        /// <summary>
        /// Returns the correct group of indexT triangle considering his neighbours triangles groups.
        /// </summary>
        private int GetFinalDraftTriangles(MyIndexTriangle[] its, LinkedList<SharedEdge>[] sel, int indexT, Point3D[] vertices)
        {
            // if visited its[indexT].Group already setted.
            if (its[indexT].Found) return its[indexT].Group;
            its[indexT].Found = true;

            //check neightbors Triangles of first vertex
            int res = CheckTriangles(its, indexT, sel, its[indexT].V1, vertices);
            if (res != 0) return res;

            //check neightbors Triangles of second vertex
            res = CheckTriangles(its, indexT, sel, its[indexT].V2, vertices);
            if (res != 0) return res;

            //check neightbors Triangles of third vertex
            res = CheckTriangles(its, indexT, sel, its[indexT].V3, vertices);

            its[indexT].Visited = true;
            return res;
        }

        /// <summary>
        /// Returns a Mesh with unused vertices deleted and vertices references reordered in Triangles list of Mesh m.
        /// </summary>
        protected Mesh DeleteUnusedVertices(Mesh m)
        {
            int[] newV = new int[m.Vertices.Length];
            int count = 1;

            foreach (IndexTriangle it in m.Triangles)
            {
                if (newV[it.V1] == 0)
                {
                    newV[it.V1] = count;
                    count++;
                }
                it.V1 = newV[it.V1] - 1;

                if (newV[it.V2] == 0)
                {
                    newV[it.V2] = count;
                    count++;
                }
                it.V2 = newV[it.V2] - 1;

                if (newV[it.V3] == 0)
                {
                    newV[it.V3] = count;
                    count++;
                }
                it.V3 = newV[it.V3] - 1;
            }

            Point3D[] finalV = new Point3D[count - 1];
            for (int i = 0; i < m.Vertices.Length; i++)
            {
                if (newV[i] != 0)
                {
                    finalV[newV[i] - 1] = m.Vertices[i];
                }
            }
            m.Vertices = finalV;
            return m;
        }

        /// <summary>
        /// Add normal's direction in Model like an arrow.
        /// </summary>
        public void DrawNormalDirection(Point3D startPt, double size)
        {
            // creates arrow that shows split direction choosen
            Mesh newArrow = Mesh.CreateArrow(0.1, 5, 0.5, 1, 10, Mesh.natureType.Plain);
            newArrow.Rotate(-Direction.AngleFromXY, Vector3D.AxisY);
            newArrow.Rotate(Direction.AngleInXY, Vector3D.AxisZ);

            newArrow.Scale(size / 50, size / 50, size / 50);
            newArrow.Translate(new Vector3D(startPt.X, startPt.Y, startPt.Z));
            newArrow.Color = Color.Magenta;
            newArrow.ColorMethod = colorMethodType.byEntity;

            if (ArrowIndex != -1)
            {
                // updates previous arrow
                _model1.Entities[ArrowIndex] = newArrow;
                //_model1.Entities[arrowIndex].Color = Color.Magenta;
            }
            else
            {
                _model1.Entities.Add(newArrow, Color.Magenta);
                ArrowIndex = _model1.Entities.IndexOf(newArrow);
            }

            _model1.Entities.ClearSelection();
            _model1.Entities.Regen();
            _model1.Invalidate();
        }

        /// <summary>
        /// Add groups red, blue, yellow in Model using BlockReference.
        /// </summary>
        private void SetBlockDefinition(Mesh blueMesh, Mesh redMesh, Mesh yellowMesh)
        {
            // removes old splitted sections
            if (_model1.Blocks.Count > 0)
            {
                _model1.Blocks.Remove("BlueBlock");
                _model1.Blocks.Remove("RedBlock");
                _model1.Blocks.Remove("yellowBlock");

                _blue.Entities.RemoveAt(0);
                _red.Entities.RemoveAt(0);
                _yellow.Entities.RemoveAt(0);
            }

            // creates block definitions of new sections
            blueMesh.Color = Color.SkyBlue;
            blueMesh.Selectable = false;
            _blue.Entities.Add(blueMesh);

            redMesh.Color = Color.Salmon;
            redMesh.Selectable = false;
            _red.Entities.Add(redMesh);

            yellowMesh.Color = Color.Wheat;
            yellowMesh.Selectable = false;
            _yellow.Entities.Add(yellowMesh);

            _model1.Blocks.Add(_blue);
            _model1.Blocks.Add(_red);
            _model1.Blocks.Add(_yellow);

            // create block references and adds them to model 
            _model1.Entities.Add(new BlockReference(0, 0, 0, "BlueBlock", 1, 1, 1, 0));
            _model1.Entities.Add(new BlockReference(0, 0, 0, "RedBlock", 1, 1, 1, 0));
            _model1.Entities.Add(new BlockReference(0, 0, 0, "yellowBlock", 1, 1, 1, 0));
        }

        /// <summary>
        /// Translates blue and red BlockReferences od offset.
        /// </summary>
        public void TranslatingSections(double offset)
        {
            foreach (Entity ent in _model1.Entities)
            {
                if (ent is BlockReference)
                {
                    // traslates red section up
                    if (((BlockReference)ent).BlockName.CompareTo("RedBlock") == 0)
                        ((BlockReference)ent).Translate(Direction * offset);

                    // traslates blue section down
                    if (((BlockReference)ent).BlockName.CompareTo("BlueBlock") == 0)
                        ((BlockReference)ent).Translate(Direction * -offset);

                    ent.RegenMode = regenType.RegenAndCompile;
                }
            }

            _model1.Entities.Regen();
            _model1.Invalidate();
        }
    }

    class MyIndexTriangle : IndexTriangle
    {
        public bool Found;          // found, but Group value could not be corrected yet
        public bool ReVisit;        // needs to be revisited
        public bool Visited;        // visit completed, Group value is correct
        public int Group;           // blue = -1, yellow = 0, red = 1
        public Vector3D Normal;

        public MyIndexTriangle(int v1, int v2, int v3, bool visited, int group, Point3D[] vertices)
            : base(v1, v2, v3)
        {
            Visited = visited;
            Group = group;
            Found = false;
            ReVisit = false;

            Triangle t = new Triangle(vertices[V1], vertices[V2], vertices[V3]);
            t.Regen(0.1);
            Normal = t.Normal;
        }
    }

}