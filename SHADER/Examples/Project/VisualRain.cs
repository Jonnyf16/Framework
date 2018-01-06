using DMS.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Example
{
	public class VisualRain
	{
		public VisualRain(Vector3 rainStartPosition, Vector3 windDirection)
		{
            this.rainState = false;
            this.rainPosition = rainStartPosition;
            this.windDirection = windDirection;
            texStar = TextureLoader.FromBitmap(Resourcen.water_splash);
			particleSystem.ReleaseInterval = 0.003f;
			particleSystem.OnParticleCreate += Create;
			particleSystem.OnAfterParticleUpdate += OnAfterParticleUpdate;
		}

		public Particle Create(float creationTime)
		{
			var p = new Particle(creationTime);
            // create only when rainState is on
            if (this.rainState)
            {
                p.LifeTime = 5f;
                Func<float> Rnd01 = () => (float)random.NextDouble();
                Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;
                //around emitter position
                p.Position = rainPosition + new Vector3(RndCoord(), RndCoord(), RndCoord()) * .1f;
                //start speed
                p.Velocity = Vector3.Zero;
                //downward gravity
                p.Acceleration = new Vector3(0, -1.0f, 0) + this.windDirection;
            }
            else
                p.LifeTime = 0f;
			return p;
		}

		private void OnAfterParticleUpdate(Particle particle)
		{
			Func<float> Rnd01 = () => (float)random.NextDouble();
			Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;

			//if collision with ground plane
			if (particle.Position.Y < 0.2)
			{
                //slightly different upward vectors
                var direction = new Vector3(RndCoord(), RndCoord(), RndCoord()).Normalized();
				var speed = particle.Velocity.Length;
				//random perturb velocity to get more water like effects
				particle.Velocity = direction * speed * 0.7f;
			}
		}

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderWaterfall = shader;
		}

		public void Update(float time, bool rainState, Vector3 rainPosition, Vector3 windDirection)
		{
			if (ReferenceEquals(shaderWaterfall, null)) return;
            // update parameters
            this.rainState = rainState;
            this.rainPosition = rainPosition;
            this.windDirection = windDirection;
            particleSystem.Update(time);
			//gather all active particle positions into array
			var positions = new Vector3[particleSystem.ParticleCount];
			var fade = new float[particleSystem.ParticleCount];
			int i = 0;
			foreach (var particle in particleSystem.Particles)
			{
				//fading with age effect
				var age = time - particle.CreationTime;
				fade[i] = 1f - age / particle.LifeTime * 1.5f;
                particle.Acceleration = new Vector3(0, -1.0f, 0) + this.windDirection;
                positions[i] = particle.Position;
				++i;
			}

			particles.SetAttribute(shaderWaterfall.GetAttributeLocation("position"), positions, VertexAttribPointerType.Float, 3);
			particles.SetAttribute(shaderWaterfall.GetAttributeLocation("fade"), fade, VertexAttribPointerType.Float, 1);
		}

		public void Render(Matrix4 camera)
		{
			if (ReferenceEquals(shaderWaterfall, null)) return;
			//setup blending equation Color = Color_s · alpha + Color_d
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.DepthMask(false);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.PointSprite);
			GL.Enable(EnableCap.VertexProgramPointSize);

			shaderWaterfall.Activate();
			GL.UniformMatrix4(shaderWaterfall.GetUniformLocation("camera"), true, ref camera);
			GL.Uniform1(shaderWaterfall.GetUniformLocation("pointSize"), 0.3f);
			//GL.Uniform1(shader.GetUniformLocation("texParticle"), 0);
			texStar.Activate();
			particles.DrawArrays(PrimitiveType.Points, particleSystem.ParticleCount);
			texStar.Deactivate();
			shaderWaterfall.Deactivate();

			GL.Disable(EnableCap.VertexProgramPointSize);
			GL.Disable(EnableCap.PointSprite);
			GL.Disable(EnableCap.Blend);
			GL.DepthMask(true);
		}

		public static readonly string ShaderName = nameof(shaderWaterfall);
		private Shader shaderWaterfall;

		private Texture texStar;
		private VAO particles = new VAO();
		private ParticleSystem<Particle> particleSystem = new ParticleSystem<Particle>(1000000);
		private Random random = new Random();

        // new shit
        private bool rainState;
        private Vector3 rainPosition;
        private Vector3 windDirection;

    }
}
