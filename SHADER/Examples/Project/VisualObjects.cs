using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Diagnostics;

namespace Example
{
	public class VisualObjects
	{
		public VisualObjects(Vector3 rainPosition)
		{
            this.rainState = false;
            this.rainPosition = rainPosition;
            this.tablePosition = new Vector3(.0f, -2.72f, .0f);

            camera.FarClip = 500;
			camera.Distance = 30;
            camera.FovY = 30;
            GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
		}

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderCloud = shader;
			if (ReferenceEquals(shader, null)) return;
            // cloud
			Mesh cloudMesh = Obj2Mesh.FromObj(Resourcen.cloud);
			this.cloud = VAOLoader.FromMesh(cloudMesh, shader);
            // table
            Mesh tableMesh = Obj2Mesh.FromObj(Resourcen.table);
            this.table = VAOLoader.FromMesh(tableMesh, shader);
        }

		public CameraOrbit Camera { get { return camera; } }

        public void Update(bool rainState, Vector3 rainPosition)
        {
            if (ReferenceEquals(shaderCloud, null)) return;
            // update parameters
            this.rainState = rainState;
            this.rainPosition = rainPosition;
            this.cloud.SetAttribute(shaderCloud.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shaderCloud.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
        }

        public void Render(Matrix4 camera)
		{
			if (ReferenceEquals(null, shaderCloud)) return;
            this.cloud.SetAttribute(shaderCloud.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shaderCloud.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            var time = (float)timeSource.Elapsed.TotalSeconds;
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			shaderCloud.Activate();
			GL.Uniform1(shaderCloud.GetUniformLocation("time"), time);
            GL.Uniform3(shaderCloud.GetUniformLocation("light1Direction"), new Vector3(-1, -1, -1).Normalized());
            GL.Uniform4(shaderCloud.GetUniformLocation("light1Color"), new Color4(1f, 1f, 1f, 1f));
            GL.Uniform3(shaderCloud.GetUniformLocation("light2Position"), new Vector3(-1, -1, 1));
            GL.Uniform4(shaderCloud.GetUniformLocation("light2Color"), new Color4(1f, .1f, .1f, 1f));
            GL.Uniform3(shaderCloud.GetUniformLocation("light3Position"), new Vector3(-2, 2, 2));
            GL.Uniform3(shaderCloud.GetUniformLocation("light3Direction"), new Vector3(1, -1, -1).Normalized());
            GL.Uniform1(shaderCloud.GetUniformLocation("light3Angle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
            GL.Uniform4(shaderCloud.GetUniformLocation("light3Color"), new Color4(0, 0, 1f, 1f));
            GL.Uniform4(shaderCloud.GetUniformLocation("ambientLightColor"), new Color4(.1f, .1f, .1f, 1f));
            GL.Uniform4(shaderCloud.GetUniformLocation("materialColor"), new Color4(.7f, .9f, .7f, 1f));
            var cam = this.camera.CalcMatrix().ToOpenTK();
            GL.UniformMatrix4(shaderCloud.GetUniformLocation("camera"), true, ref cam);
            GL.Uniform3(shaderCloud.GetUniformLocation("cameraPosition"), this.camera.CalcPosition().ToOpenTK());
            GL.UniformMatrix4(shaderCloud.GetUniformLocation("camera"), true, ref camera);
            if (this.rainState)
                this.cloud.Draw();
            this.table.Draw();
			shaderCloud.Deactivate();
		}

        public static readonly string ShaderName = nameof(shaderCloud);
        private Shader shaderCloud;
        private bool rainState;
		private Stopwatch timeSource = new Stopwatch();
        private VAO cloud;
        private VAO table;
		private CameraOrbit camera = new CameraOrbit();

        private Vector3 rainPosition;
        private Vector3 tablePosition;

    }
}
