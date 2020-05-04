using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using SARCExt;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Goombratlas
{
    public partial class WorldmapEditorForm : Form
    {
        WorldMapScene scene;

        string[] routeEntry;

        public WorldmapEditorForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            gL_ControlModern1.ActiveCamera = new GL_EditorFramework.StandardCameras.InspectCamera();

            BfresModelCache.Initialize(gL_ControlModern1);
        }
        
        private void Scene_SelectionChanged(object sender, EventArgs e)
        {
            var selectedObjects = scene.SelectedObjects.ToArray();

            if (selectedObjects.Length == 2 && selectedObjects[0] is WorldMapPoint && selectedObjects[1] is WorldMapPoint)
            {
                WorldMapPoint point1 = (WorldMapPoint)selectedObjects[0];
                WorldMapPoint point2 = (WorldMapPoint)selectedObjects[1];

                objectUIControl1.ClearObjectUIContainers();

                if(scene.TryGetConnection(point1.BoneName, point2.BoneName, out routeEntry))
                    objectUIControl1.AddObjectUIContainer(new ConnectionUIContainer(routeEntry,scene),"Connection");

                objectUIControl1.Refresh();
            }
            else
            {
                scene.SetupObjectUIControl(objectUIControl1);
            }

            sceneListView1.Refresh();
        }

        public class ConnectionUIContainer : IObjectUIContainer
        {
            string[] entry;
            EditorSceneBase scene;
            public ConnectionUIContainer(string[] entry, EditorSceneBase scene)
            {
                this.entry = entry;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                control.TextInput(entry[0], "Name");

                entry[1] = control.TextInput(entry[1], "Behaivior");
            }

            public void OnValueChangeStart()
            {

            }

            public void OnValueChanged()
            {
                
            }

            public void OnValueSet()
            {

            }

            public void UpdateProperties()
            {

            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var gamePath = System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(ofd.FileName) + "\\..\\..\\..\\");

                Program.GamePath = gamePath;

                scene = new WorldMapScene(
                SARC.UnpackRamN(Yaz0.Decompress(File.ReadAllBytes(ofd.FileName))),
                System.IO.Path.GetFileName(ofd.FileName));

                gL_ControlModern1.MainDrawable = scene;

                scene.SelectionChanged += Scene_SelectionChanged;
                scene.ObjectsMoved += Scene_ObjectsMoved;
                sceneListView1.SelectedItems = scene.SelectedObjects;
                sceneListView1.RootLists.Clear();
                sceneListView1.RootLists.Add("Map", scene.WorldMapObjects);

                foreach (var route in scene.Routes)
                {
                    sceneListView1.RootLists.Add(route.Name, route.RoutePoints);
                }

                sceneListView1.UpdateComboBoxItems();
                sceneListView1.SetRootList("Map");
            }

            sceneListView1.Refresh();
        }

        private void Scene_ObjectsMoved(object sender, EventArgs e)
        {
            objectUIControl1.Refresh();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scene?.SaveAs();
        }

        private void FUNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scene?.Fun();
        }

        private void SceneListView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (scene == null)
                return;

            //apply selection changes to scene
            if (e.SelectionChangeMode == SelectionChangeMode.SET)
            {
                scene.SelectedObjects.Clear();

                foreach (ISelectable obj in e.Items)
                    obj.SelectDefault(gL_ControlModern1);
            }
            else if (e.SelectionChangeMode == SelectionChangeMode.ADD)
            {
                foreach (ISelectable obj in e.Items)
                    obj.SelectDefault(gL_ControlModern1);
            }
            else //SelectionChangeMode.SUBTRACT
            {
                foreach (ISelectable obj in e.Items)
                    obj.DeselectAll(gL_ControlModern1);
            }

            e.Handled = true;
            gL_ControlModern1.Refresh();

            Scene_SelectionChanged(this, null);
        }

        private void DeleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scene?.DeleteSelected();
        }

        private void StraightenRouteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scene == null)
                return;

            if (scene.SelectedObjects.Count == 2)
            {
                var list = scene.SelectedObjects.ToArray();

                if(list[0] is WorldMapPoint first)
                {
                    if(list[1] is WorldMapPoint second)
                    {
                        foreach (var route in scene.Routes)
                        {
                            if(route.Name == "R"+first.BoneName+second.BoneName || route.Name == "R" + second.BoneName + first.BoneName)
                            {
                                var a = route.Start.GlobalPosition;
                                var b = route.End.GlobalPosition;

                                var count = route.RoutePoints.Count;

                                for (int i = 0; i < count; i++)
                                {
                                    route.RoutePoints[i].GlobalPosition = OpenTK.Vector3.Lerp(a, b, i / (float)count);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
