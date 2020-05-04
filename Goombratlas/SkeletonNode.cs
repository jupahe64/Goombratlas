//using Syroot.Maths;
//using Syroot.NintenTools.Bfres;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Goombratlas
//{
//    class Bone
//    {
//        public Bone RootFromBones(ResDict<Bone> bones)
//        {
//            List<Bone> nodesInOrder = new List<Bone>();

//            Bone root = ConvertToBone(bones[0]);

//            nodesInOrder.Add(root);

//            foreach (var bone in bones.Values.Skip(1))
//            {

//            }
//        }

//        public string Name { get; set; }

//        public Vector3F Position { get; set; }
//        public Vector4F Rotation { get; set; }
//        public Vector3F Scale { get; set; }
//        public Bone Parent { get; set; }
//        public List<Bone> Children { get; set; } = new List<Bone>();

//        static Bone ConvertToBone(Bone bone, Bone parent = null) => new Bone
//        {
//            Name = bone.Name,
//            Position = bone.Position,
//            Rotation = bone.Rotation,
//            Scale = bone.Scale,
//            Parent = parent
//        };

//        static Bone ConvertToBone(Bone node, short parentIndex) => new Bone
//        {
//            Name = node.Name,
//            Position = node.Position,
//            Rotation = node.Rotation,
//            Scale = node.Scale,
//            ParentIndex = parentIndex
//        };

//        public ResDict<Bone> ToBones()
//        {
//            ResDict<Bone> bones = new ResDict<Bone>();

//            AddToBones(bones);

//            return bones;
//        }

//        private void AddToBones(ResDict<Bone> bones, short parentIndex = -1)
//        {
//            bones.Add(Name, ConvertToBone(this, parentIndex));

//            foreach (var child in Children)
//                child.AddToBones(bones, (short)(bones.Count - 1));
//        }
//    }
//}
