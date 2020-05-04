using OpenTK;
using Syroot.NintenTools.Bfres;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goombratlas
{
    static class ExtensionMethods
    {
        public static Syroot.Maths.Vector3F ToVec3F(this Vector3 vec) => new Syroot.Maths.Vector3F(vec.X, vec.Y, vec.Z);

        public static Vector3 ToVec3(this Syroot.Maths.Vector3F vec) => new Vector3(vec.X, vec.Y, vec.Z);

        public static Vector3 ToSceneScale(this Vector3 vec) => new Vector3(
            (float)Math.Round(vec.X/100f, 2), 
            (float)Math.Round(vec.Y/100f, 2), 
            (float)Math.Round(vec.Z/100f, 2)
            );

        public static Vector3 ToSkeletonScale(this Vector3 vec) => vec * 100;

        public static Matrix4 CalculateRelativeMatrix(this Bone bone) =>
            Matrix4.CreateScale(bone.Scale.ToVec3()) *
            Matrix4.CreateRotationX(bone.Rotation.X) *
            Matrix4.CreateRotationY(bone.Rotation.Y) *
            Matrix4.CreateRotationZ(bone.Rotation.Z) *
            Matrix4.CreateTranslation(bone.Position.ToVec3());
    }

    class BoneIter : IEnumerator<Bone>
    {
        readonly ResDict<Bone> bones;

        public BoneIter(ResDict<Bone> bones)
        {
            this.bones = bones;
        }

        public Bone Current
        {
            get
            {
                try
                {
                    return bones[CurrentBoneIndex];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        public int CurrentBoneIndex { get; private set; } = -1;

        public void Dispose()
        {
            
        }

        public Bone PeekNext()
        {
            if (CurrentBoneIndex + 1 < bones.Count)
                return bones[CurrentBoneIndex + 1];
            else
                return null;
        }

        public bool MoveNext()
        {
            CurrentBoneIndex++;
            return CurrentBoneIndex < bones.Count;
        }

        public void Reset()
        {
            CurrentBoneIndex = -1;
        }
    }
}