using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JeremyAnsel.Media.WavefrontObj;

namespace R7O_MDL_Extractor
{
    static class Program
    {
        private static readonly Dictionary<byte[], MeshPattern> patterns = new Dictionary<byte[], MeshPattern>
        {
            {
                new byte[]{ 0x01, 0x00, 0x02 }, new MeshPattern()
                {
                    HalfFloat = false,
                    stride = 20,
                    VertexColors = false,
                    Normals = false,
                    NormalWeights = false,
                    UVs = true,
                    Bones = false,
                    bytepattern = "vxvyvzuxuy"
                }
            },
            {
                new byte[]{ 0x01, 0x24, 0x02 }, new MeshPattern()
                {
                    HalfFloat = false,
                    stride = 44,
                    VertexColors = false,
                    Normals = true,
                    NormalWeights = true,
                    UVs = true,
                    Bones = false,
                    bytepattern = "vxvyvznxnynzwxwywzuxuy"
                }
            },
            {
                new byte[]{ 0x00, 0x06, 0x03 }, new MeshPattern()
                {
                    HalfFloat = true,
                    stride = 12,
                    VertexColors = false,
                    Normals = true,
                    NormalWeights = false,
                    UVs = false,
                    Bones = false,
                    bytepattern = "vxvyvznxnynz"
                }
            },
            {
                new byte[]{ 0x01, 0x80, 0x03 }, new MeshPattern()
                {
                    HalfFloat = true,
                    stride = 10,
                    VertexColors = false,
                    Normals = false,
                    NormalWeights = false,
                    UVs = true,
                    Bones = false,
                    bytepattern = "vxvyvzuxuy"
                }
            },
            {
                new byte[]{ 0x01, 0x86, 0x03 }, new MeshPattern()
                {
                    HalfFloat = true,
                    stride = 16,
                    VertexColors = false,
                    Normals = true,
                    NormalWeights = false,
                    UVs = true,
                    Bones = false,
                    bytepattern = "vxvyvznxnynzuxuy"
                }
            },
            {
                new byte[]{ 0x01, 0x86, 0x23 }, new MeshPattern()
                {
                    HalfFloat = true,
                    stride = 20,
                    VertexColors = true,
                    Normals = true,
                    NormalWeights = false,
                    UVs = true,
                    Bones = false,
                    bytepattern = "vxvyvzRGBAnxnynzuxuy"
                }
            },
            {
                new byte[]{ 0x01, 0xb6, 0x03 }, new MeshPattern()
                {
                    HalfFloat = true,
                    stride = 22,
                    VertexColors = false,
                    Normals = true,
                    NormalWeights = true,
                    UVs = true,
                    Bones = false,
                    bytepattern = "vxvyvznxnynzwxwywzuxuy"
                }
            },
            {
                new byte[]{ 0x81, 0x86, 0x23 }, new MeshPattern()
                {
                    HalfFloat = true,
                    stride = 28,
                    VertexColors = true,
                    Normals = true,
                    NormalWeights = false,
                    UVs = true,
                    Bones = true,
                    bytepattern = "vxvyvzRGBAnxnynzuxuybbbbbbbb"
                }
            },
        };
        

        //Application.EnableVisualStyles();
        //Application.SetCompatibleTextRenderingDefault(false);
        //Application.Run(new Form1());
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Select File to Extract from:");
            List<string> paths = FileSplitter.SplitFile("R7O");
            Console.WriteLine("Converting to OBJ's:");
            foreach (var path in paths/*.Where(s => s.Contains("43080"))*/)
            {
                byte[] buffer = File.ReadAllBytes(path);
                foreach (var pattern in patterns)
                {
                    try
                    {
                        ObjFile obj = new ObjFile();
                        int index = Helpers.Search(buffer, pattern.Key, 3);
                        if (index == -1)
                        {
                            continue;
                        }

                        int length = BitConverter.ToInt32(buffer.Skip(index+3).Take(4).Reverse().ToArray(), 0);
                        if (length < 0)
                        {
                            continue;
                        }
                        index += 7;
                        int offset = 0;
                        for (int i = 0; i < length; i++)
                        {
                            offset = index + i * pattern.Value.stride;
                            float vx, vy, vz, A, R, G, B, nx, ny, nz, wx, wy, wz, ux, uy;
                            int b1, b2;
                        
                            if (pattern.Value.HalfFloat)
                            {
                                vx = Helpers.toTwoByteFloat(buffer[offset  ], buffer[offset + 1]);
                                vy = Helpers.toTwoByteFloat(buffer[offset+2], buffer[offset + 3]);
                                vz = Helpers.toTwoByteFloat(buffer[offset+4], buffer[offset + 5]);
                                offset += 6;
                            }
                            else
                            {
                                vx = BitConverter.ToSingle(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
                                vy = BitConverter.ToSingle(buffer.Skip(offset+4).Take(4).Reverse().ToArray(), 0);
                                vz = BitConverter.ToSingle(buffer.Skip(offset+8).Take(4).Reverse().ToArray(), 0);
                                offset += 12;
                            }
                            if (pattern.Value.VertexColors)
                            {
                                R = buffer[offset] / 255.0F;
                                G = buffer[offset+1] / 255.0F;
                                B = buffer[offset+2] / 255.0F;
                                //A = buffer[offset+3] / 255.0F;
                                obj.Vertices.Add(new ObjVertex(vx, vy, vz, R, G, B));
                                offset += 4;
                            }
                            else obj.Vertices.Add(new ObjVertex(vx, vy, vz));
                            if (pattern.Value.Normals)
                            {
                                if (pattern.Value.HalfFloat)
                                {
                                    nx = Helpers.toTwoByteFloat(buffer[offset], buffer[offset + 1]);
                                    ny = Helpers.toTwoByteFloat(buffer[offset + 2], buffer[offset + 3]);
                                    nz = Helpers.toTwoByteFloat(buffer[offset + 4], buffer[offset + 5]);
                                    offset += 6;
                                }
                                else
                                {
                                    nx = BitConverter.ToSingle(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
                                    ny = BitConverter.ToSingle(buffer.Skip(offset + 4).Take(4).Reverse().ToArray(), 0);
                                    nz = BitConverter.ToSingle(buffer.Skip(offset + 8).Take(4).Reverse().ToArray(), 0);
                                    offset += 12;
                                }
                                obj.VertexNormals.Add(new ObjVector3(nx, ny, nz));
                            }
                            if (pattern.Value.NormalWeights)
                            {
                                if (pattern.Value.HalfFloat)
                                {
                                    //wx = Helpers.toTwoByteFloat(buffer[offset], buffer[offset + 1]);
                                    //wy = Helpers.toTwoByteFloat(buffer[offset + 2], buffer[offset + 3]);
                                    //wz = Helpers.toTwoByteFloat(buffer[offset + 4], buffer[offset + 5]);
                                    offset += 6;
                                }
                                else
                                {
                                    //wx = BitConverter.ToSingle(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
                                    //wy = BitConverter.ToSingle(buffer.Skip(offset + 4).Take(4).Reverse().ToArray(), 0);
                                    //wz = BitConverter.ToSingle(buffer.Skip(offset + 8).Take(4).Reverse().ToArray(), 0);
                                    offset += 12;
                                }
                                //obj.VertexNormals.Add(new ObjVector3(wx, wy, wz));
                            }
                            if (pattern.Value.UVs)
                            {
                                if (pattern.Value.HalfFloat)
                                {
                                    ux = Helpers.toTwoByteFloat(buffer[offset], buffer[offset + 1]);
                                    uy = Helpers.toTwoByteFloat(buffer[offset + 2], buffer[offset + 3]);
                                    offset += 4;
                                }
                                else
                                {
                                    ux = BitConverter.ToSingle(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
                                    uy = BitConverter.ToSingle(buffer.Skip(offset + 4).Take(4).Reverse().ToArray(), 0);
                                    offset += 8;
                                }
                                obj.TextureVertices.Add(new ObjVector3(ux, uy, 0));
                            }
                            if (pattern.Value.Bones)
                            {
                                //b1 = BitConverter.ToInt32(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
                                //b2 = BitConverter.ToInt32(buffer.Skip(offset + 4).Take(4).Reverse().ToArray(), 0);
                                offset += 8;
                                //obj.Bones.Add(new ObjVector3(b1, b2, 0));
                            }
                        }
                        int length2 = BitConverter.ToInt32(buffer.Skip(offset + 12).Take(4).Reverse().ToArray(), 0);
                        List<List<int>> tStrips = new List<List<int>>();
                        List<int> tStrip = new List<int>();
                        offset += 16;
                        for (int i = 0; i < 2*length2-1; i+=2)
                        {
                            int v = (buffer[offset + i] << 8) | buffer[offset + 1 + i];
                            if (v == 0xFFFF)
                            {
                                tStrips.Add(tStrip);
                                tStrip = new List<int>();
                                continue;
                            }
                            tStrip.Add(v);
                        }
                        for (int j = 0; j < tStrips.Count; j++)
                        {
                            //Console.WriteLine(tStrips[j].Select(ts => (ts + 1).ToString()).Aggregate((ts, ts2) => { ts += " "+ts2; return ts; } ));
                            for (int i = 2; i < tStrips[j].Count; i++)
                            {
                                ObjFace face = new ObjFace();
                                int a = 1 + tStrips[j][i - 2];
                                int b = 1 + tStrips[j][(i % 2 == 1) ? i : (i - 1)];
                                int c = 1 + tStrips[j][(i % 2 == 1) ? (i - 1) : i];
                                face.Vertices.Add(new ObjTriplet(a, a, a));
                                face.Vertices.Add(new ObjTriplet(b, b, b));
                                face.Vertices.Add(new ObjTriplet(c, c, c));
                                obj.Faces.Add(face);
                            }
                        }


                        obj.WriteTo(path.Replace("R7O", "obj"));
                        Console.WriteLine("Created: " + path.Replace("R7O", "obj"));
                        string patternStr = pattern.Key.Select(k => (k + 1).ToString()).Aggregate((k, k2) => { k += " " + k2; return k; });
                        Console.WriteLine("Pattern Used: {0} V: {1} F: {2}", patternStr, obj.Vertices.Count, obj.Faces.Count);
                        break;

                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            Console.WriteLine("Job Complete.");
            Console.ReadLine();
        }
        public struct MeshPattern
        {
            public bool HalfFloat;
            public int stride;
            public bool VertexColors;
            public bool Normals;
            public bool NormalWeights;
            public bool UVs;
            public bool Bones;
            public string bytepattern;
        }

    }
}
