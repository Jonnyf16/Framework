using DMS.Application;
using DMS.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Example
{
	public class VisualSmoke
	{
		public VisualSmoke(Vector3 smokePosition, Vector3 windDirection)
		{
            this.smokeState = false;
			this.smokePosition = smokePosition;
			this.windDirection = windDirection;
			texStar = TextureLoader.FromBitmap(Resourcen.smoke); //resourceProvider.Get<Texture>(nameof(Resourcen.smoke)).Value;
			particleSystem.ReleaseInterval = 0.02f;
			particleSystem.OnParticleCreate += Create;
		}

		public Particle Create(float creationTime)
		{
			var p = new Particle(creationTime);
            if (smokeState)
            {
                p.LifeTime = 10f;
                Func<float> Rnd01 = () => (float)random.NextDouble();
                Func<float> RndCoord = () => (Rnd01() - 0.5f) * 2.0f;
                //start at emitter position
                p.Position = this.smokePosition;
                //slightly different upward vectors
                var direction = new Vector3(0.3f * RndCoord(), 1, 0.3f * RndCoord());
                //speed between [.2, .4]
                p.Velocity = (.2f + .2f * Rnd01()) * direction;
                p.Acceleration = this.windDirection;
            }
            else
                p.LifeTime = 0f;
			return p;
		}

		public void ShaderChanged(string name, Shader shader)
		{
			if (ShaderName != name) return;
			this.shaderSmoke = shader;
		}

		public void Update(float time, bool smokeState, Vector3 smokePosition)
		{
            this.smokeState = smokeState;
            this.smokePosition = smokePosition;

            if (ReferenceEquals(shaderSmoke, null)) return;
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

			particles.SetAttribute(shaderSmoke.GetAttributeLocation("position"), positions, VertexAttribPointerType.Float, 3);
			particles.SetAttribute(shaderSmoke.GetAttributeLocation("fade"), fade, VertexAttribPointerType.Float, 1);
		}

		public void Render(Matrix4 camera)
		{
			if (ReferenceEquals(shaderSmoke, null)) return;
			//setup blending equation Color = Color_s · alpha + Color_d
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.DepthMask(false);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.PointSprite);
			GL.Enable(EnableCap.VertexProgramPointSize);

			shaderSmoke.Activate();
			GL.UniformMatrix4(shaderSmoke.GetUniformLocation("camera"), true, ref camera);
			//GL.Uniform1(shader.GetUniformLocation("texParticle"), 0);
			texStar.Activate();
			particles.DrawArrays(PrimitiveType.Points, particleSystem.ParticleCount);
			texStar.Deactivate();
			shaderSmoke.Deactivate();

			GL.Disable(EnableCap.VertexProgramPointSize);
			GL.Disable(EnableCap.PointSprite);
			GL.Disable(EnableCap.Blend);
			GL.DepthMask(true);
		}

		public static readonly string ShaderName = nameof(shaderSmoke);
		private Shader shaderSmoke;

		private Texture texStar;
		private VAO particles = new VAO();
		private ParticleSystem<Particle> particleSystem = new ParticleSystem<Particle>(1000);
		private Random random = new Random();

        //private IResourceProvider resourceProvider;

        // new shit
        private Vector3 windDirection;
        private bool smokeState;
        private Vector3 smokePosition;
	}
}
