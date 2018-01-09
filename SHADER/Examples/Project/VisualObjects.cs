using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Example
{
	public class VisualObjects
	{
		public VisualObjects(Vector3 rainPosition, Vector3 lightPosition)
		{
            this.rainState = false;
            this.rainPosition = rainPosition;
            this.tablePosition = new Vector3(.0f, -1.42f, .0f);
            this.candlePosition = new Vector3(.0f, -.02f, .0f);
            this.platePosition1 = new Vector3(.6f, -.01f, .0f);
            this.platePosition2 = new Vector3(-.6f, -.01f, .0f);
            this.lightPosition = lightPosition;
            this.timeSpan = stopWatch.Elapsed;

            // environment texture
            envMap_tex = TextureLoader.FromBitmap(Resourcen.environment);
            envMap_tex.WrapMode(TextureWrapMode.MirroredRepeat);
            envMap_tex.FilterLinear();

            // table texture
            tableCloth_tex = TextureLoader.FromBitmap(Resourcen.tablecloth_tex);
            tableCloth_tex.WrapMode(TextureWrapMode.MirroredRepeat);
            tableCloth_tex.FilterLinear();

            // candle texture
            candle_tex = TextureLoader.FromBitmap(Resourcen.candle_tex);
            candle_tex.WrapMode(TextureWrapMode.MirroredRepeat);
            candle_tex.FilterLinear();

            GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);

            // time stopping for candle light flickering
            this.stopWatch.Start();
        }

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderObject = shader;
			if (ReferenceEquals(shader, null)) return;

            // cloud
            // cloud needs to be translated higher to "get" the (beginning) light from below
            Mesh cloudMesh = Obj2Mesh.FromObj(Resourcen.cloud);
            this.cloud = VAOLoader.FromMesh(cloudMesh, shader);
            // table
            Mesh tableMesh = Obj2Mesh.FromObj(Resourcen.table);
            this.table = VAOLoader.FromMesh(tableMesh, shader);
            // candle
            Mesh candleMesh = Obj2Mesh.FromObj(Resourcen.candle);
            this.candle = VAOLoader.FromMesh(candleMesh, shader);
            // plate
            Mesh plateMesh = Obj2Mesh.FromObj(Resourcen.plate);
            this.plate = VAOLoader.FromMesh(plateMesh, shader);
            // light sphere
            Mesh lightSphereMesh = Meshes.CreateSphere(0.1f, 4);
            this.lightSphere = VAOLoader.FromMesh(lightSphereMesh, shader);
            // environment sphere
            var sphere = Meshes.CreateSphere(3, 4);
            var envSphere = sphere.SwitchTriangleMeshWinding();
            this.environment = VAOLoader.FromMesh(envSphere, shader);
        }

		public CameraOrbit Camera { get { return camera; } }

        public void Update(bool rainState, Vector3 rainPosition, Vector3 lightPosition)
        {
            if (ReferenceEquals(shaderObject, null)) return;
            // update parameters
            this.rainState = rainState;
            this.rainPosition = rainPosition;
            this.lightPosition = lightPosition;
            this.cloud.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.candle.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.candlePosition }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition1 }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition2 }, VertexAttribPointerType.Float, 3, true);
            this.lightSphere.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
        }

        public void Render(Matrix4 camera)
		{
            if (ReferenceEquals(null, shaderObject)) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			shaderObject.Activate();
            Random random = new Random();

            // calculate value for candle light flickering
            timeSpan = stopWatch.Elapsed;
            double elapsedTime = timeSpan.TotalMilliseconds;
            float candleFlickering = 0.9f + (float)Math.Sin((elapsedTime / 1000) + random.NextDouble()) / 30;
            //Console.WriteLine("Value " + candleFlickering);

            // pass shader parameters
            // set and pass light values
            GL.Uniform3(shaderObject.GetUniformLocation("moonLightDirection"), new Vector3(0, 10, 10).Normalized());
            GL.Uniform4(shaderObject.GetUniformLocation("moonLightColor"), new Color4(0.2f, 0.2f, 0.5f, 1f));
            GL.Uniform3(shaderObject.GetUniformLocation("candleLightPosition"), new Vector3(0, 0.5f, 0));
            GL.Uniform4(shaderObject.GetUniformLocation("candleLightColor"), new Color4(candleFlickering, 0.6f*candleFlickering, 0f, 1.0f));
            GL.Uniform3(shaderObject.GetUniformLocation("spotLightPosition"), lightPosition.Normalized());
            GL.Uniform3(shaderObject.GetUniformLocation("spotLightDirection"), new Vector3(lightPosition[0]*(-1), -1.1f, lightPosition[2]*(-1)).Normalized());
            GL.Uniform1(shaderObject.GetUniformLocation("spotLightAngle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
            GL.Uniform4(shaderObject.GetUniformLocation("spotLightColor"), new Color4(0, 0, 1f, 1f));
            GL.Uniform4(shaderObject.GetUniformLocation("ambientLightColor"), new Color4(0.1f, 0.1f, 0.2f, 1f));

            // camera
            var cam = this.camera.CalcMatrix().ToOpenTK();
            GL.Uniform3(shaderObject.GetUniformLocation("cameraPosition"), this.camera.CalcPosition().ToOpenTK());
            GL.UniformMatrix4(shaderObject.GetUniformLocation("camera"), true, ref camera);

            // environment
            // different ids to differentiate spheres in fragment shader
            var id = 1;
            envMap_tex.Activate();
            GL.Uniform1(shaderObject.GetUniformLocation("id"), id);
            this.environment.Draw();
            envMap_tex.Deactivate();

            // objects
            id = 2;
            GL.Uniform1(shaderObject.GetUniformLocation("id"), id);

            // draw objects
            // cloud
            if (this.rainState)
            {
                this.cloud.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
                this.cloud.SetAttribute(shaderObject.GetAttributeLocation("materialColor"), new Color4[] { new Color4(0.1f, 0.1f, 0.6f, 1f) }, VertexAttribPointerType.Float, 4, true);
                this.cloud.Draw();
            }
            // plate 1
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition1 }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.plate.Draw();
            // plate 2
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition2 }, VertexAttribPointerType.Float, 3, true);
            this.plate.Draw();
            // light sphere
            this.lightSphere.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
            this.lightSphere.SetAttribute(shaderObject.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.lightSphere.Draw();
            // table
            id = 3;
            GL.Uniform1(shaderObject.GetUniformLocation("id"), id);
            tableCloth_tex.Activate();
            this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shaderObject.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.table.Draw();
            tableCloth_tex.Deactivate();
            // candle
            candle_tex.Activate();
            this.candle.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.candlePosition }, VertexAttribPointerType.Float, 3, true);
            this.candle.SetAttribute(shaderObject.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.candle.Draw();
            candle_tex.Deactivate();

            shaderObject.Deactivate();
		}

        public static readonly string ShaderName = nameof(shaderObject);
        private Shader shaderObject;
        private bool rainState;
		private Stopwatch stopWatch = new Stopwatch();
        private VAO cloud;
        private VAO table;
        private VAO candle;
        private VAO plate;
        private VAO lightSphere;
        private VAO environment;
        private CameraOrbit camera = new CameraOrbit();
        private Texture envMap_tex;
        private Texture tableCloth_tex;
        private Texture candle_tex;
        private QueryObject glTimer = new QueryObject();
        private TimeSpan timeSpan;

        private Vector3 rainPosition;
        private Vector3 tablePosition;
        private Vector3 lightPosition;
        private Vector3 candlePosition;
        private Vector3 platePosition1;
        private Vector3 platePosition2;
    }
}
