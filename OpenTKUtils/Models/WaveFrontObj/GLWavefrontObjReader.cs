using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils
{
    public class GLWaveformObjReader
    {
        // wavefront object format, not exactly well documented..
        // wavefront data : vertex's are ordered 1+
        // co-ordinate system is right handled - +x to right, +y upwards, +z towards you
        // to compensate for opengl, with +x to right, +y upwards, +z away, z co-ordinates in normals/vertexes are inverted

        public List<GLWaveformObject> ReadOBJFile(string path)      // throws exceptions
        {
            string text = null;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception)
            {
                return null;
            }

            return ReadOBJData(text);
        }

        private GLWaveformObject Create(bool cond)
        {
            if (cond)
            {
                GLWaveformObject cur = new GLWaveformObject(GLWaveformObject.ObjectType.Unassigned, reader_vertices, reader_matlib);
                reader_objects.Add(cur);
                return cur;
            }
            else
                return reader_objects.Last();
        }

        public List<GLWaveformObject> ReadOBJData(string data, bool correctzforopengl = true)          // throws exceptions
        {
            reader_vertices = new GLMeshVertices();
            reader_objects = new List<GLWaveformObject>();
            reader_current = null;
            reader_matlib = "Not set";

            float zcorr = correctzforopengl ? -1 : 1;

            using (TextReader reader = new StringReader(data))
            {
                string line;
                while( (line = reader.ReadLine()) != null )
                {
                    line = line.Trim();

                    if ( line.HasChars())
                    {
                        List<string> words = new List<string>();
                        foreach( var w in line.Split(' ', '\t'))
                        {
                            if (w.HasChars())
                                words.Add(w);
                        }

                        string type = words[0].ToLower();

                        line = line.Substring(words[0].Length).Trim();

                        words.RemoveAt(0);


                        System.Diagnostics.Debug.WriteLine("Read " + line);

                        if (type.StartsWith("#"))
                        {
                            // comment
                        }
                        else if (type == "end")     //Rob addition!
                        {
                            break;
                        }
                        else if (type == "v")
                        {
                            if (words.Count >= 3)
                            {
                                reader_vertices.Vertices.Add(new Vector4(float.Parse(words[0]), float.Parse(words[1]),
                                                    zcorr * float.Parse(words[2]), words.Count < 4 ? 1 : float.Parse(words[3])));
                            }
                        }
                        else if (type == "vt")
                        {
                            if (words.Count >= 2)
                            {
                                reader_vertices.TextureVertices.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]),
                                                            words.Count < 3 ? 0 : float.Parse(words[2])));
                            }
                        }
                        else if (type == "vn")
                        {
                            if (words.Count >= 2)
                                reader_vertices.Normals.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), zcorr * float.Parse(words[2])));
                        }
                        else if (type == "vp")
                        {
                            throw new Exception("Not implemented:" + type);
                        }

                        else if (type == "deg")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "bmat")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "step")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "cstype")
                        {
                            throw new Exception("Not implemented:" + type);
                        }

                        else if (type == "f")
                        {
                            reader_current = Create(reader_current == null || (reader_current.Objecttype != GLWaveformObject.ObjectType.Polygon && reader_current.Objecttype != GLWaveformObject.ObjectType.Unassigned));
                            reader_current.Objecttype = GLWaveformObject.ObjectType.Polygon;

                            // tbd neg
                            foreach (string w in words)
                            {
                                string[] comps = w.Split('/');

                                int ti = comps.Length > 1 ? (comps[1].InvariantParseInt(int.MinValue)) : int.MinValue;

                                if (ti != int.MinValue)
                                {
                                    if (reader_current.Indices.VertexIndices.Count != reader_current.Indices.TextureIndices.Count)
                                        throw new Exception("New texture index but previous was missing them");

                                    if (ti < 0)
                                            ti = reader_vertices.TextureVertices.Count + ti;
                                        else if (ti >= 1)
                                            ti--;

                                    ti = Math.Min(Math.Max(ti, 0), reader_vertices.TextureVertices.Count - 1);
                                    reader_current.Indices.TextureIndices.Add((uint)ti);
                                }

                                int ni = comps.Length > 2 ? (comps[2].InvariantParseInt(int.MinValue)) : int.MinValue;

                                if ( ni != int.MinValue )
                                {
                                    if (reader_current.Indices.VertexIndices.Count != reader_current.Indices.NormalIndices.Count)
                                        throw new Exception("New texture index but previous was missing them");

                                    if (ni < 0)
                                        ni = reader_vertices.Normals.Count + ni;
                                    else if (ni >= 1)
                                        ni--;

                                    ni = Math.Min(Math.Max(ni, 0), reader_vertices.Normals.Count - 1);
                                    reader_current.Indices.NormalIndices.Add((uint)ni);
                                }

                                int vi = comps[0].InvariantParseInt(int.MinValue);

                                if (vi != int.MinValue)
                                {
                                    if (vi < 0)
                                        vi = reader_vertices.Vertices.Count + vi;
                                    else if (vi >= 1)
                                        vi--;

                                    vi = Math.Min(Math.Max(vi, 0), reader_vertices.Vertices.Count - 1);
                                    reader_current.Indices.VertexIndices.Add((uint)vi);
                                }

                            }
                        }
                        else if (type == "p")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "l")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "curv")    // curve http://paulbourke.net/dataformats/obj/
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "curv2")   // 2d curve
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "surf")   // surface
                        {
                            throw new Exception("Not implemented:" + type);
                        }

                        else if (type == "parm")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "trim")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "hole")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "scrv")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "sp")
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "end")
                        {
                            throw new Exception("Not implemented:" + type);
                        }

                        else if (type == "con")
                        {
                            throw new Exception("Not implemented:" + type);
                        }

                        else if (type == "mtllib")
                        {
                            reader_matlib = line;
                            if (reader_current != null)
                                reader_current.MatLibname = reader_matlib;
                        }
                        else if (type == "usemtl")
                        {
                            reader_current = Create(reader_current == null || reader_current.Objecttype != GLWaveformObject.ObjectType.Unassigned);
                            reader_current.Material = line;
                        }
                        else if (type == "g")
                        {
                            reader_current = Create(reader_current == null || reader_current.Objecttype != GLWaveformObject.ObjectType.Unassigned);
                            reader_current.Groupname = line;
                        }
                        else if (type == "s") // smoothing group
                        {
                            // ignore smoothing groups..
                            //throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "mg") // merging group
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "o") // object name
                        {
                            reader_current = Create(reader_current == null || reader_current.Objecttype != GLWaveformObject.ObjectType.Unassigned);
                            reader_current.Objectname = line;
                        }

                        else if (type == "bevel") // Bevel interpolation
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "c_interp") //   Color interpolation
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "d_interp") // Dissolve interpolation
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "lod") //   Level of detail
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "shadow_obj") //  Shadow casting
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "trace_obj") //Ray tracing
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "ctech") //  Curve approximation technique
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else if (type == "stech") //  Surface approximation technique
                        {
                            throw new Exception("Not implemented:" + type);
                        }
                        else
                        {
                            throw new Exception("Not implemented:" + type);
                        }

                    }
                }
            }

            return reader_objects;
        }

        private GLMeshVertices reader_vertices;
        private List<GLWaveformObject> reader_objects;
        private GLWaveformObject reader_current;
        private string reader_matlib;

    }
}
