using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using Syroot.NintenTools.Bfres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goombratlas
{
    public enum TrackType
    {
        XSCA = 0x4,
        YSCA = 0x8,
        ZSCA = 0xC,
        XPOS = 0x10,
        YPOS = 0x14,
        ZPOS = 0x18,
        XROT = 0x20,
        YROT = 0x24,
        ZROT = 0x28,
        WROT = 0x2C,
    }

    class StaticModel : AbstractGlDrawable
    {
        string modelName;
        ResFile bfres;

        public StaticModel(string modelName, ResFile bfres)
        {
            this.modelName = modelName;
            this.bfres = bfres;

            //foreach(var anim in bfres.SkeletalAnims[2].BoneAnims)
            //{
            //    Console.WriteLine(anim.Name);
            //    foreach (var curve in anim.Curves)
            //    {
            //        Console.Write(Enum.GetName(typeof(TrackType), curve.AnimDataOffset) + ": " + curve.Keys[curve.Keys.GetLength(0)-1,0]*curve.Scale + curve.Offset + " ");
            //    }
            //    Console.WriteLine();
            //}
        }

        float[] transformData = new float[10];

        public override void Draw(GL_ControlModern control, Pass pass)
        {
            control.CurrentShader = BfresModelCache.BfresShaderProgram;

            if (!Visible)
            {
                BfresModelCache.BfresShaderProgram.SetFloat("dithering", 3);
            }

            control.ResetModelMatrix();
            BfresModelCache.TryDraw(modelName, control, pass, Vector4.Zero
                /*,
                (parentMtx, bone) =>
                {
                    var anim = bfres.SkeletalAnims[2].BoneAnims.Find(x => x.Name == bone.Name);
                    if (anim != null)
                    {
                        #region fill transformData
                        transformData[0] = bone.Scale.X;
                        transformData[1] = bone.Scale.Y;
                        transformData[2] = bone.Scale.Z;
                        transformData[3] = bone.Position.X;
                        transformData[4] = bone.Position.Y;
                        transformData[5] = bone.Position.Z;
                        transformData[6] = bone.Rotation.X;
                        transformData[7] = bone.Rotation.Y;
                        transformData[8] = bone.Rotation.Z;
                        transformData[9] = bone.Rotation.W;
                        #endregion

                        return Matrix4.Identity;
                    }
                    else
                    {
                        return parentMtx * bone.CalculateRelativeMatrix();
                    }
                }*/);

            BfresModelCache.BfresShaderProgram.SetFloat("dithering", 0);
        }

        public override void Draw(GL_ControlLegacy control, Pass pass)
        {
            throw new NotImplementedException();
        }

        public override void Prepare(GL_ControlModern control)
        {
            BfresModelCache.Submit(modelName, bfres, control);
        }

        public override void Prepare(GL_ControlLegacy control)
        {
            throw new NotImplementedException();
        }
    }
}
