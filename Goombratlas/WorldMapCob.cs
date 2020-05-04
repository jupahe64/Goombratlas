using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using Syroot.NintenTools.Bfres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using WinInput = System.Windows.Input;

namespace Goombratlas
{
    class WorldMapCob : SingleObject, IWorldMapObj
    {
        public override string ToString()
        {
            return ModelName;
        }

        public new static Vector4 selectColor = new Vector4(EditableObject.selectColor.Xyz, 0.5f);
        public new static Vector4 hoverColor = new Vector4(EditableObject.hoverColor.Xyz, 0.125f);

        [PropertyCapture.Undoable]
        public string ModelName { get; set; }

        public WorldMapCob(BoneIter boneIter, Matrix4 parentMatrix)
        : base(Vector4.Transform(new Vector4(boneIter.Current.Position.ToVec3(), 1), parentMatrix).Xyz.ToSceneScale())
        {
            ModelName = boneIter.Current.Name.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
        }

        public override void Prepare(GL_ControlModern control)
        {
            base.Prepare(control);

            Program.TrySubmitCobModel(ModelName, control);
        }

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if (!editorScene.ShouldBeDrawn(this))
                return;

            bool hovered = editorScene.Hovered == this;

            string modelName = ModelName;

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

                if(modelName=="cobKinoko1up" || modelName == "cobKinokoShuffle")
                    BfresModelCache.TryDraw(modelName, control, pass, highlightColor);
                else
                    BfresModelCache.TryDraw(modelName, control, pass, highlightColor);
                return;
            }
            else
            {
                if (pass == Pass.TRANSPARENT)
                    return;

                control.UpdateModelMatrix(
                    Matrix4.CreateScale(0.25f) *
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

            Renderers.ColorBlockRenderer.Draw(control, pass, blockColor, lineColor, control.NextPickingColor());
        }

        public void AddToBones(ResDict<Bone> bones, Matrix4 parentBoneMatrix, short parentBoneIndex = -1)
        {
            int cobCount = bones.Keys.Count(x => x.StartsWith(ModelName));

            string modelName = ModelName;

            if (cobCount > 0)
                modelName = ModelName + cobCount+1;

            bones.Add(modelName, new Bone
            {
                Name = modelName,
                Position = Vector4.Transform(new Vector4(Position.ToSkeletonScale(), 1), parentBoneMatrix.Inverted()).Xyz.ToVec3F(),
                Rotation = new Syroot.Maths.Vector4F(0, 0, 0, 1),
                Scale = Syroot.Maths.Vector3F.One,
                ParentIndex = parentBoneIndex,
                FlagsRotation = BoneFlagsRotation.EulerXYZ
            });
        }

        public IEnumerable<IEditableObject> GetObjects()
        {
            yield return this;
        }

        public override bool TrySetupObjectUIControl(EditorSceneBase scene, ObjectUIControl objectUIControl)
        {
            if (Selected)
            {
                objectUIControl.AddObjectUIContainer(new GeneralUIContainer(this, scene), "General");

                return true;
            }
            else
                return false;
        }

        public class GeneralUIContainer : IObjectUIContainer
        {
            PropertyCapture? capture = null;

            WorldMapCob obj;
            EditorSceneBase scene;
            public GeneralUIContainer(WorldMapCob obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                obj.ModelName = control.TextInput(obj.ModelName, "Bone");

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
    }
}
