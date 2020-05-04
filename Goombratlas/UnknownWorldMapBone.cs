using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
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
    class UnknownWorldMapBone : SingleObject, IWorldMapObj
    {
        public override string ToString()
        {
            return BoneName;
        }

        protected override Vector4 Color => new Vector4(0.5f, 0.5f, 0.5f, 1);

        protected override float BoxScale => 0.25f;

        public string BoneName { get; set; }

        private readonly Bone bone;

        public List<UnknownWorldMapBone> Children { get; set; } = new List<UnknownWorldMapBone>();

        public UnknownWorldMapBone(BoneIter boneIter, Matrix4 parentMatrix)
        : base(Vector4.Transform(new Vector4(boneIter.Current.Position.ToVec3(), 1), parentMatrix).Xyz.ToSceneScale())
        {
            BoneName = boneIter.Current.Name;
            bone = boneIter.Current;

            Dictionary<short, (Matrix4 matrix, UnknownWorldMapBone bone)> loadedWorldmapBones = new Dictionary<short, (Matrix4 matrix, UnknownWorldMapBone bone)>
            {
                { (short)boneIter.CurrentBoneIndex, (boneIter.Current.CalculateRelativeMatrix() * parentMatrix, this) }
            };

            while (loadedWorldmapBones.TryGetValue(boneIter.PeekNext()?.ParentIndex ?? -1, out (Matrix4 matrix, UnknownWorldMapBone bone) parent)) //is part of the child tree
            {
                boneIter.MoveNext();

                var bone = new UnknownWorldMapBone(boneIter, parent.matrix);

                parent.bone.Children.Add(bone);

                loadedWorldmapBones.Add(
                    (short)boneIter.CurrentBoneIndex, 
                    (boneIter.Current.CalculateRelativeMatrix() * parent.matrix, bone)
                    );
            }
        }

        public void AddToBones(ResDict<Bone> bones, Matrix4 parentBoneMatrix, short parentBoneIndex = -1)
        {
            bone.Name = BoneName;
            bone.Position = Vector4.Transform(new Vector4(Position.ToSkeletonScale(), 1), parentBoneMatrix.Inverted()).Xyz.ToVec3F();
            bone.ParentIndex = parentBoneIndex;

            bones.Add(BoneName, bone);

            parentBoneIndex = (short)(bones.Count - 1);

            foreach (var child in Children)
            {
                child.AddToBones(bones, bone.CalculateRelativeMatrix() * parentBoneMatrix, parentBoneIndex);
            }
        }

        public IEnumerable<IEditableObject> GetObjects()
        {
            yield return this;

            foreach (var obj in Children.SelectMany(x=>x.GetObjects()))
            {
                yield return obj;
            }
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

            UnknownWorldMapBone obj;
            EditorSceneBase scene;
            public GeneralUIContainer(UnknownWorldMapBone obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                obj.BoneName = control.TextInput(obj.BoneName, "Bone");

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
