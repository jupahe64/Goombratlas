﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GL_EditorFramework;
using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK.Graphics.OpenGL;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.GX2;
using Syroot.NintenTools.Bfres.Helpers;
using OpenTK;
using Syroot.BinaryData;
using TextureCompression;

namespace Goombratlas
{
    public static class BfresModelCache
    {
        private static bool initialized = false;

        public static ShaderProgram BfresShaderProgram;

        static Dictionary<string, CachedModel> cache = new Dictionary<string, CachedModel>();

        static Dictionary<string, (Vector4, VertexArrayObject, int, bool)> extraModels = new Dictionary<string, (Vector4, VertexArrayObject, int, bool)>();

        static Dictionary<string, Dictionary<string, int>> texArcCache = new Dictionary<string, Dictionary<string, int>>();

        public static int DefaultTetxure;

        public static int NoTetxure;

        public static void Initialize(GL_ControlModern control)
        {
            if (initialized)
                return;

            #region Shader Generation
            BfresShaderProgram = new ShaderProgram(
                    new FragmentShader(
                        @"#version 330
                    uniform sampler2D tex;
                    uniform vec4 highlight_color;
                    uniform float dithering;
                    in vec2 fragUV;
                    in vec4 fragColor;
                    void main(){
                        if(mod(gl_FragCoord.x,dithering)>=1 || mod(gl_FragCoord.y,dithering)>=1)
                            discard;
                        float hc_a   = highlight_color.w;
                        vec4 color = fragColor * texture(tex,fragUV);
                        gl_FragColor = vec4(color.rgb * (1-hc_a) + highlight_color.rgb * hc_a, color.a);
                        //gl_FragColor = vec4(color.a, color.a, color.a, 1);
                    }"),
                    new VertexShader(
                        @"#version 330
                    layout(location = 0) in vec4 position;
                    layout(location = 1) in vec2 uv;
                    layout(location = 2) in vec4 color;
                    uniform mat4 mtxMdl;
                    uniform mat4 mtxCam;
                    out vec2 fragUV;
                    out vec4 fragColor;

                    void main(){
                        fragUV = uv;
                        fragColor = color;
                        gl_Position = mtxCam*mtxMdl*position;
                    }"), control);
            #endregion


            #region Create Extra Models

            #region AreaCubeBase
            List<int> indices = new List<int>();

            float r = 5;
            float t = 10;
            float b = 0;

            float[] data = new float[]
            {
                -r, t, -r,
                 r, t, -r,
                -r, t,  r,
                 r, t,  r,
                -r, b, -r,
                 r, b, -r,
                -r, b,  r,
                 r, b,  r,
            };

            //-x to x
            indices.Add(0b000);
            indices.Add(0b001);
            indices.Add(0b010);
            indices.Add(0b011);
            indices.Add(0b100);
            indices.Add(0b101);
            indices.Add(0b110);
            indices.Add(0b111);

            //-y to y
            indices.Add(0b000);
            indices.Add(0b010);
            indices.Add(0b001);
            indices.Add(0b011);
            indices.Add(0b100);
            indices.Add(0b110);
            indices.Add(0b101);
            indices.Add(0b111);

            //-z to z
            indices.Add(0b000);
            indices.Add(0b100);
            indices.Add(0b001);
            indices.Add(0b101);
            indices.Add(0b010);
            indices.Add(0b110);
            indices.Add(0b011);
            indices.Add(0b111);

            SubmitExtraModel(control, true, "AreaCubeBase", indices, data, new Vector4(0, 0.5f, 1, 1));
            #endregion

            #region AreaCylinder
            indices = new List<int>();

            r = 5;
            t = 5;
            b = 0;
            int v = 16;

            data = new float[v * 2 * 3];

            double delta = Math.PI * 2 / v;

            int i = 0;
            
            for (int edgeIndex = 0; edgeIndex < v; edgeIndex++)
            {
                float x  = (float)Math.Sin(Math.PI * 2 * edgeIndex / v) * r;
                float z  = (float)Math.Cos(Math.PI * 2 * edgeIndex / v) * r;

                //top
                data[i++] = x;
                data[i++] = t;
                data[i++] = z;

                //bottom
                data[i++] = x;
                data[i++] = b;
                data[i++] = z;

                //top
                indices.Add( 2 * edgeIndex);
                indices.Add((2 * edgeIndex + 2) % (v * 2));

                //bottom
                indices.Add( 2 * edgeIndex + 1);
                indices.Add((2 * edgeIndex + 1 + 2) % (v * 2));

                //top to bottom
                indices.Add(2 * edgeIndex);
                indices.Add(2 * edgeIndex + 1);
            }
            
            SubmitExtraModel(control, true, "AreaCylinder", indices, data, new Vector4(0, 0.5f, 1, 1));
            #endregion

            #region TransparentWall
            indices = new List<int>();

            r = 5;

            data = new float[]
            {
                -r,  r, 0,
                 r,  r, 0,
                -r, -r, 0,
                 r, -r, 0,
            };
            //front
            indices.Add(0b00);
            indices.Add(0b10);
            indices.Add(0b01);

            indices.Add(0b01);
            indices.Add(0b10);
            indices.Add(0b11);

            //back
            indices.Add(0b01);
            indices.Add(0b10);
            indices.Add(0b00);

            indices.Add(0b11);
            indices.Add(0b10);
            indices.Add(0b01);

            SubmitExtraModel(control, false, "TransparentWall", indices, data, new Vector4(0, 0.5f, 1, 0.5f));
            #endregion

            #endregion

            Renderers.ColorBlockRenderer.Initialize(control);

            DefaultTetxure = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DefaultTetxure);

            var bmp = Properties.Resources.DefaultTexture;
            var bmpData = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, 32, 32),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bmp.PixelFormat);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, 32, 32, 0, PixelFormat.Bgr, PixelType.UnsignedByte, bmpData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            NoTetxure = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, NoTetxure);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, 1, 1, 0, PixelFormat.Bgr, PixelType.UnsignedByte, new uint[] { 0xFFFFFFFF });
            bmp.UnlockBits(bmpData);

            initialized = true;
        }

        private static void SubmitExtraModel(GL_ControlModern control, bool isLines, string modelName, List<int> indices, float[] data, Vector4 color)
        {
            int[] buffers = new int[2];
            GL.GenBuffers(2, buffers);

            int indexBuffer = buffers[0];
            int vaoBuffer = buffers[1];

            VertexArrayObject vao = new VertexArrayObject(vaoBuffer, indexBuffer);
            vao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            vao.Initialize(control);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vaoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);

            extraModels.Add(Path.GetFileNameWithoutExtension(modelName), (color, vao, indices.Count, isLines));
        }

        public static void Submit(string modelName, ResFile bfres, GL_ControlModern control, string textureArc = null)
        {
            if (!cache.ContainsKey(modelName))
                cache[modelName] = new CachedModel(bfres, textureArc, control);
        }

        public static bool TryDraw(string modelName, GL_ControlModern control, Pass pass, Vector4 highlightColor)
        {
            if (cache.ContainsKey(modelName))
            {
                cache[modelName].Draw(control, pass, highlightColor);
                return true;
            }
            else if (extraModels.TryGetValue(modelName, out (Vector4, VertexArrayObject, int, bool) entry))
            {
                control.CurrentShader = Renderers.ColorBlockRenderer.SolidColorShaderProgram;
                
                if (pass == Pass.PICKING)
                {
                    GL.LineWidth(5);
                    control.CurrentShader.SetVector4("color", control.NextPickingColor());
                }
                else
                {
                    GL.LineWidth(3);
                    control.CurrentShader.SetVector4("color", entry.Item1 * (1-highlightColor.W) + highlightColor * highlightColor.W);
                }

                entry.Item2.Use(control);
                
                GL.DrawElements(entry.Item4?PrimitiveType.Lines:PrimitiveType.Triangles, entry.Item3, DrawElementsType.UnsignedInt, 0);

                GL.LineWidth(2);

                return true;
            }
            else
                return false;
        }



        public static bool Contains(string modelName) => cache.ContainsKey(modelName) || extraModels.ContainsKey(modelName);

        public delegate Matrix4 BoneTransform(Matrix4 parentBoneMatrix, Bone bone);

        public struct CachedModel
        {
            struct ShapeEntry
            {
                public VertexArrayObject vao;
                public int indexBufferLength;
                public int texture;
                public (int, int)  wrapMode;
                public Pass pass;
                public ushort ridgetBoneIndex;
                public ushort[] skinBoneIndices;
            }

            struct ModelEntry
            {
                public string name;
                public ShapeEntry[] shapeEntries;
                public ResDict<Bone> bones;
                public Matrix4[] matrices;
            }

            static readonly float white = BitConverter.ToSingle(new byte[] {255, 255, 255, 255},0);

            readonly ModelEntry[] modelEntries;

            public CachedModel(ResFile bfres, string textureArc, GL_ControlModern control)
            {
                IEnumerable<Model> mdls;
                if (bfres.Name.StartsWith("CS_W"))
                    mdls = bfres.Models.Values.Skip(1);
                else
                    mdls = bfres.Models.Values;

                modelEntries = new ModelEntry[mdls.Count()];



                int modelIndex = 0;
                foreach (Model mdl in mdls)
                {
                    var shapeEntries = new ShapeEntry[mdl.Shapes.Count];

                    int shapeIndex = 0;
                    foreach (Shape shape in mdl.Shapes.Values)
                    {
                        uint[] indices = shape.Meshes[0].GetIndices().ToArray();

                        ShapeEntry shapeEntry = new ShapeEntry();

                        if (mdl.Materials[shape.MaterialIndex].TextureRefs.Count != 0)
                        {
                            int Target = 0;
                            for (int i = 0; i < mdl.Materials[shape.MaterialIndex].TextureRefs.Count; i++)
                            {
                                if (mdl.Materials[shape.MaterialIndex].TextureRefs[i].Name.Contains("_alb"))
                                {
                                    Target = i;
                                    break;
                                }
                            }
                            TextureRef texRef = mdl.Materials[shape.MaterialIndex].TextureRefs[Target];

                            TexSampler sampler = mdl.Materials[shape.MaterialIndex].Samplers[Target].TexSampler;

                            if (texRef.Texture != null)
                            {
                                shapeEntry.texture = UploadTexture(texRef.Texture);




                            }
                            else if (textureArc != null)
                            {
                                if (texArcCache.ContainsKey(textureArc) && texArcCache[textureArc].ContainsKey(texRef.Name))
                                {
                                    shapeEntry.texture = texArcCache[textureArc][texRef.Name];
                                }
                                else
                                {
                                    shapeEntry.texture = -2;
                                }
                            }

                            shapeEntry.wrapMode = ((int)GetWrapMode(sampler.ClampX),
                                    (int)GetWrapMode(sampler.ClampY));
                        }
                        else
                        {
                            shapeEntry.texture = -1;
                        }

                        switch (mdl.Materials[shape.MaterialIndex].RenderState.FlagsMode)
                        {
                            case RenderStateFlagsMode.AlphaMask:
                            case RenderStateFlagsMode.Translucent:
                            case RenderStateFlagsMode.Custom:
                                shapeEntry.pass = Pass.TRANSPARENT;
                                break;
                            default:
                                shapeEntry.pass = Pass.OPAQUE;
                                break;
                        }

                        bool usesShadow = mdl.Materials[shape.MaterialIndex].ShaderAssign.ShaderOptions.ContainsKey("var_shadow") &&
                            mdl.Materials[shape.MaterialIndex].ShaderAssign.ShaderOptions["var_shadow"].String == "1";

                        //Create a buffer instance which stores all the buffer data
                        VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shapeIndex], ByteOrder.BigEndian);

                        //Set each array first from the lib if exist. Then add the data all in one loop
                        Syroot.Maths.Vector4F[] vec4Positions = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4Normals = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4uv0 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4uv1 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4uv2 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4c0 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4t0 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4b0 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4w0 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4i0 = new Syroot.Maths.Vector4F[0];

                        //For shape morphing
                        Syroot.Maths.Vector4F[] vec4Positions1 = new Syroot.Maths.Vector4F[0];
                        Syroot.Maths.Vector4F[] vec4Positions2 = new Syroot.Maths.Vector4F[0];

                        foreach (VertexAttrib att in mdl.VertexBuffers[shapeIndex].Attributes.Values)
                        {
                            if (att.Name == "_p0")
                                vec4Positions = helper["_p0"].Data;
                            if (att.Name == "_n0")
                                vec4Normals = helper["_n0"].Data;
                            if (att.Name == "_u0")
                                vec4uv0 = helper["_u0"].Data;
                            if (att.Name == "_u1")
                                vec4uv1 = helper["_u1"].Data;
                            if (att.Name == "_u2")
                                vec4uv2 = helper["_u2"].Data;
                            if (att.Name == "_c0")
                                vec4c0 = helper["_c0"].Data;
                            if (att.Name == "_t0")
                                vec4t0 = helper["_t0"].Data;
                            if (att.Name == "_b0")
                                vec4b0 = helper["_b0"].Data;
                            if (att.Name == "_w0")
                                vec4w0 = helper["_w0"].Data;
                            if (att.Name == "_i0")
                                vec4i0 = helper["_i0"].Data;

                            if (att.Name == "_p1")
                                vec4Positions1 = helper["_p1"].Data;
                            if (att.Name == "_p2")
                                vec4Positions2 = helper["_p2"].Data;
                        }

                        shapeEntry.indexBufferLength = indices.Length;

                        float[] bufferData = new float[6 * vec4Positions.Length];

                        int _i = 0;
                        for (int i = 0; i < vec4Positions.Length; i++)
                        {
                            Vector3 pos = Vector3.Zero;
                            Vector3 pos1 = Vector3.Zero;
                            Vector3 pos2 = Vector3.Zero;
                            Vector3 nrm = Vector3.Zero;
                            Vector2 uv0 = Vector2.Zero;
                            Vector2 uv1 = Vector2.Zero;
                            Vector2 uv2 = Vector2.Zero;
                            List<float> boneWeights = new List<float>();
                            List<int> boneIds = new List<int>();
                            Vector4 tan = Vector4.Zero;
                            Vector4 bitan = Vector4.Zero;
                            Vector4 col = Vector4.One;

                            if (vec4Positions.Length > 0)
                                pos = new Vector3(vec4Positions[i].X, vec4Positions[i].Y, vec4Positions[i].Z);
                            if (vec4Positions1.Length > 0)
                                pos1 = new Vector3(vec4Positions1[i].X, vec4Positions1[i].Y, vec4Positions1[i].Z);
                            if (vec4Positions2.Length > 0)
                                pos2 = new Vector3(vec4Positions2[i].X, vec4Positions2[i].Y, vec4Positions2[i].Z);
                            if (vec4Normals.Length > 0)
                                nrm = new Vector3(vec4Normals[i].X, vec4Normals[i].Y, vec4Normals[i].Z);
                            if (vec4uv0.Length > 0)
                                uv0 = new Vector2(vec4uv0[i].X, vec4uv0[i].Y);
                            if (vec4uv1.Length > 0)
                                uv1 = new Vector2(vec4uv1[i].X, vec4uv1[i].Y);
                            if (vec4uv2.Length > 0)
                                uv2 = new Vector2(vec4uv2[i].X, vec4uv2[i].Y);
                            if (vec4w0.Length > 0)
                            {
                                boneWeights.Add(vec4w0[i].X);
                                boneWeights.Add(vec4w0[i].Y);
                                boneWeights.Add(vec4w0[i].Z);
                                boneWeights.Add(vec4w0[i].W);
                            }
                            if (vec4i0.Length > 0)
                            {
                                boneIds.Add((int)vec4i0[i].X);
                                boneIds.Add((int)vec4i0[i].Y);
                                boneIds.Add((int)vec4i0[i].Z);
                                boneIds.Add((int)vec4i0[i].W);

                            }

                            if (vec4t0.Length > 0)
                                tan = new Vector4(vec4t0[i].X, vec4t0[i].Y, vec4t0[i].Z, vec4t0[i].W);
                            if (vec4b0.Length > 0)
                                bitan = new Vector4(vec4b0[i].X, vec4b0[i].Y, vec4b0[i].Z, vec4b0[i].W);
                            if (vec4c0.Length > 0)
                                col = new Vector4(vec4c0[i].X, vec4c0[i].Y, vec4c0[i].Z, vec4c0[i].W);

                            if (shape.VertexSkinCount == 1)
                            {
                                int boneIndex = shape.BoneIndex;
                                if (boneIds.Count > 0)
                                    boneIndex = mdl.Skeleton.MatrixToBoneList[boneIds[0]];

                                //Check if the bones are a rigid type
                                //In game it seems to not transform if they are not rigid
                                if (mdl.Skeleton.Bones[boneIndex].RigidMatrixIndex != -1)
                                {
                                    shapeEntry.skinBoneIndices = shape.SkinBoneIndices.ToArray();
                                    //Matrix4 sb = transforms[boneIndex];
                                    //pos = Vector3.TransformPosition(pos, sb);
                                    //nrm = Vector3.TransformNormal(nrm, sb);
                                }
                            }

                            if (shape.VertexSkinCount == 0)
                            {
                                Bone bone = mdl.Skeleton.Bones[shape.BoneIndex];

                                shapeEntry.ridgetBoneIndex = shape.BoneIndex;

                                //if (bone.ParentIndex==-1)
                                //    boneParentMatrices[totalShapeIndex] = Matrix4.Identity;
                                //else
                                //    boneParentMatrices[totalShapeIndex] = transforms[bone.ParentIndex];

                                //boneRefs[totalShapeIndex] = bone;
                            }
                            bufferData[_i] =     pos.X;
                            bufferData[_i + 1] = pos.Y;
                            bufferData[_i + 2] = pos.Z;
                            if (vec4uv0.Length > 0)
                            {
                                bufferData[_i + 3] = uv0.X;
                                bufferData[_i + 4] = uv0.Y;
                            }
                            if (vec4c0.Length > 0 && !usesShadow)
                            {
                                bufferData[_i + 5] = BitConverter.ToSingle(new byte[]{
                            (byte)(col.X * 255),
                            (byte)(col.Y * 255),
                            (byte)(col.Z * 255),
                            (byte)(col.W * 255)}, 0);
                            }
                            else
                                bufferData[_i + 5] = white;
                            _i += 6;
                        }
                        int[] buffers = new int[2];
                        GL.GenBuffers(2, buffers);

                        int indexBuffer = buffers[0];
                        int vaoBuffer = buffers[1];

                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

                        GL.BindBuffer(BufferTarget.ArrayBuffer, vaoBuffer);
                        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * vec4Positions.Length, bufferData, BufferUsageHint.StaticDraw);

                        shapeEntry.vao = new VertexArrayObject(vaoBuffer, indexBuffer);
                        shapeEntry.vao.AddAttribute(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 6, 0);
                        shapeEntry.vao.AddAttribute(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 6, sizeof(float) * 3);
                        shapeEntry.vao.AddAttribute(2, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float) * 6, sizeof(float) * 5);

                        shapeEntry.vao.Initialize(control);

                        shapeEntries[shapeIndex] = shapeEntry;

                        shapeIndex++;
                    }

                    Matrix4[] matrices = new Matrix4[mdl.Skeleton.Bones.Count];

                    CalculateMatrices(mdl.Skeleton.Bones, ref matrices);

                    modelEntries[modelIndex] = new ModelEntry {
                        shapeEntries = shapeEntries,
                        bones = mdl.Skeleton.Bones,
                        matrices = matrices,
                        name = mdl.Name
                    };

                    modelIndex++;
                }
            }

            static void CalculateMatrices(ResDict<Bone> bones, ref Matrix4[] matrices)
            {
                for(int i = 0; i<bones.Count; i++)
                {
                    Bone bone = bones[i];
                    matrices[i] = Matrix4.CreateScale(new Vector3(bone.Scale.X, bone.Scale.Y, bone.Scale.Z));
                    
                    while(true)
                    {
                        matrices[i] = matrices[i] * 
                              Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z)) *
                              Matrix4.CreateTranslation(new Vector3(bone.Position.X, bone.Position.Y, bone.Position.Z));

                        if (bone.ParentIndex == -1)
                            break;
                        bone = bones[bone.ParentIndex];
                    }
                }
            }

            public void Draw(GL_ControlModern control, Pass pass, Vector4 highlightColor)
            {
                Matrix4 mdlMtx = control.ModelMatrix;

                if (pass == Pass.PICKING)
                {
                    control.CurrentShader = Renderers.ColorBlockRenderer.SolidColorShaderProgram;
                    control.CurrentShader.SetVector4("color", control.NextPickingColor());
                }
                else
                {
                    if(pass == Pass.TRANSPARENT)
                    {
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.Enable(EnableCap.Blend);
                    }

                    control.CurrentShader = BfresShaderProgram;

                    control.CurrentShader.SetVector4("highlight_color", highlightColor);

                    GL.ActiveTexture(TextureUnit.Texture0);
                }

                for (int modelIndex = 0; modelIndex < modelEntries.Length; modelIndex++)
                {
                    var mdlEntry = modelEntries[modelIndex];

                    for (int shapeIndex = 0; shapeIndex < mdlEntry.shapeEntries.Length; shapeIndex++)
                    {
                        var shapeEntry = mdlEntry.shapeEntries[shapeIndex];

                        control.UpdateModelMatrix(mdlEntry.matrices[shapeEntry.ridgetBoneIndex] * Matrix4.CreateScale(0.01f) * mdlMtx);

                        if (pass == shapeEntry.pass)
                        {
                            if (shapeEntry.texture == -1)
                                GL.BindTexture(TextureTarget.Texture2D, NoTetxure);
                            else if (shapeEntry.texture == -2)
                                GL.BindTexture(TextureTarget.Texture2D, DefaultTetxure);
                            else
                                GL.BindTexture(TextureTarget.Texture2D, shapeEntry.texture);

                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, shapeEntry.wrapMode.Item1);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, shapeEntry.wrapMode.Item2);
                        }

                        if (pass == shapeEntry.pass || pass == Pass.PICKING)
                        {
                            shapeEntry.vao.Use(control);

                            GL.DrawElements(PrimitiveType.Triangles, shapeEntry.indexBufferLength, DrawElementsType.UnsignedInt, 0);
                        }
                    }
                }

                control.UpdateModelMatrix(mdlMtx);

                GL.Disable(EnableCap.Blend);
            }
        }
        
        private static void GetPixelFormats(GX2SurfaceFormat Format, out PixelInternalFormat pixelInternalFormat)
        {
            switch (Format)
            {
                case GX2SurfaceFormat.T_BC1_UNorm:
                    pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case GX2SurfaceFormat.T_BC1_SRGB:
                    pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case GX2SurfaceFormat.T_BC2_UNorm:
                    pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case GX2SurfaceFormat.T_BC2_SRGB:
                    pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case GX2SurfaceFormat.T_BC3_UNorm:
                    pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case GX2SurfaceFormat.T_BC3_SRGB:
                    pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case GX2SurfaceFormat.T_BC4_UNorm:
                    pixelInternalFormat = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case GX2SurfaceFormat.T_BC4_SNorm:
                    pixelInternalFormat = PixelInternalFormat.CompressedSignedRedRgtc1;
                    break;
                case GX2SurfaceFormat.T_BC5_UNorm:
                    pixelInternalFormat = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                case GX2SurfaceFormat.T_BC5_SNorm:
                    pixelInternalFormat = PixelInternalFormat.CompressedSignedRgRgtc2;
                    break;
                case GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNorm:
                    pixelInternalFormat = PixelInternalFormat.Rgba;
                    break;
                case GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB:
                    pixelInternalFormat = PixelInternalFormat.Rgba;
                    break;
                default:
                    pixelInternalFormat = PixelInternalFormat.Rgba;
                    break;
            }
        }

        private static TextureWrapMode GetWrapMode(GX2TexClamp texClamp)
        {
            switch (texClamp)
            {
                case GX2TexClamp.Clamp:
                    return TextureWrapMode.Clamp;
                case GX2TexClamp.ClampBorder:
                    return TextureWrapMode.ClampToBorder;
                case GX2TexClamp.Mirror:
                    return TextureWrapMode.MirroredRepeat;
                default:
                    return TextureWrapMode.Repeat;
            }
        }

        /// <summary>
        /// Uploads a texture to the OpenGL texture units
        /// </summary>
        /// <param name="texture">Texture to upload data from</param>
        /// <returns>Integer ID of the uploaded texture</returns>
        private static int UploadTexture(Texture texture)
        {
            #region Deswizzle
            uint bpp = GX2.surfaceGetBitsPerPixel((uint)texture.Format) >> 3;

            GX2.GX2Surface surf = new GX2.GX2Surface
            {
                bpp = bpp,
                height = texture.Height,
                width = texture.Width,
                aa = (uint)texture.AAMode,
                alignment = texture.Alignment,
                depth = texture.Depth,
                dim = (uint)texture.Dim,
                format = (uint)texture.Format,
                use = (uint)texture.Use,
                pitch = texture.Pitch,
                data = texture.Data,
                numMips = texture.MipCount,
                mipOffset = texture.MipOffsets,
                mipData = texture.MipData,
                tileMode = (uint)texture.TileMode,
                swizzle = texture.Swizzle,
                numArray = texture.ArrayLength
            };
            
            if (surf.mipData == null)
                surf.numMips = 1;

            byte[] deswizzled = GX2.Decode(surf, 0, 0);


            #endregion
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);


            if (texture.Format == GX2SurfaceFormat.T_BC4_UNorm)
            {
                deswizzled = DDSCompressor.DecompressBC4(deswizzled, (int)texture.Width, (int)texture.Height, false).Data;
            }
            else if (texture.Format == GX2SurfaceFormat.T_BC4_SNorm)
            {
                deswizzled = DDSCompressor.DecompressBC4(deswizzled, (int)texture.Width, (int)texture.Height, true).Data;
            }
            else if (texture.Format == GX2SurfaceFormat.T_BC5_UNorm)
            {
                deswizzled = DDSCompressor.DecompressBC5(deswizzled, (int)texture.Width, (int)texture.Height, false, true);
            }
            else if (texture.Format == GX2SurfaceFormat.T_BC5_SNorm)
            {
                deswizzled = DDSCompressor.DecompressBC5(deswizzled, (int)texture.Width, (int)texture.Height, true, true);
            }
            else
            {
                GetPixelFormats(texture.Format, out PixelInternalFormat internalFormat);
                
                if (internalFormat != PixelInternalFormat.Rgba)
                {
                    GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, (InternalFormat)internalFormat, (int)texture.Width, (int)texture.Height, 0, deswizzled.Length, deswizzled);

                    goto DATA_UPLOADED;
                }
            }

            #region channel reassign

            byte[] sources = new byte[] { 0, 0, 0, 0, 0, 0xFF };

            for (int i = 0; i < deswizzled.Length; i += 4)
            {
                sources[0] = deswizzled[i];
                sources[1] = deswizzled[i + 1];
                sources[2] = deswizzled[i + 2];
                sources[3] = deswizzled[i + 3];

                deswizzled[i] = sources[(int)texture.CompSelR];
                deswizzled[i + 1] = sources[(int)texture.CompSelG];
                deswizzled[i + 2] = sources[(int)texture.CompSelB];
                deswizzled[i + 3] = sources[(int)texture.CompSelA];
                //deswizzled[i + 3] = 0xFF;
            }
            #endregion

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)texture.Width, (int)texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, deswizzled);

        DATA_UPLOADED:

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return tex;
        }
    }
}
