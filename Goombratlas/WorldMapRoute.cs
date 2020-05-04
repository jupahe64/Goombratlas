using GL_EditorFramework.EditorDrawables;
using OpenTK;
using Syroot.NintenTools.Bfres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goombratlas
{
    class WorldMapRoute
    {
        public readonly List<WorldMapPoint> RoutePoints = new List<WorldMapPoint>();

        public IEnumerable<WorldMapPoint> Points
        {
            get
            {
                if (Start != null)
                    yield return Start;

                foreach (WorldMapPoint point in RoutePoints)
                    yield return point;

                if (End != null)
                    yield return End;
            }
        }

        public WorldMapPoint Start { get; set; } = null;

        public WorldMapPoint End { get; set; } = null;

        public override string ToString()
        {
            return Name;
        }

        public void AddToBones(ResDict<Bone> bones, Matrix4 parentBoneMatrix, short parentBoneIndex = -1)
        {
            bones.Add(Name, new Bone
            {
                Name = Name,
                Position = Vector4.Transform(new Vector4(0, 0, 0, 1), parentBoneMatrix.Inverted()).Xyz.ToVec3F(),
                Rotation = new Syroot.Maths.Vector4F(0, 0, 0, 1),
                Scale = Syroot.Maths.Vector3F.One,
                ParentIndex = parentBoneIndex,
                FlagsRotation = BoneFlagsRotation.EulerXYZ
            });

            parentBoneIndex = (short)(bones.Count-1);

            foreach(WorldMapPoint point in RoutePoints)
            {
                point.AddToBones(bones, parentBoneMatrix, parentBoneIndex);

                parentBoneMatrix = Matrix4.CreateTranslation(point.Position.ToSkeletonScale());
                parentBoneIndex++;
            }
        }

        public string Name { get; private set; }

        public WorldMapRoute(BoneIter boneIter, Matrix4 parentMatrix, Dictionary<string,string[]> pointCsv)
        {
            Name = boneIter.Current.Name;

            Dictionary<short, Matrix4> loadedBoneMatrices = new Dictionary<short, Matrix4>
            {
                { (short)boneIter.CurrentBoneIndex, boneIter.Current.CalculateRelativeMatrix() * parentMatrix }
            };

            while (loadedBoneMatrices.TryGetValue(boneIter.PeekNext()?.ParentIndex ?? -1, out Matrix4 _parentMatrix)) //is part of the child tree
            {
                boneIter.MoveNext();

                pointCsv.TryGetValue(boneIter.Current.Name, out string[] attributes);

                RoutePoints.Add(new WorldMapPoint(boneIter, _parentMatrix, attributes, this));

                loadedBoneMatrices.Add((short)boneIter.CurrentBoneIndex, boneIter.Current.CalculateRelativeMatrix() * _parentMatrix);
            }
        }
    }
}
