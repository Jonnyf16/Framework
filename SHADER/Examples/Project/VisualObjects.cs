﻿using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Diagnostics;

namespace Example
{
	public class VisualObjects
	{
		public VisualObjects(Vector3 rainPosition)
		{
            this.rainState = false;
            this.rainPosition = rainPosition;
            this.tablePosition = new Vector3(.0f, -1.93f, .0f);

            camera.FarClip = 500;
			camera.Distance = 30;
            camera.FovY = 30;
            GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
            GL.ClearColor(Color.Black); // set background color
        }

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderObject = shader;
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
            if (ReferenceEquals(shaderObject, null)) return;
            // update parameters
            this.rainState = rainState;
            this.rainPosition = rainPosition;
            this.cloud.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
            this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
        }

        public void Render(Matrix4 camera)
		{
            if (ReferenceEquals(null, shaderObject)) return;
            var time = (float)timeSource.Elapsed.TotalSeconds;
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			shaderObject.Activate();
			GL.Uniform1(shaderObject.GetUniformLocation("time"), time);
            GL.Uniform3(shaderObject.GetUniformLocation("light1Direction"), new Vector3(-1, -1, -1).Normalized());
            GL.Uniform4(shaderObject.GetUniformLocation("light1Color"), new Color4(0f, 1f, 0f, 1f));
            GL.Uniform3(shaderObject.GetUniformLocation("light2Position"), new Vector3(0.0f, 0.1f, 0.0f).Normalized());
            GL.Uniform4(shaderObject.GetUniformLocation("light2Color"), new Color4(1f, 0f, 0f, 1f));
            GL.Uniform3(shaderObject.GetUniformLocation("light3Position"), new Vector3(0, 2, 0));
            GL.Uniform3(shaderObject.GetUniformLocation("light3Direction"), new Vector3(0, -0.1f, 0).Normalized());
            GL.Uniform1(shaderObject.GetUniformLocation("light3Angle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
            GL.Uniform4(shaderObject.GetUniformLocation("light3Color"), new Color4(0, 0, 1f, 1f));
            GL.Uniform4(shaderObject.GetUniformLocation("ambientLightColor"), new Color4(0.1f, 0.1f, 0.1f, 1f));
            GL.Uniform4(shaderObject.GetUniformLocation("materialColor"), new Color4(1f, 1f, 1f, 1f));
            var cam = this.camera.CalcMatrix().ToOpenTK();
            GL.UniformMatrix4(shaderObject.GetUniformLocation("camera"), true, ref cam);
            GL.Uniform3(shaderObject.GetUniformLocation("cameraPosition"), this.camera.CalcPosition().ToOpenTK());
            GL.UniformMatrix4(shaderObject.GetUniformLocation("camera"), true, ref camera);
            if (this.rainState)
            {
                this.cloud.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
                this.cloud.Draw();
            }
            this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.table.Draw();
			shaderObject.Deactivate();
		}

        public static readonly string ShaderName = nameof(shaderObject);
        private Shader shaderObject;
        private bool rainState;
		private Stopwatch timeSource = new Stopwatch();
        private VAO cloud;
        private VAO table;
		private CameraOrbit camera = new CameraOrbit();

        private Vector3 rainPosition;
        private Vector3 tablePosition;

    }
}
