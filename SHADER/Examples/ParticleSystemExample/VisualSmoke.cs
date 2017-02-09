﻿using Framework;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ShaderDebugging;
using System;

namespace Example
{
	public class VisualSmoke
	{
		public Vector3 Emitter { get; set; }
		public Vector3 Wind { get; set; }

		public VisualSmoke(Vector3 emitterPos, Vector3 wind)
		{
			Emitter = emitterPos;
			Wind = wind;
			texStar = TextureLoader.FromBitmap(Resourcen.smoke);
			shaderWatcher = new ShaderFileDebugger("../../ParticleSystemExample/Resources/smoke.vert"
				, "../../ParticleSystemExample/Resources/smoke.frag"
				, Resourcen.smoke_vert, Resourcen.smoke_frag);

			particleSystem.ReleaseInterval = 0.02f;
			particleSystem.OnParticleCreate += Create;
		}

		public Particle Create(float creationTime)
		{
			var p = new Particle(creationTime);
			p.LifeTime = 10f;
			Func<float> Rnd01 = () => (float)random.NextDouble();
			Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;
			//start at emitter position
			p.Position = Emitter;
			//slightly different upward vectors
			var direction = new Vector3(0.3f * RndCoord(), 1, 0.3f * RndCoord());
			//speed between [.2, .4]
			p.Velocity = (.2f + .2f * Rnd01()) * direction;
			p.Acceleration = Wind;
			return p;
		}

		public void Update(float time)
		{
			particleSystem.Update(time);
			//gather all active particle positions into array
			var positions = new Vector3[particleSystem.ParticleCount];
			var fade = new float[particleSystem.ParticleCount];
			int i = 0;
			foreach (var particle in particleSystem.Particles)
			{
				var p = particle as Particle;
				var age = time - p.CreationTime;
				fade[i] = 1f - age / p.LifeTime;
				positions[i] = p.Position;
				++i;
			}

			shaderWatcher.CheckForShaderChange();
			particles.SetAttribute(shaderWatcher.Shader.GetAttributeLocation("position"), positions, VertexAttribPointerType.Float, 3);
			particles.SetAttribute(shaderWatcher.Shader.GetAttributeLocation("fade"), fade, VertexAttribPointerType.Float, 1);
		}

		public void Render(Matrix4 camera)
		{
			//setup blending equation Color = Color_s · alpha + Color_d
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.DepthMask(false);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.PointSprite);
			GL.Enable(EnableCap.VertexProgramPointSize);

			var shader = shaderWatcher.Shader;
			shader.Activate();
			GL.UniformMatrix4(shader.GetUniformLocation("camera"), true, ref camera);
			//GL.Uniform1(shader.GetUniformLocation("texParticle"), 0);
			texStar.Activate();
			particles.DrawArrays(PrimitiveType.Points, particleSystem.ParticleCount);
			texStar.Deactivate();
			shader.Deactivate();

			GL.Disable(EnableCap.VertexProgramPointSize);
			GL.Disable(EnableCap.PointSprite);
			GL.Disable(EnableCap.Blend);
			GL.DepthMask(true);
		}

		private ShaderFileDebugger shaderWatcher;
		private Texture texStar;
		private VAO particles = new VAO();
		private ParticleSystem<Particle> particleSystem = new ParticleSystem<Particle>(1000);
		private Random random = new Random();
	}
}
