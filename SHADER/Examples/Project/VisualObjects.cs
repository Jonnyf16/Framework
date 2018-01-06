using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Diagnostics;

namespace Example
{
	public class VisualObjects
	{
		public VisualObjects(Vector3 rainPosition, Vector3 lightPosition)
		{
            this.rainState = false;
            this.rainPosition = rainPosition;
            this.tablePosition = new Vector3(.0f, -1.93f, .0f);
            this.lightPosition = lightPosition;

            camera.FarClip = 500;
			camera.Distance = 30;
            camera.FovY = 30;

            GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);

            Color4 backgroundColor = new Color4(0.0f, 0.05f, 0.15f, 1.0f);
            GL.ClearColor(backgroundColor); // set background color
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
            // light
            Mesh lightSphereMesh = Meshes.CreateSphere(0.25f, 4);
            this.lightSphere = VAOLoader.FromMesh(lightSphereMesh, shader);

            /**
            var mesh = new Mesh();
            //var sphere = Meshes.CreateSphere(1, 4);
            //mesh.Add(sphere.Transform(System.Numerics.Matrix4x4.CreateTranslation(rainPosition[0], rainPosition[1], rainPosition[2])));
            var xform = new Transformation();
            var cloud = Obj2Mesh.FromObj(Resourcen.cloud);
            mesh.Add(cloud.Transform(System.Numerics.Matrix4x4.CreateTranslation(0, 8, 0)));
            var table = Obj2Mesh.FromObj(Resourcen.table);
            mesh.Add(table);
            this.geometry = VAOLoader.FromMesh(mesh, shader);
            **/
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
            this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
            //this.geometry.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { new Vector3(0, -2.0f, 0) }, VertexAttribPointerType.Float, 3, true);
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
            GL.Uniform3(shaderObject.GetUniformLocation("light2Position"), new Vector3(lightPosition[0], -.5f, lightPosition[2]).Normalized());
            GL.Uniform4(shaderObject.GetUniformLocation("light2Color"), new Color4(1f, 0f, 0f, 1f));
            GL.Uniform3(shaderObject.GetUniformLocation("light3Position"), lightPosition.Normalized());
            GL.Uniform3(shaderObject.GetUniformLocation("light3Direction"), new Vector3(lightPosition[0]*(-1), -1.1f, lightPosition[2]*(-1)).Normalized());
            GL.Uniform1(shaderObject.GetUniformLocation("light3Angle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
            GL.Uniform4(shaderObject.GetUniformLocation("light3Color"), new Color4(0, 0, 1f, 1f));
            GL.Uniform4(shaderObject.GetUniformLocation("ambientLightColor"), new Color4(0.1f, 0.1f, 0.2f, 1f));
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
            this.lightSphere.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
            this.lightSphere.Draw();

            //geometry.Draw();
            shaderObject.Deactivate();
		}

        public static readonly string ShaderName = nameof(shaderObject);
        private Shader shaderObject;
        private bool rainState;
		private Stopwatch timeSource = new Stopwatch();
        private VAO cloud;
        private VAO table;
        private VAO lightSphere;
        //private VAO geometry;
        private CameraOrbit camera = new CameraOrbit();

        private Vector3 rainPosition;
        private Vector3 tablePosition;
        private Vector3 lightPosition;

    }
}
