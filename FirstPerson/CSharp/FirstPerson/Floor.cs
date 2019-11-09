using System.Collections.Generic;
using System.Drawing;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
using PointF = System.Drawing.PointF;

namespace WpfApplication1
{
    class Floor : Mesh
    {

        private float width  = 16;
        private float height = 16;
        private float tile_S = 8;
        private float tile_T = 8;

        private TextureBase floorTexture;
        private TextureBase lightMapTexture;        

        public bool multiTexture;

        public Floor(bool multitexture) : base(4, 2, Mesh.natureType.RichPlain)
        {
            this.multiTexture = multitexture;

            Vertices[0] = new Point3D(-width*0.5, height*0.5);
            Vertices[1] = new Point3D(width*0.5, height*0.5);
            Vertices[2] = new Point3D(width*0.5, -height*0.5);
            Vertices[3] = new Point3D(-width*0.5, -height*0.5);

            Triangles[0] = new IndexTriangle(0,1,2);
            Triangles[1] = new IndexTriangle(0,2,3);
        }

        public override void Dispose()
        {
            base.Dispose();
            
            floorTexture.Dispose();
            lightMapTexture.Dispose();            
        }

        private const string Textures = "../../../../../../dataset/Assets/Textures/";

        public override void Compile(CompileParams data)
        {
            base.Compile(data);

            floorTexture = data.RenderContext.CreateTexture2D(new Bitmap(Textures + "floor_color_map.jpg"), textureFilteringFunctionType.LinearMipmapLinear);
            lightMapTexture = data.RenderContext.CreateTexture2D(new Bitmap(Textures + "floor_light_map.jpg"), textureFilteringFunctionType.LinearMipmapLinear);

        }

        protected override void Render(RenderParams data)
        {            
            if (!multiTexture)
            {
                // Set the texture replace function
                data.RenderContext.SetShader(shaderType.Texture2DNoLights);

                data.RenderContext.SetTexture(floorTexture);
                data.RenderContext.DrawRichPlainQuads(Vertices, new Vector3D[] {Vector3D.AxisZ, Vector3D.AxisZ, Vector3D.AxisZ, Vector3D.AxisZ}, new PointF[] { new PointF(0,0), new PointF(tile_S, 0), new PointF(tile_S, tile_T), new PointF(0, tile_T)});                
            }            
        }
    }
}