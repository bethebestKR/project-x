using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    public Transform cameraTransform; // 카메라 Transform
    public CharacterController characterController; // 캐릭터 컨트롤러

    public float maxSpeed = 20f; // 평지에서의 최대 속도
    public float acceleration = 40f; // 가속도
    public float deceleration = 40f; // 감속도
    public float jumpSpeed = 10f; // 점프 속도
    public float gravity = -20f; // 중력
    public float slopeSlideGravity = -40f; // 경사면에서 추가 중력
    public float slopeSpeedMultiplier = 2f; // 경사면에서 속도 증가 배율

    private Vector3 currentVelocity = Vector3.zero; // 현재 속도
    private float yVelocity = 0; // Y축 속도

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // 좌우 입력
        float v = Input.GetAxis("Vertical");   // 상하 입력

        Vector3 inputDirection = new Vector3(h, 0, v).normalized;

        if (inputDirection.magnitude > 0.1f)
        {
            // 입력이 있으면 가속
            Vector3 targetVelocity = inputDirection * maxSpeed;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // 입력이 없으면 감속
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // 경사면 계산
        bool isOnSlope = IsOnSlope(out Vector3 slopeNormal);
        if (isOnSlope)
        {
            // 경사면에서 추가 중력 및 속도 증가
            Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(slopeNormal, Vector3.down), slopeNormal);
            currentVelocity += slopeDirection * slopeSpeedMultiplier * Time.deltaTime;
            yVelocity += slopeSlideGravity * Time.deltaTime;
        }

        // 카메라 기준 방향으로 변환
        Vector3 moveDirection = cameraTransform.TransformDirection(currentVelocity);

        if (characterController.isGrounded)
        {
            yVelocity = 0; // 땅에 있을 때 Y축 속도 초기화

            if (Input.GetKeyDown(KeyCode.Space))
            {
                yVelocity = jumpSpeed; // 점프
            }
        }

        yVelocity += gravity * Time.deltaTime; // 중력 적용
        moveDirection.y = yVelocity; // Y축 속도 추가

        characterController.Move(moveDirection * Time.deltaTime); // 캐릭터 이동
    }

    private bool IsOnSlope(out Vector3 slopeNormal)
    {
        // 경사면인지 확인하고 경사면의 법선을 반환
        if (characterController.isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 + 0.1f))
            {
                slopeNormal = hit.normal;
                return Vector3.Angle(Vector3.up, slopeNormal) > characterController.slopeLimit;
            }
        }

        slopeNormal = Vector3.up;
        return false;
    }
}
