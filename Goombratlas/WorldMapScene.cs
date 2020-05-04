using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using SARCExt;
using Syroot.BinaryData;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace Goombratlas
{
    class WorldMapScene : EditorSceneBase
    {
        class ConnectionDrawer : AbstractGlDrawable
        {
            WorldMapScene scene;

            public ConnectionDrawer(WorldMapScene scene)
            {
                this.scene = scene;
            }

            public override void Draw(GL_ControlModern control, Pass pass)
            {
                if(pass!=Pass.OPAQUE)
                    return;

                control.ResetModelMatrix();

                control.CurrentShader = Renderers.ColorBlockRenderer.SolidColorShaderProgram;
                foreach (var route in scene.Routes)
                {
                    int randomColor = control.RNG.Next();
                    var color = new Vector4(
                        (((randomColor >> 16) & 0xFF) / 255f) * 0.5f + 0.25f,
                        (((randomColor >> 8) & 0xFF) / 255f) * 0.5f + 0.25f,
                        ((randomColor & 0xFF) / 255f) * 0.5f + 0.25f,
                        1
                        );

                    if (scene.unlockRoutesSecret.Contains(route.Name))
                        color = EditableObject.selectColor - new Vector4(0, 0.5f, 0.5f, 0);
                    else if (scene.unlockRoutesNormal.Contains(route.Name))
                        color = EditableObject.selectColor - new Vector4(0.5f, 0, 0.5f, 0);

                    control.CurrentShader.SetVector4("color", color);
                    GL.Begin(PrimitiveType.LineStrip);
                    foreach (var item in route.Points)
                    {
                        GL.Vertex3(item.GlobalPosition+new Vector3(0, 0.03125f,0));
                    }
                    GL.End();
                }
            }

            public override void Draw(GL_ControlLegacy control, Pass pass)
            {
                throw new NotImplementedException();
            }

            public override void Prepare(GL_ControlModern control)
            {
                Renderers.ColorBlockRenderer.Initialize(control);
            }

            public override void Prepare(GL_ControlLegacy control)
            {
                throw new NotImplementedException();
            }
        }

        private KeyValuePair<string, byte[]> bfresEntry;
        private KeyValuePair<string, byte[]> pointEntry;
        private KeyValuePair<string, byte[]> routeEntry;
        private ResFile bfres;
        private ResDict<Bone> bones;
        private Dictionary<string, string[]> pointCsv;
        private Dictionary<string, string[]> routeCsv;
        private SarcData sarc;
        private string sarcName;

        public List<IWorldMapObj> WorldMapObjects { get; private set; } = new List<IWorldMapObj>();

        public List<WorldMapRoute> Routes { get; private set; } = new List<WorldMapRoute>();

        public bool TryGetPoint(string name, out WorldMapPoint point)
        {
            foreach (var obj in WorldMapObjects)
            {
                if (obj is WorldMapPoint _point)
                {
                    if (_point.BoneName == name)
                    {
                        point = _point;
                        return true;
                    }
                }
            }

            foreach (var route in Routes)
            {
                foreach (var _point in route.RoutePoints)
                {
                    if (_point.BoneName == name)
                    {
                        point = _point;
                        return true;
                    }
                }
            }

            point = null;
            return false;
        }

        public WorldMapScene(SarcData sarc, string sarcName)
        {
            multiSelect = true;

            #region read files
            this.sarc = sarc;
            this.sarcName = sarcName;

            bfresEntry = sarc.Files.First(x => x.Key.EndsWith(".bfres"));

            pointEntry = sarc.Files.First(x => x.Key.StartsWith("pointW"));

            routeEntry = sarc.Files.First(x => x.Key.StartsWith("routeW"));

            bfres = new ResFile(new MemoryStream(bfresEntry.Value));
            bones = bfres.Models[0].Skeleton.Bones;

            pointCsv = Encoding.GetEncoding("shift-jis").GetString(sarc.Files.First(file => file.Key.StartsWith("pointW")).Value).
                Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Reverse().Skip(1).Select(row =>
                {
                    int pos = 0;
                    List<string> fields = new List<string>();

                    while (pos < row.Length)
                    {
                        string value;

                            // Special handling for quoted field
                            if (row[pos] == '"')
                        {
                                // Skip initial quote
                                pos++;

                                // Parse quoted value
                                int start = pos;
                            while (pos < row.Length)
                            {
                                    // Test for quote character
                                    if (row[pos] == '"')
                                {
                                        // Found one
                                        pos++;

                                        // If two quotes together, keep one
                                        // Otherwise, indicates end of value
                                        if (pos >= row.Length || row[pos] != '"')
                                    {
                                        pos--;
                                        break;
                                    }
                                }
                                pos++;
                            }
                            value = row.Substring(start, pos - start);
                            value = value.Replace("\"\"", "\"");
                        }
                        else
                        {
                                // Parse unquoted value
                                int start = pos;
                            while (pos < row.Length && row[pos] != ',')
                                pos++;
                            value = row.Substring(start, pos - start);
                        }

                            // Add field to list
                            fields.Add(value);

                            // Eat up to and including next comma
                            while (pos < row.Length && row[pos] != ',')
                            pos++;
                        if (pos < row.Length)
                            pos++;
                    }

                    return fields.ToArray();
                }).Reverse().ToArray().ToDictionary(x => x[1]);

            routeCsv = Encoding.GetEncoding("shift-jis").GetString(sarc.Files.First(file => file.Key.StartsWith("routeW")).Value).
                Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Reverse().Skip(1).Select(row =>
                {
                    int pos = 0;
                    List<string> fields = new List<string>();

                    while (pos < row.Length)
                    {
                        string value;

                            // Special handling for quoted field
                            if (row[pos] == '"')
                        {
                                // Skip initial quote
                                pos++;

                                // Parse quoted value
                                int start = pos;
                            while (pos < row.Length)
                            {
                                    // Test for quote character
                                    if (row[pos] == '"')
                                {
                                        // Found one
                                        pos++;

                                        // If two quotes together, keep one
                                        // Otherwise, indicates end of value
                                        if (pos >= row.Length || row[pos] != '"')
                                    {
                                        pos--;
                                        break;
                                    }
                                }
                                pos++;
                            }
                            value = row.Substring(start, pos - start);
                            value = value.Replace("\"\"", "\"");
                        }
                        else
                        {
                                // Parse unquoted value
                                int start = pos;
                            while (pos < row.Length && row[pos] != ',')
                                pos++;
                            value = row.Substring(start, pos - start);
                        }

                            // Add field to list
                            fields.Add(value);

                            // Eat up to and including next comma
                            while (pos < row.Length && row[pos] != ',')
                            pos++;
                        if (pos < row.Length)
                            pos++;
                    }

                    return fields.ToArray();
                }).Reverse().ToArray().ToDictionary(x => x[0]);


            //SaveFileDialog sfd = new SaveFileDialog()
            //{
            //    FileName = sarcName
            //};

            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            //    var stream = new MemoryStream();
            //    bfres.Save(stream, true);

            //    sarc.Files[bfresEntry.Key] = stream.ToArray();

            //    Yaz0.Compress(new FileStream(sfd.FileName, FileMode.OpenOrCreate), SARC.PackN(sarc));
            //}

            #endregion

            #region read map from bones
            Matrix4 rootMatrix = bones[0].CalculateRelativeMatrix();

            var boneIter = new BoneIter(bones);

            boneIter.MoveNext(); //first bone is the root, we don't need to read it

            while(boneIter.MoveNext())
            {
                var bone = boneIter.Current;

                if (bone.ParentIndex == -1) //there is more than one root (should never happen)
                    break;

                if (bone.Name.StartsWith("R")) //indicates a Route
                {
                    Routes.Add(new WorldMapRoute(boneIter, rootMatrix, pointCsv));
                    continue;
                }

                if (bone.ParentIndex != 0)
                    throw new Exception(bone.Name + " is in an unexpected Layer, proceeding might currupt the file");

                Vector4 pos = new Vector4(
                    bone.Position.X / 100,
                    bone.Position.Y / 100,
                    bone.Position.Z / 100,
                    1
                    );

                if (bone.Name == "course")
                {
                    Matrix4 courseBoneMatrix = boneIter.Current.CalculateRelativeMatrix() * rootMatrix;

                    short courseBoneIndex = (short)boneIter.CurrentBoneIndex;
                    while (boneIter.PeekNext()?.ParentIndex == courseBoneIndex)
                    {
                        boneIter.MoveNext();
                        if (pointCsv.ContainsKey(boneIter.Current.Name))
                            WorldMapObjects.Add(new WorldMapPoint(boneIter, courseBoneMatrix, pointCsv[boneIter.Current.Name]));
                        else
                            WorldMapObjects.Add(new UnknownWorldMapBone(boneIter, rootMatrix));
                    }
                }
                else if (pointCsv.ContainsKey(bone.Name))
                    WorldMapObjects.Add(new WorldMapPoint(boneIter, rootMatrix, pointCsv[bone.Name]));
                else if (bone.Name.StartsWith("cob"))
                    WorldMapObjects.Add(new WorldMapCob(boneIter, rootMatrix));
                else
                    WorldMapObjects.Add(new UnknownWorldMapBone(boneIter, rootMatrix));
            }
            #endregion

            foreach (var route in Routes)
            {
                TryGetPoint(route.Name.Substring(1, 4), out WorldMapPoint start);
                TryGetPoint(route.Name.Substring(5, 4), out WorldMapPoint end);

                route.Start = start;
                route.End = end;
            }

            //ModelObjectReplace();

            StaticObjects.Add(new StaticModel(bfresEntry.Key, bfres));
            StaticObjects.Add(new ConnectionDrawer(this));

            SelectionChanged += WorldMapScene_SelectionChanged;
        }

        public readonly HashSet<string> unlockRoutesNormal = new HashSet<string>();

        public readonly HashSet<string> unlockRoutesSecret = new HashSet<string>();

        private void WorldMapScene_SelectionChanged(object sender, EventArgs e)
        {
            unlockRoutesNormal.Clear();
            unlockRoutesSecret.Clear();

            if (SelectedObjects.Count==1 && SelectedObjects.First() is WorldMapPoint point)
            {
                if (pointCsv.TryGetValue(point.BoneName, out string[] entry))
                {
                    foreach (var item in entry[4].Split(','))
                        unlockRoutesNormal.Add(item);

                    foreach (var item in entry[7].Split(','))
                        unlockRoutesSecret.Add(item);
                }
            }
        }

        public void ModelObjectReplace()
        {
            var mdl = bfres.Models[2];
            int shapeIndex = mdl.Shapes.IndexOf("polySurface1489__mtGrass");
            VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shapeIndex], ByteOrder.BigEndian);

            List<uint> index_buffer = new List<uint>();

            List<Vector3> positions = new List<Vector3>();

            List<Vector2> uvs = new List<Vector2>();

            List<Vector3> normals = new List<Vector3>();

            List<Syroot.Maths.Vector4F> _p0 = new List<Syroot.Maths.Vector4F>();

            List<Syroot.Maths.Vector4F> _n0 = new List<Syroot.Maths.Vector4F>();

            List<Syroot.Maths.Vector4F> _c0 = new List<Syroot.Maths.Vector4F>();

            List<Syroot.Maths.Vector4F> _u0 = new List<Syroot.Maths.Vector4F>();

            List<Syroot.Maths.Vector4F> _u1 = new List<Syroot.Maths.Vector4F>();

            Dictionary<(int, int, int), uint> indices = new Dictionary<(int, int, int), uint>();

            foreach (string row in File.ReadAllLines("C:\\Users\\Jupahe\\Documents\\NsmbuStuff\\For sierra\\terraforming.obj"))
            {
                string[] args = row.Split(' ');
                if (args[0] == "v")
                    positions.Add(new Vector3(
                        float.Parse(args[1]),
                        float.Parse(args[2]),
                        float.Parse(args[3])
                        ));
                else if (args[0] == "vt")
                    uvs.Add(new Vector2(
                        float.Parse(args[1]),
                        float.Parse(args[2])
                        ));
                else if (args[0] == "vn")
                    normals.Add(new Vector3(
                        float.Parse(args[1]),
                        float.Parse(args[2]),
                        float.Parse(args[3])
                        ));
                else if (args[0] == "f")
                {
                    for (int i = 1; i < args.Length; i++)
                    {
                        int[] _ind = args[i].Split('/').Select(x => int.Parse(x) - 1).ToArray();

                        var key = (_ind[0], _ind[1], _ind[2]);

                        if (indices.ContainsKey(key))
                            index_buffer.Add(indices[key]);
                        else
                        {
                            uint index = (uint)_p0.Count;

                            _p0.Add(new Syroot.Maths.Vector4F(
                                positions[_ind[0]].X,
                                positions[_ind[0]].Y,
                                positions[_ind[0]].Z,
                                0
                                ));
                            
                            _u0.Add(new Syroot.Maths.Vector4F(
                                uvs[_ind[1]].X,
                                uvs[_ind[1]].Y,
                                0,
                                0
                                ));

                            _u1.Add(Syroot.Maths.Vector4F.Zero);

                            _c0.Add(Syroot.Maths.Vector4F.One);

                            _n0.Add(new Syroot.Maths.Vector4F(
                                normals[_ind[2]].X,
                                normals[_ind[2]].Y,
                                normals[_ind[2]].Z,
                                0
                                ));

                            indices.Add(key, index);

                            index_buffer.Add(index);
                        }
                    }
                }
            }

            helper["_p0"].Data = _p0.ToArray();
            helper["_n0"].Data = _n0.ToArray();
            helper["_c0"].Data = _c0.ToArray();
            helper["_u0"].Data = _u0.ToArray();
            helper["_u1"].Data = _u1.ToArray();

            mdl.VertexBuffers[shapeIndex] = helper.ToVertexBuffer();

            mdl.Shapes[shapeIndex].Meshes[0].SetIndices(index_buffer);
        }

        public void Fun()
        {
            foreach (var item in pointCsv.Values)
            {
                item[2] = "stop";
            }

            foreach (var item in routeCsv.Values)
            {
                item[1] = "ジャンプ";
            }
        }

        public bool TryGetConnection(string point1, string point2, out string[] entry)
        {
            string name = $"R{point1}{point2}";
            if (routeCsv.ContainsKey(name))
            {
                entry = routeCsv[name];
                return true;
            }
            name = $"R{point2}{point1}";
            if (routeCsv.ContainsKey(name))
            {
                entry = routeCsv[name];
                return true;
            }
            entry = new string[] {"",""};
            return false;
        }

        private Matrix4 GetBoneMatrix(int boneIndex)
        {
            Matrix4 boneMatrix = Matrix4.CreateScale(new Vector3(
                bones[boneIndex].Scale.X,
                bones[boneIndex].Scale.Y,
                bones[boneIndex].Scale.Z
                )) *
            Matrix4.CreateRotationX(bones[boneIndex].Rotation.X) *
            Matrix4.CreateRotationY(bones[boneIndex].Rotation.Y) *
            Matrix4.CreateRotationZ(bones[boneIndex].Rotation.Z) *
            Matrix4.CreateTranslation(new Vector3(
                bones[boneIndex].Position.X,
                bones[boneIndex].Position.Y,
                bones[boneIndex].Position.Z
                ));

            if (bones[boneIndex].ParentIndex == -1)
                return boneMatrix;
            else
                return boneMatrix * GetBoneMatrix(bones[boneIndex].ParentIndex);
        }

        public void SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                FileName = sarcName
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                sarc.Files[pointEntry.Key] = SaveCsvFromRows(pointCsv.Values.OrderBy(x=>x[0]));

                sarc.Files[routeEntry.Key] = SaveCsvFromRows(routeCsv.Values);

                string rootName = bones[0].Name;

                bones.Clear();

                bones.Add(rootName, new Bone
                {
                    Name = rootName,
                    Position = Syroot.Maths.Vector3F.Zero,
                    Rotation = new Syroot.Maths.Vector4F(0, 0, 0, 1),
                    Scale = Syroot.Maths.Vector3F.One,
                    ParentIndex = -1,
                    FlagsRotation = BoneFlagsRotation.EulerXYZ
                });

                foreach (var obj in WorldMapObjects)
                {
                    obj.AddToBones(bones, Matrix4.Identity, 0);
                }

                foreach (var route in Routes)
                {
                    route.AddToBones(bones, Matrix4.Identity, 0);
                }

                var stream = new MemoryStream();
                bfres.Save(stream, true);

                sarc.Files[bfresEntry.Key] = stream.ToArray();
                
                Yaz0.Compress(new FileStream(sfd.FileName, FileMode.OpenOrCreate), SARC.PackN(sarc));
            }
        }

        private static byte[] SaveCsvFromRows(IEnumerable<string[]> pointCsv)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var row in pointCsv)
            {
                foreach (string value in row)
                {
                    // Implement special handling for values that contain comma or quote
                    // Enclose in quotes and double up any double quotes
                    if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                        builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                    else
                        builder.Append(value);

                    builder.Append(',');
                }
                builder.Append("\r\n");
            }

            var bytes = Encoding.GetEncoding("shift-jis").GetBytes(builder.ToString());
            return bytes;
        }

        public override void DeleteSelected()
        {
            foreach (var route in Routes)
            {
                for (int i = route.RoutePoints.Count - 1; i >= 0; i--)
                {
                    WorldMapPoint point = route.RoutePoints[i];
                    if (point.IsSelected() && !pointCsv.ContainsKey(point.BoneName))
                    {
                        string[] connections = routeCsv.Keys.Where(x => x.Contains(point.BoneName)).ToArray();

                        if (connections.Length == 2)
                        {
                            foreach (var item in connections)
                            {
                                routeCsv.Remove(item);
                            }

                            string firstName = i == 0 ? route.Name.Substring(1,4) : route.RoutePoints[i - 1].BoneName;

                            string secondName = i == route.RoutePoints.Count - 1 ? route.Name.Substring(5, 4) : route.RoutePoints[i + 1].BoneName;

                            string connectionName = "R" + firstName + secondName;

                            routeCsv.Add(connectionName, new string[] { connectionName, connections[0]});

                            route.RoutePoints.RemoveAt(i);
                        }
                    }
                }
            }
        }

        protected override IEnumerable<IEditableObject> GetObjects()
        {
            foreach (var obj in WorldMapObjects)
            {
                foreach (var _obj in obj.GetObjects())
                {
                    yield return _obj;
                }
            }
            
            foreach (var route in Routes)
            {
                foreach (var point in route.RoutePoints)
                {
                    yield return point;
                }
            }
        }

        public override uint KeyDown(KeyEventArgs e, GL_ControlBase control, bool isRepeat)
        {
            if (e.KeyCode == Keys.V && !isRepeat)
            {
                StaticObjects[0].Visible = false;

                return REDRAW_PICKING;
            }

            return base.KeyDown(e, control, isRepeat);
        }

        public override uint KeyUp(KeyEventArgs e, GL_ControlBase control)
        {
            if (e.KeyCode == Keys.V)
            {
                StaticObjects[0].Visible = true;

                return REDRAW_PICKING;
            }

            return base.KeyUp(e, control);
        }
    }
}
