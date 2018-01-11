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
        private Shader shaderObject;
        private Shader shaderShadow;
        public static readonly string ShaderName = nameof(shaderObject);
        public static readonly string ShaderShadowName = nameof(shaderShadow);
        private bool rainState;
        private bool smokeState;
        private Stopwatch stopWatch = new Stopwatch();
        private VAO cloud;
        private VAO tableCloth;
        private VAO table;
        private VAO tableLegs;
        private VAO candle;
        private VAO plate;
        private VAO wineGlass;
        private VAO grapes;
        private VAO knife;
        private VAO fork;
        private VAO lightSphere;
        private VAO environment;
        private CameraOrbit camera = new CameraOrbit();
        private Texture envMap_tex;
        private Texture tableCloth_tex;
        private Texture candle_tex;
        private QueryObject glTimer = new QueryObject();
        private TimeSpan timeSpan;
        private Random random = new Random();
        private FBO fboShadowMap = new FBOwithDepth(Texture.Create(12336, 12336, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float));
        private CameraOrbit cameraLight = new CameraOrbit();

        private Vector3 rainPosition;
        private Vector3 tablePosition;
        private Vector3 lightPosition;
        private Vector3 candlePosition;
        private Vector3 platePosition1;
        private Vector3 platePosition2;
        private Vector3 platePosition3;
        private Vector3 wineGlassPosition;
        private Vector3 grapePosition;
        private Vector3 knifePosition;
        private Vector3 forkPosition;

        public VisualObjects(Vector3 rainPosition, Vector3 lightPosition)
		{
            this.rainState = false;
            this.smokeState = false;
            this.rainPosition = rainPosition;
            this.tablePosition = new Vector3(.0f, -1.42f, -.1f);
            this.candlePosition = new Vector3(.0f, -.02f, .0f);
            this.platePosition1 = new Vector3(.6f, -.02f, .0f);
            this.platePosition2 = new Vector3(-.6f, -.02f, .0f);
            this.platePosition3 = new Vector3(.3f, -.02f, .65f);
            this.wineGlassPosition = new Vector3(.3f, -.02f, .65f);
            this.grapePosition = new Vector3(.35f, -.02f, .62f);
            this.knifePosition = new Vector3(-.6f, -.02f, .3f);
            this.forkPosition = new Vector3(-.6f, -.02f, -.3f);
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

            // Shadow mapping
            fboShadowMap.Texture.FilterNearest();

            // Camera Light
            cameraLight.FarClip = 50;
            cameraLight.Distance = 1;
            cameraLight.Elevation = 90;
            cameraLight.Azimuth = 0;
        }

		public void ShaderChanged(string name, Shader shader)
		{
            if (ShaderName == name)
            {
                this.shaderObject = shader;
                if (ReferenceEquals(shader, null)) return;

                // cloud
                // cloud needs to be translated higher to "get" the (beginning) light from below
                Mesh cloudMesh = Obj2Mesh.FromObj(Resourcen.cloud);
                this.cloud = VAOLoader.FromMesh(cloudMesh, shader);
                // table
                Mesh tableClothMesh = Obj2Mesh.FromObj(Resourcen.tableCloth);
                this.tableCloth = VAOLoader.FromMesh(tableClothMesh, shader);
                // table top
                Mesh tableMesh = Obj2Mesh.FromObj(Resourcen.table);
                this.table = VAOLoader.FromMesh(tableMesh, shader);
                // candle
                Mesh candleMesh = Obj2Mesh.FromObj(Resourcen.candle);
                this.candle = VAOLoader.FromMesh(candleMesh, shader);
                // plate
                Mesh plateMesh = Obj2Mesh.FromObj(Resourcen.plate);
                this.plate = VAOLoader.FromMesh(plateMesh, shader);
                // wine glass
                Mesh wineGlassMesh = Obj2Mesh.FromObj(Resourcen.wineGlass);
                this.wineGlass = VAOLoader.FromMesh(wineGlassMesh, shader);
                // grapes
                Mesh grapesMesh = Obj2Mesh.FromObj(Resourcen.grapes);
                this.grapes = VAOLoader.FromMesh(grapesMesh, shader);
                // knife
                Mesh knifeMesh = Obj2Mesh.FromObj(Resourcen.knife);
                //knifeMesh.Transform(System.Numerics.Matrix4x4.CreateRotationX(-90f));
                this.knife = VAOLoader.FromMesh(knifeMesh, shader);
                // fork
                Mesh forkMesh = Obj2Mesh.FromObj(Resourcen.fork);
                this.fork = VAOLoader.FromMesh(forkMesh, shader);
                // light sphere
                Mesh lightSphereMesh = Meshes.CreateSphere(0.1f, 4);
                this.lightSphere = VAOLoader.FromMesh(lightSphereMesh, shader);
                // environment sphere
                var sphere = Meshes.CreateSphere(3, 4);
                var envSphere = sphere.SwitchTriangleMeshWinding();
                this.environment = VAOLoader.FromMesh(envSphere, shader);
            }
            else if (ShaderShadowName == name)
            {
                this.shaderShadow = shader;
                if (ReferenceEquals(shaderObject, null)) return;
            }
        }

		public CameraOrbit Camera { get { return camera; } }

        public void Update(bool rainState, Vector3 rainPosition, Vector3 lightPosition, bool smokeState)
        {
            if (ReferenceEquals(shaderObject, null)) return;
            // update parameters
            this.rainState = rainState;
            this.rainPosition = rainPosition;
            this.lightPosition = lightPosition;
            this.smokeState = smokeState;
            // TODO: delete following
            this.cloud.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
            this.tableCloth.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.tableCloth.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.candle.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.candlePosition }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition1 }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition2 }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition3 }, VertexAttribPointerType.Float, 3, true);
            this.grapes.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.grapePosition }, VertexAttribPointerType.Float, 3, true);
            this.wineGlass.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.wineGlassPosition }, VertexAttribPointerType.Float, 3, true);
            this.knife.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.knifePosition }, VertexAttribPointerType.Float, 3, true);
            this.fork.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.forkPosition }, VertexAttribPointerType.Float, 3, true);
            this.lightSphere.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
        }

        public void Render(Matrix4 camera)
		{
            if (ReferenceEquals(null, shaderObject)) return;
            if (ReferenceEquals(null, shaderShadow)) return;

            // calculate value for candle light flickering
            timeSpan = stopWatch.Elapsed;
            double elapsedTime = timeSpan.TotalMilliseconds;
            var factor = 1;
            if (smokeState)
                factor = 4;
            float candleFlickering = (float)Math.Sin((elapsedTime / 1000 / factor) + (0.2 + random.NextDouble())) / 30 * factor;
            // setup light for shadow 
            cameraLight.Distance = 1.1f + candleFlickering;
            var light = cameraLight.CalcMatrix().ToOpenTK();
            
            shaderShadow.Activate();
            fboShadowMap.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UniformMatrix4(shaderShadow.GetUniformLocation("camera"), true, ref light);
            this.DrawScene(shaderShadow, light, candleFlickering);
            fboShadowMap.Deactivate();
            shaderShadow.Deactivate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shaderObject.Activate();
            fboShadowMap.Texture.Activate();
            GL.UniformMatrix4(shaderObject.GetUniformLocation("camera"), true, ref camera);
            GL.UniformMatrix4(shaderObject.GetUniformLocation("light"), true, ref light);
            this.DrawScene(shaderObject, camera, candleFlickering);

            fboShadowMap.Texture.Deactivate();
            shaderObject.Deactivate();
        }

        private void DrawScene(Shader shader, Matrix4 camera, float candleFlickering)
        {
            //Console.WriteLine("Value " + candleFlickering);

            // pass shader parameters
            // set and pass light values
            GL.Uniform3(shader.GetUniformLocation("moonLightDirection"), new Vector3(0, -10, 10).Normalized());
            GL.Uniform4(shader.GetUniformLocation("moonLightColor"), new Color4(.05f, .05f, .25f, 1f));
            GL.Uniform3(shader.GetUniformLocation("candleLightPosition"), new Vector3(0, 0.5f, 0));
            var candleLightStrength = 0.9f;
            if (smokeState)
                candleLightStrength = 0.6f;
            GL.Uniform4(shader.GetUniformLocation("candleLightColor"), new Color4(candleLightStrength + candleFlickering, 0.6f * (candleLightStrength + candleFlickering), 0f, 1.0f));
            GL.Uniform3(shader.GetUniformLocation("spotLightPosition"), lightPosition.Normalized());
            GL.Uniform3(shader.GetUniformLocation("spotLightDirection"), new Vector3(lightPosition[0] * (-1), -1.1f, lightPosition[2] * (-1)).Normalized());
            GL.Uniform1(shader.GetUniformLocation("spotLightAngle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
            GL.Uniform4(shader.GetUniformLocation("spotLightColor"), new Color4(0, 0, 1f, 1f));
            GL.Uniform4(shader.GetUniformLocation("ambientLightColor"), new Color4(0.1f, 0.1f, 0.2f, 1f));

            // camera
            GL.Uniform3(shader.GetUniformLocation("cameraPosition"), this.camera.CalcPosition().ToOpenTK());
            GL.UniformMatrix4(shader.GetUniformLocation("camera"), true, ref camera);

            // environment
            // different ids to differentiate spheres in fragment shader
            var id = 1;
            // variable that excludes object from shadow throwing
            var noShadow = 1;
            //envMap_tex.Activate();
            GL.Uniform1(shader.GetUniformLocation("id"), id);
            //this.environment.Draw();
            //envMap_tex.Deactivate();

            // objects
            id = 2;
            GL.Uniform1(shader.GetUniformLocation("id"), id);

            // draw objects
            // cloud
            if (this.rainState)
            {
                GL.Uniform1(shader.GetUniformLocation("noShadow"), noShadow);
                this.cloud.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
                this.cloud.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(0.1f, 0.1f, 0.6f, 1f) }, VertexAttribPointerType.Float, 4, true);
                this.cloud.Draw();
            }
            noShadow = 0;
            GL.Uniform1(shader.GetUniformLocation("noShadow"), noShadow);
            // plate 1
            this.plate.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition1 }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.plate.Draw();
            // plate 2
            this.plate.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition2 }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.plate.Draw();
            // grape plate
            this.plate.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.platePosition3 }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.8f, .8f, .8f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.plate.Draw();
            // wine glass
            this.wineGlass.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.wineGlassPosition }, VertexAttribPointerType.Float, 3, true);
            this.wineGlass.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            //this.wineGlass.Draw();
            // grapes
            this.grapes.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.grapePosition }, VertexAttribPointerType.Float, 3, true);
            this.grapes.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(0f, 1f, 0f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.grapes.Draw();
            // knife
            this.knife.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.knifePosition }, VertexAttribPointerType.Float, 3, true);
            this.knife.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.5f, .5f, .5f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.knife.Draw();
            // fork
            this.fork.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.forkPosition }, VertexAttribPointerType.Float, 3, true);
            this.fork.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.5f, .5f, .5f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.fork.Draw();
            // light sphere
            this.lightSphere.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
            this.lightSphere.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.lightSphere.Draw();
            // table
            id = 3;
            GL.Uniform1(shader.GetUniformLocation("id"), id);
            //tableCloth_tex.Activate();
            this.tableCloth.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.tableCloth.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.tableCloth.Draw();
            //tableCloth_tex.Deactivate();
            // table
            this.table.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.table.Draw();
            // candle
            //candle_tex.Activate();
            this.candle.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.candlePosition }, VertexAttribPointerType.Float, 3, true);
            this.candle.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.candle.Draw();
            //candle_tex.Deactivate();
        }
    }
}
