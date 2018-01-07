﻿using DMS.OpenGL;
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
            this.tablePosition = new Vector3(.0f, -4.75f, .0f);
            this.lightPosition = lightPosition;

            envMap = TextureLoader.FromBitmap(Resourcen.environment);
            envMap.WrapMode(TextureWrapMode.MirroredRepeat);
            envMap.FilterLinear();

            GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);

            Color4 backgroundColor = new Color4(0.0f, 0.05f, 0.15f, 1.0f);
            //GL.ClearColor(backgroundColor); // set background color
        }

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderObject = shader;
			if (ReferenceEquals(shader, null)) return;

            // cloud
            // cloud needs to be translated higher to "get" the (beginning) light from below
            Mesh cloudMesh = Obj2Mesh.FromObj(Resourcen.cloud).Transform(System.Numerics.Matrix4x4.CreateTranslation(0, 1, 0));
            this.cloud = VAOLoader.FromMesh(cloudMesh, shader);
            /**
            // table
            Mesh tableMesh = Obj2Mesh.FromObj(Resourcen.table);
            this.table = VAOLoader.FromMesh(tableMesh, shader);
            // light
            Mesh lightSphereMesh = Meshes.CreateSphere(0.25f, 4);
            this.lightSphere = VAOLoader.FromMesh(lightSphereMesh, shader);
            **/

            // All objects together
            var mesh = new Mesh();
            // sphere
            var lightSphereMesh = Meshes.CreateSphere(0.25f, 4);
            mesh.Add(lightSphereMesh.Transform(System.Numerics.Matrix4x4.CreateTranslation(lightPosition[0], lightPosition[1], lightPosition[2])));
            // cloud
            //var cloud = Obj2Mesh.FromObj(Resourcen.cloud);
            //mesh.Add(cloud.Transform(System.Numerics.Matrix4x4.CreateTranslation(rainPosition[0], rainPosition[1], rainPosition[2])));
            // table
            var table = Obj2Mesh.FromObj(Resourcen.table);
            mesh.Add(table.Transform(System.Numerics.Matrix4x4.CreateTranslation(tablePosition[0], tablePosition[1], tablePosition[2])));
            var candle = Obj2Mesh.FromObj(Resourcen.candle);
            mesh.Add(candle.Transform(System.Numerics.Matrix4x4.CreateTranslation(0, 0.5f, 0)));
            this.geometry = VAOLoader.FromMesh(mesh, shader);

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
            this.rainPosition = new Vector3(rainPosition[0], rainPosition[1]-0.25f, rainPosition[2]);   // Correct height, because of transformation at creation
            this.lightPosition = lightPosition;
            this.cloud.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
            //this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            //this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
            this.geometry.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { new Vector3(0) }, VertexAttribPointerType.Float, 3, false);

        }

        public void Render(Matrix4 camera)
		{
            if (ReferenceEquals(null, shaderObject)) return;
            

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			shaderObject.Activate();
            Random random = new Random();

        // pass shader parameters
            GL.Uniform3(shaderObject.GetUniformLocation("light1Direction"), new Vector3(0, 10, 10).Normalized());
            GL.Uniform4(shaderObject.GetUniformLocation("light1Color"), new Color4(0.1f, 0.1f, 0.1f, 1f));
            GL.Uniform3(shaderObject.GetUniformLocation("light2Position"), new Vector3(lightPosition[0], 0.5f, lightPosition[2]));
            GL.Uniform4(shaderObject.GetUniformLocation("light2Color"), new Color4((float)Math.Sin(random.NextDouble()), 0f, 0f, 1f));
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

            // environment
            // different ids to differentiate spheres in fragment shader
            var id = 1;
            envMap.Activate();
            GL.Uniform1(shaderObject.GetUniformLocation("id"), id);
            this.environment.Draw();
            envMap.Deactivate();

            // objects
            id = 2;
            GL.Uniform1(shaderObject.GetUniformLocation("id"), id);

            // draw objects
            if (this.rainState)
            {
                this.cloud.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.rainPosition }, VertexAttribPointerType.Float, 3, true);
                this.cloud.Draw();
            }
            /**
            this.table.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.tablePosition }, VertexAttribPointerType.Float, 3, true);
            this.table.Draw();
            this.lightSphere.SetAttribute(shaderObject.GetAttributeLocation("instancePosition"), new Vector3[] { this.lightPosition }, VertexAttribPointerType.Float, 3, true);
            this.lightSphere.Draw();
            **/

            geometry.Draw();
            shaderObject.Deactivate();
		}

        public static readonly string ShaderName = nameof(shaderObject);
        private Shader shaderObject;
        private bool rainState;
		private Stopwatch timeSource = new Stopwatch();
        private VAO cloud;
        //private VAO table;
        //private VAO lightSphere;
        private VAO geometry;
        private VAO environment;
        private CameraOrbit camera = new CameraOrbit();
        private Texture envMap;

        private Vector3 rainPosition;
        private Vector3 tablePosition;
        private Vector3 lightPosition;

    }
}