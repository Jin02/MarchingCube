using System;
using UnityEngine;

public class PerlinNoise
{
	private uint 	octaveCount;
	private uint 	seed;

	private float	persistence;

	public PerlinNoise(uint seed, uint octave, float persistence)
	{
		this.seed 			= seed;
		this.octaveCount 	= octave;
		this.persistence 	= persistence;
	}

	private float PseudoRandom2D(int x, int y) //Pseudo random
	{
		long r = x + y * seed;
		r = (r << 13) ^ r;
		r *= r * r * 15731 + 789221;
		float result = 1f - ((r + 1376312589) & 0x7fffffff) / 1073741824f;
		return result;
	}

	private float PseudoRandom3D(int x, int y, int z)
	{
		long r = x + y * seed + z * seed * seed;
		r = (r << 13) ^ r;
		r *= r * r * 15731 + 789221;
		float result = 1f - ((r + 1376312589) & 0x7fffffff) / 1073741824f;
		return result;
	}

	private float Interpolation(float from, float to, float t)
	{
		float tpi = t * (float)System.Math.PI;
		float tcos = (1 - (float)System.Math.Cos(tpi)) * 0.5f;
		return from * (1 - tcos) + to * tcos;
	}

	private float Noise2D(float x, float y)
	{
		int 	integerX 	= (int)x;
		float 	fractionalX = x - (float)integerX;

		int 	integerY 	= (int)y;
		float 	fractionalY = y - (float)integerY;

		float tl = PseudoRandom2D (integerX, integerY);
		float tr = PseudoRandom2D (integerX + 1, integerY);
		float bl = PseudoRandom2D (integerX, integerY + 1);
		float br = PseudoRandom2D (integerX + 1, integerY + 1);

		float topIntp = Interpolation (tl, tr, fractionalX);
		float botIntp = Interpolation (bl, br, fractionalX);
            
		return Interpolation (topIntp, botIntp, fractionalY);
	}

	private float Noise3D(float x, float y, float z)
	{
		int 	integerX 	= (int)x;
		float 	fractionalX = x - (float)integerX;
			
		int 	integerY 	= (int)y;
		float 	fractionalY = y - (float)integerY;

		int 	integerZ 	= (int)z;
		float 	fractionalZ = z - (float)integerZ;

		float backTopLeft 	= PseudoRandom3D(integerX, integerY, integerZ);
		float backTopRight 	= PseudoRandom3D(integerX+1, integerY, integerZ);
		float backBotLeft 	= PseudoRandom3D(integerX, integerY+1, integerZ);
		float backBotRight 	= PseudoRandom3D(integerX+1, integerY+1, integerZ);

		float frontTopLeft 	= PseudoRandom3D(integerX,   integerY, 	 integerZ+1);
		float frontTopRight = PseudoRandom3D(integerX+1, integerY, 	 integerZ+1);
		float frontBotLeft 	= PseudoRandom3D(integerX, 	 integerY+1, integerZ+1);
		float frontBotRight = PseudoRandom3D(integerX+1, integerY+1, integerZ+1);

		float backTop = Interpolation(backTopLeft, backTopRight, fractionalX);
		float backBot = Interpolation(backBotLeft, backBotRight, fractionalX);

		float frontTop = Interpolation(frontTopLeft, frontTopRight, fractionalX);
		float frontBot = Interpolation(frontBotLeft, frontBotRight, fractionalX);

		float back 	= Interpolation(backTop, backBot, fractionalY);
		float front = Interpolation(frontTop, frontBot, fractionalY);

		float allInterP = Interpolation(back, front, fractionalZ);

		return allInterP;
	}

	public float Get2D(float x, float y) //return -1 ~ 1 float value (maybe...)
	{
		float total = 0.0f;

		for (int i=0; i<this.octaveCount; ++i)
		{
			float freq 	= 2 << i;
			float amp 	= Mathf.Pow(this.persistence, i);

			total += Noise2D((float)x * freq, (float)y * freq) * amp;
		}

		return total;
	}

	public float Get3D(float x, float y, float z) //return -1 ~ 1 float value (maybe...)
	{
		float total = 0.0f;
		float freq = 1.0f;
		float amp = 1.0f;
			
		for (int i=0; i<this.octaveCount; ++i)
		{
			total += Noise3D((float)x * freq, (float)y * freq, (float)z * freq) * amp;

			freq = 2 << i;
			amp *= this.persistence;
		}

		return total;
	}

}
