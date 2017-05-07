﻿using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using DMS.ShaderDebugging;
using DMS.System;
using System.IO;

namespace Example
{
	public class MainVisual : IWindow
	{
		public CameraOrbit OrbitCamera { get { return camera; } }

		public MainVisual()
		{
			var dir = Path.GetDirectoryName(PathTools.GetSourceFilePath()) + "/Resources/";
			shaderWatcher = new ShaderFileDebugger(dir + "vertex.vert", dir + "fragment.frag"
				, Resourcen.vertex, Resourcen.fragment);
			geometry = CreateMesh(shaderWatcher.Shader);

			camera.FarClip = 50;
			camera.Distance = 5;
			camera.FovY = 30;

			GL.ClearColor(Color4.White);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
		}

		public void Render()
		{
			if (shaderWatcher.CheckForShaderChange())
			{
				//update geometry when shader changes
				geometry = CreateMesh(shaderWatcher.Shader);
			}
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			var shader = shaderWatcher.Shader;
			shader.Activate();
			GL.Uniform3(shader.GetUniformLocation("light1Direction"), new Vector3(-1, -1, -1).Normalized());
			GL.Uniform4(shader.GetUniformLocation("light1Color"), new Color4(1f, 1f, 1f, 1f));
			GL.Uniform3(shader.GetUniformLocation("light2Position"), new Vector3(-1, -1, 1));
			GL.Uniform4(shader.GetUniformLocation("light2Color"), new Color4(1f, .1f, .1f, 1f));
			GL.Uniform3(shader.GetUniformLocation("light3Position"), new Vector3(-2, 2, 2));
			GL.Uniform3(shader.GetUniformLocation("light3Direction"), new Vector3(1, -1, -1).Normalized());
			GL.Uniform1(shader.GetUniformLocation("light3Angle"), DMS.Geometry.MathHelper.DegreesToRadians(10f));
			GL.Uniform4(shader.GetUniformLocation("light3Color"), new Color4(0, 0, 1f, 1f));
			GL.Uniform4(shader.GetUniformLocation("ambientLightColor"), new Color4(.3f, .3f, .1f, 1f));
			GL.Uniform4(shader.GetUniformLocation("materialColor"), new Color4(.7f, .7f, .7f, 1f));
			var cam = camera.CalcMatrix().ToOpenTK();
			GL.UniformMatrix4(shader.GetUniformLocation("camera"), true, ref cam);
			GL.Uniform3(shader.GetUniformLocation("cameraPosition"), camera.CalcPosition().ToOpenTK());
			geometry.Draw();
			shader.Deactivate();
		}

		public void Update(float updatePeriod)
		{
		}

		private CameraOrbit camera = new CameraOrbit();
		private ShaderFileDebugger shaderWatcher;
		private VAO geometry;

		private static VAO CreateMesh(Shader shader)
		{
			var mesh = new Mesh();
			var sphere = Meshes.CreateSphere(1, 4);
			mesh.Add(sphere);
			var suzanne = Obj2Mesh.FromObj(Resourcen.suzanne);
			mesh.Add(suzanne.Transform(System.Numerics.Matrix4x4.CreateTranslation(2, 2, -2)));
			return VAOLoader.FromMesh(mesh, shader);
		}
	}
}
