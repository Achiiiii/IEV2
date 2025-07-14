using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostProcessBuilder
{
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string targetPath)
	{
		if(target==BuildTarget.StandaloneWindows64)
		{
			var dataPath = targetPath.Replace(".exe", "_Data");
			var packagePath = Path.GetFullPath("Packages/com.redpill.videopose/");

			void CopyAsset(string filename)
			{
				File.Copy(Path.Combine(packagePath, "Runtime/Plugins/x86_64/"+filename), Path.Combine(dataPath, "Plugins/x86_64/"+filename), true);
			}
			CopyAsset("license.dat");
			CopyAsset("WindowsCommon.vp3d");
			CopyAsset("rnnv2.onnx");
			try{
				File.Move(Path.Combine(dataPath, "Plugins/x86_64/DirectML.dll"), Path.Combine(dataPath, "../DirectML.dll"));
			}catch (Exception){}
		}
	}
}