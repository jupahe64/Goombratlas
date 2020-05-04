using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using Syroot.NintenTools.Bfres;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using WinInput = System.Windows.Input;

namespace Goombratlas
{
    class WorldMapPoint : SingleObject, IWorldMapObj
    {
        public override string ToString()
        {
            return BoneName;
        }

        public new static Vector4 selectColor = new Vector4(EditableObject.selectColor.Xyz, 0.5f);
        public new static Vector4 hoverColor = new Vector4(EditableObject.hoverColor.Xyz, 0.125f);

        public string BoneName { get; set; }

        public string[] attributes;

        public WorldMapRoute Route { get; set; }

        public WorldMapCob Cob { get; set; }

        //public List<(string, WorldMapPoint)> Connections { get; private set; } = new List<(string, WorldMapPoint)>();

        public WorldMapPoint(BoneIter boneIter, Matrix4 parentMatrix, string[] attributes, WorldMapRoute route = null)
        : base(Vector4.Transform(new Vector4(boneIter.Current.Position.ToVec3(),1), parentMatrix).Xyz.ToSceneScale())
        {
            BoneName = boneIter.Current.Name;
            this.attributes = attributes;
            Route = route;
            if (boneIter.PeekNext()?.ParentIndex == boneIter.CurrentBoneIndex && (boneIter.PeekNext()?.Name.StartsWith("cob")??false))
            {
                Matrix4 boneMatrix = boneIter.Current.CalculateRelativeMatrix() * parentMatrix;
                boneIter.MoveNext();
                Cob = new WorldMapCob(boneIter, boneMatrix);
            }
        }

        public override void Prepare(GL_ControlModern control)
        {
            base.Prepare(control);

            Program.TrySubmitCobModel("cobCourse", control);
            Program.TrySubmitCobModel("cobCannon", control);
            Program.TrySubmitCobModel("cobSwitchA", control);
            Program.TrySubmitCobModel("cobSwitchB", control);
        }

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if (!editorScene.ShouldBeDrawn(this))
                return;

            bool hovered = editorScene.Hovered == this;

            string modelName = "";

            float noModelScale = 0.0625f;

            if (BoneName.StartsWith("W") && !BoneName.EndsWith("M0"))
            {
                modelName = "cobCourse";
            }
            //else if(attributes!=null && attributes[2].Contains("switchA"))
            //{
            //    modelName = "cobSwitchA";
            //}
            //else if (attributes != null && attributes[2].Contains("switchB"))
            //{
            //    modelName = "cobSwitchB";
            //}

            if (BfresModelCache.Contains(modelName))
            {
                control.UpdateModelMatrix(
                    Matrix4.CreateTranslation(Selected ? editorScene.SelectionTransformAction.NewPos(GlobalPosition) : GlobalPosition));

                Vector4 highlightColor;

                if (Selected)
                    highlightColor = selectColor;
                else if (hovered)
                    highlightColor = hoverColor;
                else
                    highlightColor = Vector4.Zero;

                BfresModelCache.TryDraw(modelName, control, pass, highlightColor);
                return;
            }
            else
            {
                if (pass == Pass.TRANSPARENT)
                    return;

                control.UpdateModelMatrix(
                Matrix4.CreateScale(noModelScale) *
                    Matrix4.CreateTranslation(Selected ? editorScene.SelectionTransformAction.NewPos(GlobalPosition) : GlobalPosition));
            }

            Vector4 blockColor;
            Vector4 lineColor;

            if (hovered && Selected)
                lineColor = hoverColor;
            else if (hovered || Selected)
                lineColor = selectColor;
            else
                lineColor = Color;

            if (hovered && Selected)
                blockColor = Color * 0.5f + hoverColor * 0.5f;
            else if (hovered || Selected)
                blockColor = Color * 0.5f + selectColor * 0.5f;
            else
                blockColor = Color;

            Renderers.ColorBlockRenderer.DrawWithoutTextures(control, pass, blockColor, lineColor, control.NextPickingColor());
        }

        public void AddToBones(ResDict<Bone> bones, Matrix4 parentBoneMatrix, short parentBoneIndex = -1)
        {
            bones.Add(BoneName, new Bone
            {
                Name = BoneName,
                Position = Vector4.Transform(new Vector4(Position.ToSkeletonScale(), 1), parentBoneMatrix.Inverted()).Xyz.ToVec3F(),
                Rotation = new Syroot.Maths.Vector4F(0,0,0,0),
                Scale = Syroot.Maths.Vector3F.One,
                ParentIndex = parentBoneIndex,
                FlagsRotation = BoneFlagsRotation.EulerXYZ
            });

            Cob?.AddToBones(bones, Matrix4.CreateTranslation(Position.ToSkeletonScale()), (short)(bones.Count-1));
        }

        public IEnumerable<GL_EditorFramework.EditorDrawables.IEditableObject> GetObjects()
        {
            yield return this;

            if (Cob != null)
                yield return Cob;
        }

        public override bool TrySetupObjectUIControl(EditorSceneBase scene, ObjectUIControl objectUIControl)
        {
            if (Selected)
            {
                objectUIControl.AddObjectUIContainer(new GeneralUIContainer(this, scene), "General");
                if (attributes != null)
                    objectUIControl.AddObjectUIContainer(new WorldMapPointUIContainer(this, scene), "Attributes");

                return true;
            }
            else
                return false;
        }

        public class GeneralUIContainer : IObjectUIContainer
        {
            PropertyCapture? capture = null;

            WorldMapPoint obj;
            EditorSceneBase scene;
            public GeneralUIContainer(WorldMapPoint obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                control.TextInput(obj.BoneName, "Bone");

                if (WinInput.Keyboard.IsKeyDown(WinInput.Key.LeftShift))
                    obj.Position = control.Vector3Input(obj.Position, "Position", 1, 16);
                else
                    obj.Position = control.Vector3Input(obj.Position, "Position", 0.125f, 2);
            }

            public void OnValueChangeStart()
            {
                capture = new PropertyCapture(obj);
            }

            public void OnValueChanged()
            {
                scene.Refresh();
            }

            public void OnValueSet()
            {
                capture?.HandleUndo(scene);
                capture = null;
                scene.Refresh();
            }

            public void UpdateProperties()
            {

            }
        }

        public class WorldMapPointUIContainer : IObjectUIContainer
        {
            WorldMapPoint obj;
            EditorSceneBase scene;
            public WorldMapPointUIContainer(WorldMapPoint obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                obj.attributes[2] = control.FullWidthTextInput(obj.attributes[2], "Events");

                control.Spacing(20);

                control.Heading("Normal Exit");
                obj.attributes[3] = control.FullWidthTextInput(obj.attributes[3], "Unlock Levels");
                obj.attributes[4] = control.FullWidthTextInput(obj.attributes[4], "Unlock Routes");

                control.Spacing(20);

                control.Heading("Secret Exit");
                obj.attributes[5] = control.FullWidthTextInput(obj.attributes[5], "Events");
                obj.attributes[6] = control.FullWidthTextInput(obj.attributes[6], "Unlock Levels");
                obj.attributes[7] = control.FullWidthTextInput(obj.attributes[7], "Unlock Routes");
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
    }
}
