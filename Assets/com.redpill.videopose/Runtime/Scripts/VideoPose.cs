using System;
using System.Runtime.InteropServices;
using UnityEngine;
[Serializable]
public class Pose
{
	public long id;
	public Quaternion[] localRotations;
	public Vector3[] localPositions;
	public Matrix4x4[] globalTransforms;
	public Vector3[] keyPoints;
	public Vector3[] localPositionsAligned;
}
public class VideoPose
{
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	struct Void{}
	private TResult TryCatch<TResult>(Func<TResult> f)
	{
#if UNITY_EDITOR
		try{return f();}
		catch(EntryPointNotFoundException){}
		catch(DllNotFoundException){}
		return default;
#else
		return f();
#endif
	}
	
	public delegate void OnPoseDelegate(Pose pose);
	private delegate void OnPoseDataDelegate(
		long id,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 62)]Quaternion[] localRotations,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 62)]Vector3[] localPositions,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 62)]Matrix4x4[] globalTransforms,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 27)]Vector3[] keyPoints,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = 62)]Vector3[] localPositionsAligned,
		IntPtr userdata
	);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	private const string lib = "VideoPose";
#else
	private const string lib = "__Internal";
#endif
	[DllImport(lib)]
	private static extern bool VideoPose_Start([MarshalAs(UnmanagedType.FunctionPtr)]OnPoseDataDelegate callback, IntPtr data);
	[DllImport(lib)]
	private static extern Void VideoPose_SetUpperBodyMode(bool set);
	[DllImport(lib)]
	private static extern Void VideoPose_Stop();
	[DllImport(lib)]
	private static extern Void VideoPose_Reset(float fovy, bool detect_apose);
	[DllImport(lib)]
	private static extern Void VideoPose_PushFrame(ulong id, IntPtr data, int w, int h, int c);

	[AOT.MonoPInvokeCallback(typeof(OnPoseDataDelegate))]
	private static void OnPoseData(
		long id, 
		Quaternion[] localRotations,
		Vector3[] localPositions,
		Matrix4x4[] globalTransforms,
		Vector3[] keyPoints,
		Vector3[] localPositionsAligned,
		IntPtr data
	)
	{
		var handle = GCHandle.FromIntPtr(data);
		VideoPose videoPose = (VideoPose)handle.Target;
		videoPose.callback(new Pose{
			id=id,
			localRotations=localRotations,
			localPositions=localPositions,
			globalTransforms=globalTransforms,
			keyPoints=keyPoints,
			localPositionsAligned=localPositionsAligned
		});
	}
	private OnPoseDelegate callback;
	private GCHandle handle;
	public VideoPose() => handle = GCHandle.Alloc(this);
	~VideoPose() => handle.Free();
	public bool Start(OnPoseDelegate callback)
	{
		this.callback = callback;
		return TryCatch(()=>VideoPose_Start(OnPoseData, GCHandle.ToIntPtr(handle)));
	}
	public void SetUpperBodyMode(bool set) => TryCatch(()=>VideoPose_SetUpperBodyMode(set));
	public void Stop() => TryCatch(()=>VideoPose_Stop());
	public void Reset(float fovy=50f, bool detect_apose=true) => TryCatch(()=>VideoPose_Reset(fovy, detect_apose));
	public void PushFrame(ulong id, IntPtr data, int w, int h, int c) => TryCatch(()=>VideoPose_PushFrame(id, data, w, h, c)); 
}