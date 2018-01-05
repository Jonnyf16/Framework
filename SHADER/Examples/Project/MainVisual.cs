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
			plane = new VisualPlane();
            planeTex = TextureLoader.FromBitmap(Resourcen.tablecloth);

            // wind setup
            this.windDirection = new Vector3(0.0f);

            // rain setup
            this.rainState = false;
            this.rainPosition = new Vector3(-.5f, 1, -.5f);
            this.visualRain = new VisualRain(this.rainPosition, this.windDirection);

            // cloud setup
            this.visualCloud = new VisualCloud(this.rainPosition);

            // candle setup
            this.candleState = true;
            this.candlePosition = new Vector3(.2f, 0, 0);
            this.candleThickness = .15f;

            // smoke setup
            smokeState = false;
            this.visualSmoke = new VisualSmoke(Vector3.Zero, this.windDirection);

            // camera setup
			camera.FarClip = 20;
			camera.Distance = 2;
			camera.FovY = 70;
			camera.Elevation = 15;

			GL.Enable(EnableCap.DepthTest);
		}

		public void ShaderChanged(string name, Shader shader)
		{
			visualSmoke.ShaderChanged(name, shader);
			visualRain.ShaderChanged(name, shader);
            visualCloud.ShaderChanged(name, shader);
        }

		public void Update(float time)
		{
            KeyboardEvent();
            checkRainCandleCollision();
            glTimerUpdate.Activate(QueryTarget.TimeElapsed);
			visualSmoke.Update(time, this.smokeState, this.candlePosition, this.windDirection);
            visualRain.Update(time, this.rainState, this.rainPosition, this.windDirection);
            visualCloud.Update(this.rainState, this.rainPosition);
			glTimerUpdate.Deactivate();
		}

		public void Render()
		{
			glTimerRender.Activate(QueryTarget.TimeElapsed);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            var cam = camera.CalcMatrix().ToOpenTK();            
            visualCloud.Render(cam);
            planeTex.Activate();
            plane.Draw(cam);
            planeTex.Deactivate();
            visualSmoke.Render(cam);
            visualRain.Render(cam);
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
                if ((rainPosition[0]+windDirection[0] > (candlePosition[0] - candleThickness)) && (rainPosition[0] + windDirection[0] < candlePosition[0] + candleThickness) &&
                    (rainPosition[2] + windDirection[2] > (candlePosition[2] - candleThickness)) && (rainPosition[2] + windDirection[2] < candlePosition[2] + candleThickness))
                    smokeState = true;
                else
                    smokeState = false;
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

            // wind strength
            if (keyboardState[Key.Right])
                windDirection[0] += movingSpeed;
            else if (keyboardState[Key.Left])
                windDirection[0] -= movingSpeed;
            else if (keyboardState[Key.Down])
                windDirection[2] += movingSpeed;
            else if (keyboardState[Key.Up])
                windDirection[2] -= movingSpeed;

            // rain toggle
            if (keyboardState[Key.Q])
                rainState = true;
            else if (keyboardState[Key.E])
                rainState = false;

            // rain movement
            if (rainState)
            {
                if (keyboardState[Key.D])
                    rainPosition[0] += movingSpeed;
                else if (keyboardState[Key.A])
                    rainPosition[0] -= movingSpeed;
                else if (keyboardState[Key.S])
                    rainPosition[2] += movingSpeed;
                else if (keyboardState[Key.W])
                    rainPosition[2] -= movingSpeed;
            }

            // smoke toggle
            if (keyboardState[Key.R])
                smokeState = true;
            if (keyboardState[Key.T])
                smokeState = false;
        }

        private CameraOrbit camera = new CameraOrbit();

        private VisualPlane plane;
		private readonly VisualSmoke visualSmoke;
		private readonly VisualRain visualRain;
        private readonly VisualCloud visualCloud;
		private QueryObject glTimerRender = new QueryObject();
		private QueryObject glTimerUpdate = new QueryObject();

        // new shit
        private Texture planeTex;
        KeyboardState keyboardState;
        private bool rainState;
        private Vector3 rainPosition;
        private bool candleState;
        private Vector3 candlePosition;
        private float candleThickness;
        private bool smokeState;
        private Vector3 windDirection;

    }
}
