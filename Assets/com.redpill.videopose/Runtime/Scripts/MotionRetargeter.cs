using System;
using UnityEngine;

public interface IMotionSource
{
	public Pose GetPose();
}

public class MotionRetargeter : MonoBehaviour
{
	public bool align = false;
	private const int NJ = 62;
	private Quaternion[] tposeRotations = new Quaternion[NJ];
	private Transform[] targetBones = new Transform[NJ];
	private IMotionSource motionSource;
	public Animator avatar;
	private HumanBodyBones[] boneId = {
		HumanBodyBones.Hips,
		HumanBodyBones.RightUpperLeg,
		HumanBodyBones.RightLowerLeg,
		HumanBodyBones.RightFoot,
		HumanBodyBones.LeftUpperLeg,
		HumanBodyBones.LeftLowerLeg,
		HumanBodyBones.LeftFoot,
		HumanBodyBones.Spine,
		HumanBodyBones.Chest,
		HumanBodyBones.UpperChest,
		HumanBodyBones.Neck,
		HumanBodyBones.Head,
		HumanBodyBones.LeftShoulder,
		HumanBodyBones.LeftUpperArm,
		HumanBodyBones.LeftLowerArm,
		HumanBodyBones.LeftHand,
		HumanBodyBones.RightShoulder,
		HumanBodyBones.RightUpperArm,
		HumanBodyBones.RightLowerArm,
		HumanBodyBones.RightHand,
		HumanBodyBones.RightToes,
		HumanBodyBones.LeftToes,

		HumanBodyBones.RightThumbProximal,
		HumanBodyBones.RightThumbIntermediate,
		HumanBodyBones.RightThumbDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.RightIndexProximal,
		HumanBodyBones.RightIndexIntermediate,
		HumanBodyBones.RightIndexDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.RightMiddleProximal,
		HumanBodyBones.RightMiddleIntermediate,
		HumanBodyBones.RightMiddleDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.RightRingProximal,
		HumanBodyBones.RightRingIntermediate,
		HumanBodyBones.RightRingDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.RightLittleProximal,
		HumanBodyBones.RightLittleIntermediate,
		HumanBodyBones.RightLittleDistal,
			HumanBodyBones.LastBone,

		HumanBodyBones.LeftThumbProximal,
		HumanBodyBones.LeftThumbIntermediate,
		HumanBodyBones.LeftThumbDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.LeftIndexProximal,
		HumanBodyBones.LeftIndexIntermediate,
		HumanBodyBones.LeftIndexDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.LeftMiddleProximal,
		HumanBodyBones.LeftMiddleIntermediate,
		HumanBodyBones.LeftMiddleDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.LeftRingProximal,
		HumanBodyBones.LeftRingIntermediate,
		HumanBodyBones.LeftRingDistal,
			HumanBodyBones.LastBone,
		HumanBodyBones.LeftLittleProximal,
		HumanBodyBones.LeftLittleIntermediate,
		HumanBodyBones.LeftLittleDistal,
			HumanBodyBones.LastBone,
	};

	void Start()
	{
		motionSource = GetComponent<IMotionSource>();
		// init_target_bones
		if(avatar==null)
			avatar = GetComponent<Animator>();
		for (int i = 0; i < NJ; i++)
			targetBones[i] = boneId[i] != HumanBodyBones.LastBone ? avatar.GetBoneTransform(boneId[i]) : null;
		// scan_bones
		for (int i = 0; i < targetBones.Length; i++)
		{
			Transform bone = targetBones[i];
			tposeRotations[i] = bone != null ? Quaternion.Inverse(avatar.gameObject.transform.rotation) * bone.rotation : Quaternion.identity;
		}
	}

	void Update()
	{
		// pose
		var sourcePose = motionSource.GetPose();
		if(sourcePose == null)
			return;
		var localPositions = align ? sourcePose.localPositionsAligned : sourcePose.localPositions;
		if(localPositions!=null)
		{
			var localPositionsCopy = new Vector3[localPositions.Length];
			Array.Copy(localPositions, localPositionsCopy, localPositionsCopy.Length);
			var scale = avatar.transform.localScale;
			var root_inv_scale = targetBones[0].localToWorldMatrix.inverse.lossyScale;
			localPositionsCopy[0].x *= -1;
			localPositionsCopy[0].Scale(scale);
			localPositionsCopy[0].Scale(root_inv_scale);
			targetBones[0].localPosition = localPositionsCopy[0];
		}
		if(sourcePose.globalTransforms!=null)
		{
			var root_inv = Quaternion.Inverse(avatar.gameObject.transform.rotation);
			for (int i = 0; i < NJ; i++)
			{
				var globalRotation = sourcePose.globalTransforms[i].rotation;
				globalRotation.x *= -1;
				globalRotation.w *= -1;
				if(i==3 || i==6)
					globalRotation = globalRotation * Quaternion.Euler(new Vector3(-30, 0, 0));
				if (targetBones[i] != null)
				{
					var localRotation = Quaternion.Inverse(root_inv * targetBones[i].parent.rotation) * globalRotation * tposeRotations[i];
					targetBones[i].localRotation = localRotation;
				}
			}
		}
	}
}
