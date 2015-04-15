using System;
using UnityEngine;

public class CustomPerlinNoise : PerlinNoise
{
	private Vector3 size;

	public CustomPerlinNoise(uint seed, uint octave, float persistence, float frequency, Vector3 size) : base(seed, octave, persistence)
	{
		this.size.x = size.x / frequency;
		this.size.y = size.y / frequency;
		this.size.z = size.z / frequency;
	}

	public float Get(int x, int y, int z)
	{
		return base.Get3D ((float)x / size.x, (float)y / size.y, (float)z / size.z);
	}
}
