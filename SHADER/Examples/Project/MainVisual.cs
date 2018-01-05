using DMS.OpenGL;
using DMS.Geometry;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System;

namespace Example
{
	public class MainVisual
	{
		public CameraOrbit OrbitCamera { get { return camera; } }

		public MainVisual()
		{
            // plane setup
            this.plane = new VisualPlane();

            // wind setup
            this.windDirection = new Vector3(0.0f);

            // rain setup
            this.rainState = false;
            this.rainPosition = new Vector3(-.5f, 1f, -.5f);
            this.visualRain = new VisualRain(this.rainPosition, this.windDirection);

            // cloud setup
            this.cloudTranslation = new Vector3(.0f, .1f, .0f);
            this.visualCloud = new VisualObjects(this.rainPosition);

            // candle setup
            this.candleState = true;
            this.candlePosition = new Vector3(.2f, 0, 0);
            this.candleThickness = .15f;

            // smoke setup
            this.smokeState = false;
            this.visualSmoke = new VisualSmoke(Vector3.Zero, this.windDirection);

            // camera setup
            this.camera.FarClip = 40;
            this.camera.Distance = 2;
            this.camera.FovY = 70;
            this.camera.Elevation = 15;

			GL.Enable(EnableCap.DepthTest);
		}

		public void ShaderChanged(string name, Shader shader)
		{
            this.visualSmoke.ShaderChanged(name, shader);
            this.visualRain.ShaderChanged(name, shader);
            this.visualCloud.ShaderChanged(name, shader);
        }

		public void Update(float time)
		{
            KeyboardEvent();
            checkRainCandleCollision();
            glTimerUpdate.Activate(QueryTarget.TimeElapsed);
            this.visualSmoke.Update(time, this.smokeState, this.candlePosition, this.windDirection);
            this.visualRain.Update(time, this.rainState, this.rainPosition, this.windDirection);
            this.visualCloud.Update(this.rainState, this.rainPosition + this.cloudTranslation);
			glTimerUpdate.Deactivate();
		}

		public void Render()
		{
			glTimerRender.Activate(QueryTarget.TimeElapsed);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            var cam = camera.CalcMatrix().ToOpenTK();
            this.visualCloud.Render(cam);
            this.plane.Draw(cam);

            this.visualSmoke.Render(cam);
            this.visualRain.Render(cam);
            glTimerRender.Deactivate();

			Console.Write("Update:");
			Console.Write(glTimerUpdate.ResultLong / 1e6);
			Console.Write("msec  Render:");
			Console.Write(glTimerRender.ResultLong / 1e6);
			Console.WriteLine("msec");
		}

        private void checkRainCandleCollision()
        {
            if (candleState && rainState)
            {
                // check if rain is above candle
                if (((rainPosition[0] + windDirection[0]) > (candlePosition[0] - candleThickness)) && ((rainPosition[0] + windDirection[0]) < candlePosition[0] + candleThickness) &&
                    ((rainPosition[2] + windDirection[2]) > (candlePosition[2] - candleThickness)) && ((rainPosition[2] + windDirection[2]) < candlePosition[2] + candleThickness))
                    this.smokeState = true;
                else
                    this.smokeState = false;
            }
            //Console.WriteLine("RainPosition: [{0}, {1}]", rainPosition[0], rainPosition[2]);
            //Console.WriteLine("CandlePosition: [{0}, {1}]", candlePosition[0], candlePosition[2]);
            //Console.WriteLine("Rain meets Candle: {0}", smokeState);
        }

        private void KeyboardEvent()
        {
            // Get current state
            keyboardState = OpenTK.Input.Keyboard.GetState();
            float movingSpeed = 0.01f;

            // wind reset
            if (keyboardState[Key.O])
                this.windDirection = new Vector3(0.0f);

            // wind strength
            if (keyboardState[Key.L])
                this.windDirection[0] += movingSpeed;
            else if (keyboardState[Key.J])
                this.windDirection[0] -= movingSpeed;
            else if (keyboardState[Key.K])
                this.windDirection[2] += movingSpeed;
            else if (keyboardState[Key.I])
                this.windDirection[2] -= movingSpeed;

            // rain toggle
            if (keyboardState[Key.Q])
                this.rainState = true;
            else if (keyboardState[Key.E])
                this.rainState = false;

            // rain movement
            if (rainState)
            {
                if (keyboardState[Key.D])
                    this.rainPosition[0] += movingSpeed;
                else if (keyboardState[Key.A])
                    this.rainPosition[0] -= movingSpeed;
                else if (keyboardState[Key.S])
                    this.rainPosition[2] += movingSpeed;
                else if (keyboardState[Key.W])
                    this.rainPosition[2] -= movingSpeed;
            }

            // smoke toggle
            if (keyboardState[Key.R])
                this.smokeState = true;
            if (keyboardState[Key.T])
                this.smokeState = false;
        }

        private CameraOrbit camera = new CameraOrbit();

        private VisualPlane plane;
		private readonly VisualSmoke visualSmoke;
		private readonly VisualRain visualRain;
        private readonly VisualObjects visualCloud;
		private QueryObject glTimerRender = new QueryObject();
		private QueryObject glTimerUpdate = new QueryObject();

        // new shit
        private Random random = new Random();
        KeyboardState keyboardState;
        private bool rainState;
        private Vector3 rainPosition;
        private Vector3 cloudTranslation;
        private bool candleState;
        private Vector3 candlePosition;
        private float candleThickness;
        private bool smokeState;
        private Vector3 windDirection;

    }
}
