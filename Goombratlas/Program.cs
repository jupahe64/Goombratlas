using GL_EditorFramework.GL_Core;
using OpenTK;
using SARCExt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Goombratlas
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new WorldmapEditorForm());
        }

        private static string gamePath;

        public static string GamePath { 
            get => gamePath;
            set
            {
                string fileName = value + "\\content\\Common\\actor\\cobPack.szs";

                if (File.Exists(fileName))
                    cobPack = SARC.UnpackRamN(Yaz0.Decompress(File.ReadAllBytes(fileName))).Files;

                gamePath = value;
            }
        }

        private static Dictionary<string, byte[]> cobPack;

        public static void TrySubmitCobModel(string modelName, GL_ControlModern control)
        {
            if (GamePath == null)
            {
                Debugger.Break();
                return;
            }

            string fileName = $"{GamePath}\\content\\Common\\actor\\{modelName}.szs";
            if (cobPack!=null && cobPack.TryGetValue(modelName, out byte[] data))
            {
                BfresModelCache.Submit(
                    modelName,
                    new Syroot.NintenTools.Bfres.ResFile(new MemoryStream(
                        SARC.UnpackRamN(data).Files[modelName + ".bfres"]
                        )),
                    control
                    );
            }
            else if(File.Exists(fileName))
            {
                BfresModelCache.Submit(
                    modelName,
                    new Syroot.NintenTools.Bfres.ResFile(new MemoryStream(
                        SARC.UnpackRamN(Yaz0.Decompress(File.ReadAllBytes(fileName))).Files[modelName + ".bfres"]
                        )),
                    control
                    );
            }
        }
    }
}
