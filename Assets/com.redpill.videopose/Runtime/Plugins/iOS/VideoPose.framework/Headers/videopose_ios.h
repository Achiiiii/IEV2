#pragma once

extern "C"
{
	typedef void (*PoseDataReadyCallback)(
		unsigned long long id,
		const float* local_rotations,
		const float* local_positions,
		const float* global_transforms,
		const float* keypoints,
		const float* local_positions_align,
		void* userdata
	);
	bool VideoPose_Start(PoseDataReadyCallback callback, void* userdata);
	void VideoPose_SetUpperBodyMode(bool);
	void VideoPose_Stop();
	void VideoPose_Reset(float fovy, bool detect_apose);
	void VideoPose_PushFrame(unsigned long long id, void* data, int w, int h, int c);
}
