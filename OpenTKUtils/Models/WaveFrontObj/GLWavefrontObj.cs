using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils
{
    // wavefront object, pointing to vertices (may be shared with other objects) 
    // Indicies, and containing meta data like materials

    public class GLWaveformObject
    {
        public enum ObjectType { Unassigned, Polygon };
        public GLMeshVertices Vertices { get; set; }
        public GLMeshIndices Indices { get; set; }

        public string Material { get; set; }
        public string Groupname { get; set; }
        public string Objectname { get; set; }
        public string MatLibname { get; set; }

        public ObjectType Objecttype { get; set; }

        public GLWaveformObject(ObjectType t, GLMeshVertices vert, string matl)
        {
            Objecttype = t;
            Vertices = vert;
            MatLibname = matl;
            Indices = new GLMeshIndices();
        }
    }
}
