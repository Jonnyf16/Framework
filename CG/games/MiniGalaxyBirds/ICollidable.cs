﻿using Framework;
using Geometry;

namespace MiniGalaxyBirds
{
	interface ICollidable
	{
		Box2D Frame { get; }
	}
}