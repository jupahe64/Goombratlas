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
    interface IWorldMapObj
    {
        Vector3 Position { get; set; }

        IEnumerable<IEditableObject> GetObjects();

        void AddToBones(ResDict<Bone> bones, Matrix4 parentBoneMatrix, short parentBoneIndex = -1);
    }
}
