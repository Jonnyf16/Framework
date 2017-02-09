﻿using Framework;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ShaderDebugging;
using System;

namespace Example
{
	public class VisualWaterfall
	{
		public VisualWaterfall(Vector3 emitterPos)
		{
			this.emitterPos = emitterPos;
			texStar = TextureLoader.FromBitmap(Resourcen.water_splash);
			shaderWatcher = new ShaderFileDebugger("../../ParticleSystemExample/Resources/smoke.vert"
				, "../../ParticleSystemExample/Resources/smoke.frag"
				, Resourcen.smoke_vert, Resourcen.smoke_frag);

			particleSystem.ReleaseInterval = 0.03f;
			particleSystem.OnParticleCreate += Create;
			particleSystem.OnAfterParticleUpdate += OnAfterParticleUpdate;
		}

		public Particle Create(float creationTime)
		{
			var p = new Particle(creationTime);
			p.LifeTime = 10f;
			Func<float> Rnd01 = () => (float)random.NextDouble();
			Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;
			//around emitter position
			p.Position = emitterPos + new Vector3(RndCoord(), RndCoord(), RndCoord()) * .1f;
			//speed between [.002, .004]
			p.Velocity = Vector3.Zero;
			//gravity
			p.Acceleration = new Vector3(0, -.4f, 0);
			return p;
		}

		private Vector3 Reflect(Vector3 incoming, Vector3 normal)
		{
			return 2 * Vector3.Dot(normal, -incoming) *normal + incoming;
		}

		private void OnAfterParticleUpdate(Particle particle)
		{
			Func<float> Rnd01 = () => (float)random.NextDouble();
			Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;

			if (particle.Position.Y < 0)
			{
				//slightly different upward vectors
				var direction = new Vector3(RndCoord(), Rnd01(), RndCoord()).Normalized();
				var speed = particle.Velocity.Length;
				particle.Velocity = direction * speed * 0.7f;
			}
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
				var age = time - particle.CreationTime;
				fade[i] = 1f - age / particle.LifeTime;
				positions[i] = particle.Position;
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
			shader.BeginUse();
			GL.UniformMatrix4(shader.GetUniformLocation("camera"), true, ref camera);
			//GL.Uniform1(shader.GetUniformLocation("texParticle"), 0);
			texStar.BeginUse();
			particles.DrawArrays(PrimitiveType.Points, particleSystem.ParticleCount);
			texStar.EndUse();
			shader.EndUse();

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
		private readonly Vector3 emitterPos;
	}
}
