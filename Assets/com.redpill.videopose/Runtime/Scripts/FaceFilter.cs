using System;

public class FaceFilter
{
	private long last_time = 0;
	private long next_blink_time = 0;

	private const int NM = 18;
	private float[] weights = new float[NM];
	private float[] velocities = new float[NM];

	public float halflife_to_damping(float halflife, float eps = 1e-5f)
	{
		return (4.0f * 0.69314718056f) / (halflife + eps);
	}

	public float fast_negexp(float x)
	{
		return 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
	}

	public void simple_spring_damper_implicit(ref float x, ref float v, float x_goal, float halflife, float dt)
	{
		float y = halflife_to_damping(halflife) / 2.0f;
		float j0 = x - x_goal;
		float j1 = v + j0 * y;
		float eydt = fast_negexp(y * dt);

		x = eydt * (j0 + j1 * dt) + x_goal;
		v = eydt * (v - j1 * y * dt);
	}


	public float[] Run(int vis_out)
	{
		var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		var dt = now - last_time;
		last_time = now;

		next_blink_time -= dt;
		bool blinking = false;
		if (next_blink_time <= 0)
		{
			blinking = true;
			if (next_blink_time < -100)
			{
				next_blink_time = new Random().Next(3000, 6000);
				blinking = false;
			}
		}

		var morphs = new float[NM];

		if (vis_out > 0)
			morphs[vis_out] = 1;
		if (blinking)
			morphs[13] = 1;

		var half_lifes = new float[18];
		for (int i = 0; i < half_lifes.Length; i++) half_lifes[i] = 0.035f;
		//Array.Fill(half_lifes, 0.035f);
		if (vis_out == 0)
			for (int i = 0; i < half_lifes.Length; i++) half_lifes[i] = 0.04f;
		//Array.Fill(half_lifes, 0.04f);
		half_lifes[4] = (vis_out == 4) ? 0.03f : 0.04f;
		half_lifes[5] = (vis_out == 5) ? 0.03f : 0.04f;
		half_lifes[8] = (vis_out == 8) ? 0.025f : 0.04f;
		half_lifes[10] = (vis_out == 10) ? 0.03f : 0.04f;
		half_lifes[11] = (vis_out == 11) ? 0.025f : 0.03f;
		half_lifes[12] = (vis_out == 12) ? 0.025f : 0.03f;
		for (var i = 0; i < 13; ++i)
			simple_spring_damper_implicit(ref weights[i], ref velocities[i], morphs[i], half_lifes[i], dt / 1000f);
		//simple_spring_damper_implicit(weights[i], vels[i], goals[i], 0.023f, dt);
		simple_spring_damper_implicit(ref weights[13], ref velocities[13], morphs[13], 0.01f, dt / 1000f);
		return weights;
	}

}
