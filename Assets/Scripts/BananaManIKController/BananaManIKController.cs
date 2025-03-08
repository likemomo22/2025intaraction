using UnityEngine;

public class BananaManIKController : MonoBehaviour
{
    public Animator animator;
    public Transform handTarget; // 拖拽目标
    private bool isDragging = false;
    private Vector3 offset;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // 检测鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log("点击到了：" + hit.transform.name);
                if (hit.transform == handTarget)
                {
                    isDragging = true;
                    offset = handTarget.position - hit.point;
                }
            }
        }

        // 拖拽时更新目标位置
        if (isDragging && Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                handTarget.position = hit.point + offset;
            }
        }

        // 释放鼠标
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            // 设置右手的 IK 目标
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, handTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, handTarget.rotation);
        }
    }
}