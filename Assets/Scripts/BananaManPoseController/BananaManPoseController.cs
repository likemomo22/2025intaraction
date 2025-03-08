using System.Collections.Generic;
using UnityEngine;

public class BananaManPoseController : MonoBehaviour
{
    public Animator animator; // Banana Man 的 Animator
    public Transform head;
    public Transform leftShoulder, rightShoulder;
    public Transform leftElbow, rightElbow;
    public Transform leftWrist, rightWrist;
    public Transform leftHip, rightHip;
    public Transform leftKnee, rightKnee;
    public Transform leftAnkle, rightAnkle;

    private Dictionary<int, Transform> boneMapping;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // 绑定 MediaPipe 关键点到 Unity 的骨骼
        boneMapping = new Dictionary<int, Transform>
        {
            { 0, head },
            { 11, leftShoulder },
            { 12, rightShoulder },
            { 13, leftElbow },
            { 14, rightElbow },
            { 15, leftWrist },
            { 16, rightWrist },
            { 23, leftHip },
            { 24, rightHip },
            { 25, leftKnee },
            { 26, rightKnee },
            { 27, leftAnkle },
            { 28, rightAnkle }
        };
    }

    public void UpdatePose(Landmark[] landmarks)
    {
        Dictionary<int, Vector3> worldPositions = new Dictionary<int, Vector3>();

        // 将所有 landmarks 转换为 Unity 世界坐标
        foreach (var landmark in landmarks)
        {
            if (boneMapping.ContainsKey(landmark.idx))
            {
                worldPositions[landmark.idx] = ConvertLandmarkToWorldPosition(landmark);
            }
        }

        // 更新骨骼位置
        foreach (var kvp in boneMapping)
        {
            if (worldPositions.ContainsKey(kvp.Key) && kvp.Value != null)
            {
                kvp.Value.position = Vector3.Lerp(kvp.Value.position, worldPositions[kvp.Key], 0.5f);
            }
        }

        // 计算旋转
        ApplyBoneRotations(worldPositions);
    }

    private Vector3 ConvertLandmarkToWorldPosition(Landmark landmark)
    {
        float worldX = (0.5f - landmark.x); // **左右翻转 x**
        float worldY = landmark.y; // **适配高度**
        float worldZ = 80f; // **固定 Z 轴**

        return new Vector3(worldX, worldY, worldZ);
    }

    private void ApplyBoneRotations(Dictionary<int, Vector3> positions)
    {
        if (positions.Count < 33) return;

        // **肩膀旋转**
        if (positions.ContainsKey(11) && positions.ContainsKey(12))
        {
            Quaternion shoulderRotation = CalculateRotation(positions[11], positions[12]);
            leftShoulder.rotation = Quaternion.Lerp(leftShoulder.rotation, shoulderRotation, 0.5f);
            rightShoulder.rotation = Quaternion.Lerp(rightShoulder.rotation, shoulderRotation, 0.5f);
        }

        // **手臂旋转**
        ApplyJointRotation(11, 13, leftElbow, positions);
        ApplyJointRotation(12, 14, rightElbow, positions);
        ApplyJointRotation(13, 15, leftWrist, positions);
        ApplyJointRotation(14, 16, rightWrist, positions);

        // **腿部旋转**
        ApplyJointRotation(23, 25, leftKnee, positions);
        ApplyJointRotation(24, 26, rightKnee, positions);
        ApplyJointRotation(25, 27, leftAnkle, positions);
        ApplyJointRotation(26, 28, rightAnkle, positions);
    }

    private void ApplyJointRotation(int startIdx, int endIdx, Transform bone, Dictionary<int, Vector3> positions)
    {
        if (positions.ContainsKey(startIdx) && positions.ContainsKey(endIdx) && bone != null)
        {
            Quaternion rotation = CalculateRotation(positions[startIdx], positions[endIdx]);
            bone.rotation = Quaternion.Lerp(bone.rotation, rotation, 0.5f);
        }
    }

    private Quaternion CalculateRotation(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        return Quaternion.LookRotation(direction);
    }
}
