using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Example
{
	public class VisualCloud
	{
		public VisualCloud(Vector3 rainPosition)
		{
            this.rainState = false;
            this.rainPosition = rainPosition;

            camera.FarClip = 500;
			camera.Distance = 30;
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
		}

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderCloud = shader;
			if (ReferenceEquals(shader, null)) return;
			Mesh mesh = Obj2Mesh.FromObj(Resourcen.suzanne);
			this.cloud = VAOLoader.FromMesh(mesh, shader);
		}

		public CameraOrbit Camera { get { return camera; } }

        public void Update(bool rainState, Vector3 rainPosition)
        {
            if (ReferenceEquals(shaderCloud, null)) return;
            // update parameters
            this.rainState = rainState;
            this.rainPosition = rainPosition;

            this.cloud.SetAttribute(shaderCloud.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
            this.cloud.SetAttribute(shaderCloud.GetAttributeLocation("instanceScale"), new float[] { 0.3f }, VertexAttribPointerType.Float, 1, true);
        }

        public void Render(Matrix4 camera)
		{
			if (ReferenceEquals(null, shaderCloud)) return;
            this.cloud.SetAttribute(shaderCloud.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
			this.cloud.SetAttribute(shaderCloud.GetAttributeLocation("instanceScale"), new float[] { 0.3f }, VertexAttribPointerType.Float, 1, true);
            
			var time = (float)timeSource.Elapsed.TotalSeconds;

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			shaderCloud.Activate();
			GL.Uniform1(shaderCloud.GetUniformLocation("time"), time);
			//Matrix4 cam = camera.CalcMatrix().ToOpenTK();
			GL.UniformMatrix4(shaderCloud.GetUniformLocation("camera"), true, ref camera);
            if (this.rainState)
                this.cloud.Draw();
			shaderCloud.Deactivate();
		}

        public static readonly string ShaderName = nameof(shaderCloud);
        private Shader shaderCloud;
        private bool rainState;
		private Stopwatch timeSource = new Stopwatch();
        private VAO cloud;
		private CameraOrbit camera = new CameraOrbit();

        private Vector3 rainPosition;

    }
}
