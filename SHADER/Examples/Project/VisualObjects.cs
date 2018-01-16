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
        private TimeSpan timeSpan;
        private VAO cloud;
        private VAO tableCloth;
        private VAO table;
        private VAO tableLegs;
        private VAO candle;
        private VAO plate;
        private VAO grapePlate;
        private VAO wineBottle;
        private VAO wineBottleLabel;
        private VAO wineBottleShutter;
        private VAO grapes;
        private VAO knifeLeft;
        private VAO knifeRight;
        private VAO forkLeft;
        private VAO forkRight;
        private VAO environment;
        private Texture envMap_tex;
        private QueryObject glTimer = new QueryObject();
        private Random random = new Random();
        private FBO fboShadowMap = new FBOwithDepth(Texture.Create(12336, 12336, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float));
        private CameraOrbit cameraLight = new CameraOrbit();

        private Vector3 rainPosition;
        private Vector3 tablePosition;
        private Vector3 lightPosition;
        private Vector3 candlePosition;
        private Vector3 plateLeftPosition;
        private Vector3 plateRightPosition;
        private Vector3 plateGrapePosition;
        private Vector3 wineBottlePosition;
        private Vector3 grapePosition;
        private Vector3 knifeLeftPosition;
        private Vector3 knifeRightPosition;
        private Vector3 forkLeftPosition;
        private Vector3 forkRightPosition;

        public VisualObjects(Vector3 rainPosition, Vector3 candlePosition, Vector3 lightPosition)
		{
            this.rainState = false;
            this.smokeState = false;
            this.rainPosition = rainPosition;
            this.candlePosition = candlePosition;
            this.tablePosition = new Vector3(.0f, -1.42f, -.1f);
            this.plateLeftPosition = new Vector3(.6f, -.015f, .0f);
            this.plateRightPosition = new Vector3(-.6f, -.015f, .0f);
            this.plateGrapePosition = new Vector3(0, -.015f, .4f);
            this.wineBottlePosition = new Vector3(-.1f, -.015f, -.4f);
            this.grapePosition = new Vector3(.05f, -.04f, .35f);
            this.knifeLeftPosition = new Vector3(-.6f, -.015f, .3f);
            this.knifeRightPosition = new Vector3(.6f, -.015f, -.3f);
            this.forkLeftPosition = new Vector3(-.6f, -.015f, -.3f);
            this.forkRightPosition = new Vector3(.6f, -.015f, .3f);
            this.lightPosition = lightPosition;
            this.timeSpan = stopWatch.Elapsed;

            // environment texture
            envMap_tex = TextureLoader.FromBitmap(Resourcen.environment);
            envMap_tex.WrapMode(TextureWrapMode.MirroredRepeat);
            envMap_tex.FilterLinear();

            GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);

            // time stopping for candle light flickering
            this.stopWatch.Start();

            // Shadow mapping
            fboShadowMap.Texture.FilterNearest();

            // Camera Light
            cameraLight.FarClip = 50;
            cameraLight.Distance = lightPosition[1];
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
                // grape plate
                var xform = new Transformation();
                xform.ScaleGlobal(new System.Numerics.Vector3(.8f));
                Mesh grapePlateMesh = Obj2Mesh.FromObj(Resourcen.plate);
                this.grapePlate = VAOLoader.FromMesh(grapePlateMesh.Transform(xform), shader);
                // wine bottle
                Mesh wineBottleMesh = Obj2Mesh.FromObj(Resourcen.wineBottle);
                this.wineBottle = VAOLoader.FromMesh(wineBottleMesh, shader);
                // wine bottle label
                Mesh wineBottleLabelMesh = Obj2Mesh.FromObj(Resourcen.wineBottleLabel);
                this.wineBottleLabel = VAOLoader.FromMesh(wineBottleLabelMesh, shader);
                // wine bottle shutter
                Mesh wineBottleShutterMesh = Obj2Mesh.FromObj(Resourcen.wineBottleShutter);
                this.wineBottleShutter = VAOLoader.FromMesh(wineBottleShutterMesh, shader);
                // grapes
                Mesh grapesMesh = Obj2Mesh.FromObj(Resourcen.grapes);
                this.grapes = VAOLoader.FromMesh(grapesMesh, shader);
                // knife left
                Mesh knifeLeftMesh = Obj2Mesh.FromObj(Resourcen.knife);
                this.knifeLeft = VAOLoader.FromMesh(knifeLeftMesh, shader);
                // knife right
                var xform_ = new Transformation();
                xform_.RotateYGlobal(180f);
                Mesh knifeRightMesh = Obj2Mesh.FromObj(Resourcen.knife);
                this.knifeRight = VAOLoader.FromMesh(knifeRightMesh.Transform(xform_), shader);
                // fork left
                Mesh forkLeftMesh = Obj2Mesh.FromObj(Resourcen.fork);
                this.forkLeft = VAOLoader.FromMesh(forkLeftMesh, shader);
                // fork right
                Mesh forkRightMesh = Obj2Mesh.FromObj(Resourcen.fork);
                this.forkRight = VAOLoader.FromMesh(forkRightMesh.Transform(xform_), shader);
                // environment sphere
                var sphere = Meshes.CreateSphere(6, 4);
                var envSphere = sphere.SwitchTriangleMeshWinding();
                this.environment = VAOLoader.FromMesh(envSphere, shader);
            }
            else if (ShaderShadowName == name)
            {
                this.shaderShadow = shader;
                if (ReferenceEquals(shaderObject, null)) return;
            }
        }

        public void Update(bool rainState, Vector3 rainPosition, Vector3 candlePosition, Vector3 lightPosition, bool smokeState)
        {
            if (ReferenceEquals(shaderObject, null)) return;
            // update parameters
            this.rainState = rainState;
            this.rainPosition = rainPosition;
            this.candlePosition = candlePosition;
            this.lightPosition = lightPosition;
            this.smokeState = smokeState;
        }

        public void Render(CameraOrbit camera)
		{
            if (ReferenceEquals(null, shaderObject)) return;
            if (ReferenceEquals(null, shaderShadow)) return;

            // calculate value for candle light flickering
            timeSpan = stopWatch.Elapsed;
            double elapsedTime = timeSpan.TotalMilliseconds;
            var factor = 1;
            if (smokeState)
                factor = 4;
            float candleFlickering = (float)Math.Sin((elapsedTime / 1000 / factor) + (.2 + random.NextDouble())) / 30 * factor;
            // setup light for shadow 
            cameraLight.Distance = this.lightPosition[1] + candleFlickering;
            var light = cameraLight.CalcMatrix().ToOpenTK();
            
            shaderShadow.Activate();
            fboShadowMap.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UniformMatrix4(shaderShadow.GetUniformLocation("camera"), true, ref light);
            this.DrawScene(shaderShadow, cameraLight, candleFlickering);
            fboShadowMap.Deactivate();
            shaderShadow.Deactivate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shaderObject.Activate();
            fboShadowMap.Texture.Activate();
            var cam = camera.CalcMatrix().ToOpenTK();
            GL.UniformMatrix4(shaderObject.GetUniformLocation("camera"), true, ref cam);
            GL.UniformMatrix4(shaderObject.GetUniformLocation("light"), true, ref light);
            this.DrawScene(shaderObject, camera, candleFlickering);
            fboShadowMap.Texture.Deactivate();
            shaderObject.Deactivate();

            // draw environment
            shaderObject.Activate();
            this.DrawEnvironment(shaderObject, camera);
            shaderObject.Deactivate();
        }

        private void DrawEnvironment(Shader shader, CameraOrbit camera)
        {
            // camera
            var cam = camera.CalcMatrix().ToOpenTK();
            GL.Uniform3(shader.GetUniformLocation("cameraPosition"), camera.CalcPosition().ToOpenTK());
            GL.UniformMatrix4(shader.GetUniformLocation("camera"), true, ref cam);

            // environment
            // different ids to differentiate spheres in fragment shader
            var id = 1;
            // variable that excludes object from shadow throwing
            envMap_tex.Activate();
            GL.Uniform1(shader.GetUniformLocation("id"), id);
            this.environment.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { new Vector3(0, 0, 0) }, VertexAttribPointerType.Float, 3, true);
            this.environment.Draw();
            envMap_tex.Deactivate();
        }
        
        private void DrawScene(Shader shader, CameraOrbit camera, float candleFlickering)
        {
            //Console.WriteLine("Value " + candleFlickering);

            // pass shader parameters
            // set and pass light values
            var candleLightStrength = 0.9f;
            if (smokeState)
                candleLightStrength = 0.6f;
            GL.Uniform3(shader.GetUniformLocation("moonLightDirection"), new Vector3(0, -10, 10).Normalized());
            GL.Uniform4(shader.GetUniformLocation("moonLightColor"), new Color4(.05f, .05f, .25f, 1f));
            GL.Uniform3(shader.GetUniformLocation("candleLightPosition"), new Vector3(0, .5f, 0));
            GL.Uniform4(shader.GetUniformLocation("candleLightColor"), new Color4(candleLightStrength + candleFlickering, .6f * (candleLightStrength + candleFlickering), 0f, 1f));
            GL.Uniform3(shader.GetUniformLocation("spotLightPosition"), lightPosition.Normalized());
            GL.Uniform3(shader.GetUniformLocation("spotLightDirection"), new Vector3(lightPosition[0] * (-1), -1.1f, lightPosition[2] * (-1)).Normalized());
            GL.Uniform1(shader.GetUniformLocation("spotLightAngle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
            GL.Uniform4(shader.GetUniformLocation("spotLightColor"), new Color4(0, 0, 1f, 1f));
            GL.Uniform4(shader.GetUniformLocation("ambientLightColor"), new Color4(0.1f, 0.1f, 0.2f, 1f));

            // camera
            var cam = camera.CalcMatrix().ToOpenTK();
            GL.Uniform3(shader.GetUniformLocation("cameraPosition"), camera.CalcPosition().ToOpenTK());
            GL.UniformMatrix4(shader.GetUniformLocation("camera"), true, ref cam);

            var id = 2;
            GL.Uniform1(shader.GetUniformLocation("id"), id);

            // draw objects
            var noShadow = 0;
            GL.Uniform1(shader.GetUniformLocation("noShadow"), noShadow);
            // plate 1
            this.plate.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.plateLeftPosition }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.plate.Draw();
            // plate 2
            this.plate.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.plateRightPosition }, VertexAttribPointerType.Float, 3, true);
            this.plate.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.plate.Draw();
            // grape plate
            this.grapePlate.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.plateGrapePosition }, VertexAttribPointerType.Float, 3, true);
            this.grapePlate.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.grapePlate.Draw();
            // wine bottle
            this.wineBottle.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.wineBottlePosition }, VertexAttribPointerType.Float, 3, true);
            this.wineBottle.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.1f, .4f, .1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.wineBottle.Draw();
            // wine bottle label
            this.wineBottleLabel.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.wineBottlePosition }, VertexAttribPointerType.Float, 3, true);
            this.wineBottleLabel.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.wineBottleLabel.Draw();
            // wine bottle shutter
            this.wineBottleShutter.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.wineBottlePosition }, VertexAttribPointerType.Float, 3, true);
            this.wineBottleShutter.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(0.54f, 0.12f, 0.2f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.wineBottleShutter.Draw();
            // grapes
            this.grapes.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.grapePosition }, VertexAttribPointerType.Float, 3, true);
            this.grapes.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(0.54f, .15f, 0.4f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.grapes.Draw();
            // knife right
            this.knifeRight.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.knifeRightPosition }, VertexAttribPointerType.Float, 3, true);
            this.knifeRight.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.5f, .5f, .5f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.knifeRight.Draw();
            // knife left
            this.knifeLeft.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.knifeLeftPosition }, VertexAttribPointerType.Float, 3, true);
            this.knifeLeft.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.5f, .5f, .5f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.knifeLeft.Draw();
            // fork right
            this.forkRight.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.forkRightPosition }, VertexAttribPointerType.Float, 3, true);
            this.forkRight.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.5f, .5f, .5f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.forkRight.Draw();
            // fork left
            this.forkLeft.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.forkLeftPosition }, VertexAttribPointerType.Float, 3, true);
            this.forkLeft.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(.5f, .5f, .5f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.forkLeft.Draw();
            // candle
            this.candle.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.candlePosition }, VertexAttribPointerType.Float, 3, true);
            this.candle.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.candle.Draw();

            // table cloth
            id = 3;
            GL.Uniform1(shader.GetUniformLocation("id"), id);
            this.tableCloth.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.tableCloth.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(0.27f, 0.19f, 0.125f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.tableCloth.Draw();

            // cloud
            if (this.rainState)
            {
                noShadow = 1;
                GL.Uniform1(shader.GetUniformLocation("noShadow"), noShadow);
                this.cloud.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
                this.cloud.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
                this.cloud.Draw();
            }

            noShadow = 0;
            GL.Uniform1(shader.GetUniformLocation("noShadow"), noShadow);

            // table
            this.table.SetAttribute(shader.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shader.GetAttributeLocation("materialColor"), new Color4[] { new Color4(1f, 1f, 1f, 1f) }, VertexAttribPointerType.Float, 4, true);
            this.table.Draw();
        }
    }
}
