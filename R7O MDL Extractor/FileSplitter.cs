using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace R7O_MDL_Extractor
{
    public class FileSplitter
    {
        public static List<string> SplitFile(string splitStr)
        {
            List<string> paths = new List<string>();
            byte[] buffer;
            string filename;
            // Open dialog for user to supply a file.
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }
                filename = openFileDialog.FileName;
            }
            // get all data from file.
            buffer = File.ReadAllBytes(filename);
            int index = 0;
            int mdlStart = int.MinValue;
            int mdlEnd;
            while ((index = Helpers.Search(buffer, Encoding.ASCII.GetBytes(splitStr), index)) != -1)
            {
                mdlEnd = index;
                if (mdlStart != int.MinValue)
                {
                    // Extract each sub model to a new file.
                    string path = filename + "_parts/submdl_0x" + mdlStart.ToString("X8") + "." + splitStr;
                    SaveFile(buffer, filename, mdlStart, mdlEnd, path);
                    paths.Add(path);
                }
                else
                {
                    // head of file, likely not needed, don't add to paths.
                    SaveFile(buffer, filename, 0, index, filename + "_parts/header."+ Encoding.ASCII.GetString(buffer.Take(3).ToArray()));
                }
                mdlStart = index;
                index += 3;
            }
            if (mdlStart != int.MinValue)
            {
                // get whatever submodel is at the end.
                string path = filename + "_parts/submdl_0x" + mdlStart.ToString("X8") + "." + splitStr;

                SaveFile(buffer, filename, mdlStart, buffer.Length, path);
                paths.Add(path);
            }
            return paths;
        }

        private static void SaveFile(byte[] buffer, string filename, int mdlStart, int mdlEnd, string path)
        {
            byte[] subMdlBuffer = new byte[mdlEnd - mdlStart];
            Buffer.BlockCopy(buffer, mdlStart, subMdlBuffer, 0, mdlEnd - mdlStart);
            Directory.CreateDirectory(filename + "_parts/");
            File.WriteAllBytes(path, subMdlBuffer);
        }
    }
}
