using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    /// <summary>
    /// 총알에 맞았을 때 신호를 주는 함수
    /// </summary>
    public Action<RaycastHit> onHit;

    /// <summary>
    /// 각종 사운드들
    /// </summary>
    public AudioClip[] audioClips;

    /// <summary>
    /// 플레이어 카메라
    /// </summary>
    public CinemachineVirtualCamera[] cinemachineVirtualCameras;

    /// <summary>
    /// 1인칭 시점 트랜스폼
    /// </summary>
    public Transform firstPerson;

    /// <summary>
    /// 3인칭 시점 총 트랜스폼
    /// </summary>
    public Transform thirdPerson;

    /// <summary>
    /// 탄피 생성 위치
    /// </summary>
    public Transform shellTransform;

    /// <summary>
    /// 총알 생성 위치
    /// </summary>
    public Transform bulletTransform;

    /// <summary>
    /// 3인칭 시점 플레이어 트랜스폼
    /// </summary>
    public GameObject player;

    /// <summary>
    /// 플레이어 최대 이동 속도
    /// </summary>
    public float moveSpeedSetting = 8.0f;

    /// <summary>
    /// 총 발사 인터벌
    /// </summary>
    public float shootInterval = 0.5f;

    /// <summary>
    /// 이동속도 변화율
    /// </summary>
    public float speedChangeRate = 5.0f;

    /// <summary>
    /// 점프 쿨타임
    /// </summary>
    public float jumpInterval = 1f;

    /// <summary>
    /// 마우스 감도
    /// </summary>
    public float mouseSensitivity = 0.1f;

    /// <summary>
    /// 회전 변화율
    /// </summary>
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;

    /// <summary>
    /// y축 최대 회전
    /// </summary>
    public float topClamp = 70.0f;

    /// <summary>
    /// y축 최소 회전
    /// </summary>
    public float bottomClamp = -30.0f;

    /// <summary>
    /// 총기 반동 진동 강도
    /// </summary>
    public float amplitudeGain = 1.0f;

    /// <summary>
    /// 총디 반동 진동 주기
    /// </summary>
    public float frequencyGain = 0.5f;

    /// <summary>
    /// 총알 개수
    /// </summary>
    private int bulletNumber = 7;

    /// <summary>
    /// 총알 개수에 따라서 장전을 하게 해주는 프로퍼티
    /// </summary>
    public int BulletNumber
    {
        get => bulletNumber;
        set
        {
            bulletNumber = value;
            Debug.Log($"총알 개수 : {bulletNumber}");
            if(bulletNumber < 7)
            {
                isReload = true;
                if(bulletNumber < 1)
                {
                    Reload();
                }
            }
        }
    }

    /// <summary>
    /// 현재 속도
    /// </summary>
    private float speed = 0.0f;

    /// <summary>
    /// 평소 속도
    /// </summary>
    private float moveSpeed;

    /// <summary>
    /// 총 발사 쿨타임
    /// </summary>
    private float shootCoolTime = 0.0f;

    /// <summary>
    /// 점프 쿨타임
    /// </summary>
    private float jumpCoolTime = 0.0f;

    /// <summary>
    /// 장전 시간
    /// </summary>
    private float reloadTime = 3.7f;

    /// <summary>
    /// x축 회전 값
    /// </summary>
    private float xRotation = 0f;

    /// <summary>
    /// y축 회전 값
    /// </summary>
    private float yRotation = 0f;

    /// <summary>
    /// 플레이어 회전률
    /// </summary>
    private float targetRotation = 0.0f;

    /// <summary>
    /// 회전 벨로시티
    /// </summary>
    private float rotationVelocity;

    /// <summary>
    /// 총알 발사 거리
    /// </summary>
    private float rayLength = 100.0f;

    /// <summary>
    /// 속도 증가율
    /// </summary>
    private float speedIncrease = 1.5f;

    /// <summary>
    /// 속도 감소율
    /// </summary>
    private float speedDecrease = 2.0f / 3.0f;

    /// <summary>
    /// 움직이면 true, 안움직이면 false
    /// </summary>
    private bool IsMove => IsJumpReady && moveInput != Vector3.zero;   // 바닥에 닿았을 때 누르고 있는 버튼 그대로 다시 움직이기

    /// <summary>
    /// 줌을 했는지 아닌지 알려주는 bool타입 변수
    /// </summary>
    private bool isZoom = false;

    /// <summary>
    /// 앉아있는지 아닌지 판단하는 변수(true면 앉아있고, false면 서있다.)
    /// </summary>
    private bool isSit = false;

    /// <summary>
    /// 장전 할 수 있음
    /// </summary>
    private bool isReload = false;

    /// <summary>
    /// 바닥위에 있는지 판단하는 변수
    /// </summary>
    private bool isGrounded = true;

    /// <summary>
    /// 앉거나 설때 이동속도를 돌려주는 프로퍼티
    /// </summary>
    public bool IsSit
    {
        get => isSit;
        set
        {
            if (isSit != value)
            {
                isSit = value;
                animator.SetBool(Sit_Hash, isSit);
                if (isSit)
                {
                    moveSpeed = 0;
                }
                else
                {
                    StartCoroutine(StandWaitTime());
                }
            }
        }
    }

    /// <summary>
    /// 총알 발사 준비됐는지 확인하는 프로퍼티
    /// </summary>
    private bool IsShootReady => (shootCoolTime < 0 && BulletNumber > 0);

    /// <summary>
    /// 점프할 준비가 됐는지 알려주는 프로퍼티
    /// </summary>
    private bool IsJumpReady => (jumpCoolTime < 0 && isGrounded);

    /// <summary>
    /// 플레이어 이동 방향
    /// </summary>
    Vector3 moveInput = Vector3.zero;

    // 컴포넌트들
    GroundSensor groundSensor;
    Rigidbody rigid;
    Rigidbody FPrigid;
    PlayerInputActions playerInput;
    Animator animator;
    CinemachineBasicMultiChannelPerlin noise;
    Animator armAnimator;
    Transform root;
    Transform zoom;
    Transform waist;

    // 애니메이터 해쉬값
    readonly int Move_Hash = Animator.StringToHash("Move");
    readonly int Shoot_Hash = Animator.StringToHash("Shoot");
    readonly int Reload_Hash = Animator.StringToHash("Reload");
    readonly int Sit_Hash = Animator.StringToHash("Sit");
    readonly int Jump_Hash = Animator.StringToHash("Jump");

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();                          // 3인칭 리지드바디
        FPrigid = firstPerson.GetComponent<Rigidbody>();            // 1인칭 리지드바디
        playerInput = new PlayerInputActions();                     // 인풋 액션
        root = transform.GetChild(0);                               // 3인칭 트랜스폼
        zoom = transform.GetChild(1);                               // 1인칭 트랜스폼
        animator = GetComponent<Animator>();                        // 3인칭 애니메이터
        armAnimator = zoom.GetChild(0).GetComponent<Animator>();    // 1인칭 애니메이터
        noise = cinemachineVirtualCameras[1].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>(); // 총 쏠때 화면 흔들림을 위한 컴포넌트
        groundSensor = GetComponentInChildren<GroundSensor>();      // 바닥 센서 컴포넌트
    }

    private void Start()
    {
        firstPerson.gameObject.SetActive(isZoom);       // 1인칭 팔 안보이게 하기
        moveSpeed = moveSpeedSetting;                   // 설정한 속도를 moveSpeed에 저장하기
        noise.m_AmplitudeGain = 0f;                     // 화면 Noise
        noise.m_FrequencyGain = 0f;                     // 없애기
        shootCoolTime = shootInterval;                  // 총 쏘는 쿨타임 주기 => Draaw떄문에 바로 쏘면 안됨

        groundSensor.onGround += (isGround) =>
        {
            isGrounded = isGround;  // 바닥에 닿으면 true, 아니면 false
        };
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Move.performed += OnWalkInput;
        playerInput.Player.Move.canceled += OffWalkInput;
        playerInput.Player.Look.performed += OnLookInput;
        playerInput.Player.Walk.performed += OnSlow;
        playerInput.Player.Walk.canceled += OffSlow;
        playerInput.Player.Jump.performed += OnJump;
        playerInput.Player.Shoot.performed += OnShot;
        playerInput.Player.Zoom.performed += OnZoom;
        playerInput.Player.Reload.performed += Reloading;
        playerInput.Player.Sit.performed += OnSit;
        playerInput.Player.Run.performed += OnRun;
        playerInput.Player.Run.canceled += OffRun;
    }

    private void OnDisable()
    {
        playerInput.Player.Run.canceled -= OffRun;
        playerInput.Player.Run.performed -= OnRun;
        playerInput.Player.Sit.performed -= OnSit;
        playerInput.Player.Reload.performed -= Reloading;
        playerInput.Player.Zoom.performed -= OnZoom;
        playerInput.Player.Shoot.performed -= OnShot;
        playerInput.Player.Jump.performed -= OnJump;
        playerInput.Player.Walk.canceled -= OffSlow;
        playerInput.Player.Walk.performed -= OnSlow;
        playerInput.Player.Look.performed -= OnLookInput;
        playerInput.Player.Move.canceled -= OffWalkInput;
        playerInput.Player.Move.performed -= OnWalkInput;
        playerInput.Player.Disable();
    }

    /// <summary>
    /// WASD 누를떄
    /// </summary>
    /// <param name="obj"></param>
    private void OnWalkInput(InputAction.CallbackContext obj)
    {

        IsSit = false;  // 앉아있다면 일어나기
        Vector2 input = obj.ReadValue<Vector2>();   // WASD에 따른 값 저장하기
        moveInput = new Vector3(input.x, 0, input.y);   // 저장한값 Vector3로 변환해서 변수에 저장하기
    }

    /// <summary>
    /// WASD 뗄떄
    /// </summary>
    /// <param name="_"></param>
    private void OffWalkInput(InputAction.CallbackContext _)
    {
        moveInput = Vector3.zero;   // 변수 값 없애기
    }

    /// <summary>
    /// 마우스 움직임 입력값
    /// </summary>
    /// <param name="obj"></param>
    private void OnLookInput(InputAction.CallbackContext obj)
    {
        Vector2 input = obj.ReadValue<Vector2>();   // 마우스 위치 읽기

        yRotation += input.x * mouseSensitivity;    // 마우스 감도에 따라 yRotation에 더해주기
        xRotation -= input.y * mouseSensitivity;    // 마우스 감도에 따라 xRotation에 빼주기(x축의 회전과 좌표에서의 x는 반대이기 때문에 빼준다)

        yRotation = ClampAngle(yRotation, float.MinValue, float.MaxValue);  // yRotation -360 ~ 360으로 변환
        xRotation = ClampAngle(xRotation, bottomClamp, topClamp);           // xRotation -360 ~ 360으로 변환
    }

    /// <summary>
    /// C키 누를 때
    /// </summary>
    /// <param name="_"></param>
    private void OnSlow(InputAction.CallbackContext _)
    {
        moveSpeed = speedDecrease * moveSpeedSetting;   // 느리게 움직이기
    }

    /// <summary>
    /// C키 뗄 때
    /// </summary>
    /// <param name="_"></param>
    private void OffSlow(InputAction.CallbackContext _)
    {
        moveSpeed = moveSpeedSetting;   // 원래 속도로 되돌리기
    }

    /// <summary>
    /// 점프키 입력 함수
    /// </summary>
    /// <param name="_"></param>
    private void OnJump(InputAction.CallbackContext _)
    {
        IsSit = false;  // 앉아있다면 일어나기
        if (IsJumpReady && !isZoom)    // 점프할 수 있는 상태가 되면
        {
            jumpCoolTime = jumpInterval;    // 점프 쿨타임 주기
            animator.SetTrigger(Jump_Hash); // 점프 애니메이션 주기
            rigid.velocity = Vector3.zero;
            rigid.AddForce(5 * transform.up + speed * transform.forward, ForceMode.Impulse);
        }
        
    }

    /// <summary>
    /// 마우스 좌클릭
    /// </summary>
    /// <param name="_"></param>
    private void OnShot(InputAction.CallbackContext _)
    {
        if (isZoom && IsShootReady) // 줌 상태이고 총알 발사 준비가 됐으면
        {
            shootCoolTime = shootInterval;  // 발사 쿨타임 주기
            armAnimator.SetTrigger(Shoot_Hash); // 애니메이션 주기
            BulletNumber--;     // 총알 하나 줄기
            AudioSource.PlayClipAtPoint(audioClips[0], bulletTransform.position);   // 총알 발사 소리내기
            StartCoroutine(Recoil());   // 화면 흔들림 코루틴 실행
            Factory.Instance.GetShell(shellTransform.position, FPrigid.rotation.eulerAngles);   // 탄피 생성하기

            Ray ray = new Ray(bulletTransform.position, bulletTransform.forward);   // 총구에서 Ray발사하기
            if (Physics.Raycast(ray, out RaycastHit hitInfo, rayLength))    // Ray에 맞았다면
            {
                onHit?.Invoke(hitInfo); // onHit 델리게이트 실행
            }
        }
    }

    /// <summary>
    /// 마우스 우클릭
    /// </summary>
    /// <param name="_"></param>
    private void OnZoom(InputAction.CallbackContext _)
    {
        isZoom = !isZoom;   // 1인칭 3인칭 On/Off
        firstPerson.gameObject.SetActive(isZoom);   
        thirdPerson.gameObject.SetActive(!isZoom);  
        player.SetActive(!isZoom);
        rigid.rotation = Quaternion.Euler(0, yRotation, 0);     // 1인칭에서 3인칭으로 돌아갔을 때 몸 회전을 자연스럽게 해주기
        CameraChange();                             
    }

    /// <summary>
    /// R키
    /// </summary>
    /// <param name="_"></param>
    private void Reloading(InputAction.CallbackContext _)
    {
        if (isZoom && isReload) // 줌 상태이고, 장전할 수 있는 상태이면
        {
            Reload();   // 장전하기
        }
    }

    /// <summary>
    /// 왼쪽 CTRL
    /// </summary>
    /// <param name="_"></param>
    private void OnSit(InputAction.CallbackContext _)
    {
        IsSit = !IsSit; // 앉아있다면 일어나기, 일어나있다면 앉기
    }

    /// <summary>
    /// 왼쪽 Shift 누를 때
    /// </summary>
    /// <param name="_"></param>
    private void OnRun(InputAction.CallbackContext _)
    {
        moveSpeed = speedIncrease * moveSpeedSetting;   // 빠르게 움직이기
    }

    /// <summary>
    /// 왼쪽 shift 뗄 때
    /// </summary>
    /// <param name="_"></param>
    private void OffRun(InputAction.CallbackContext _)
    {
        moveSpeed = moveSpeedSetting;   // 원래속도로 되돌리기
    }

    private void Update()
    {
        shootCoolTime -= Time.deltaTime;        // 총 발사 쿨타임 줄이기
        jumpCoolTime -= Time.deltaTime;         // 점프 쿨타임 줄이기
    }

    private void FixedUpdate()
    {
        if (isZoom)
        {
            ZoomMove(Time.fixedDeltaTime);
        }
        else
        {
            Movement(Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 플레이어 이동 함수
    /// </summary>
    /// <param name="deltaTime">시간</param>
    void Movement(float deltaTime)
    {
        if (IsMove)
        {
            // 이동속도 서서히 증가
            speed = Mathf.Lerp(speed, moveSpeed, deltaTime * speedChangeRate);

            // 플레이어 WASD에 따른 부드러운 회전 부여
            targetRotation = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + root.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, rotation, 0);

            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation)) < 1.0f) // 회전을 완료했다면
            {
                rigid.velocity = speed * transform.forward; // 플레이어의 forward 방향으로 이동
            }
            else                                                                             // 회전을 하고있다면
            {
                // 입력된 방향으로 이동
                Vector3 currentMoveDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;
                rigid.velocity = speed * currentMoveDirection;
            }

            // 카메라는 마우스에 고정
            root.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
        else
        {
            // 이동속도 서서히 감소(증가할 때의 2배 속도)
            speed = Mathf.Lerp(speed, 0.0f, deltaTime * speedChangeRate);

            // 카메라는 마우스에 고정
            root.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }

        animator.SetFloat(Move_Hash, speed); // 이동 애니메이션 해쉬값 변경
    }

    /// <summary>
    /// 줌 상황에서의 움직임
    /// </summary>
    /// <param name="deltaTime"></param>
    void ZoomMove(float deltaTime)
    {
        if (IsMove)
        {
            // 이동 속도 서서히 증가
            speed = Mathf.Lerp(speed, moveSpeed, deltaTime * speedChangeRate);
            rigid.rotation = Quaternion.Euler(0, yRotation, 0);     // 1인칭에서 3인칭으로 돌아갔을 때 몸 회전을 자연스럽게 해주기
            FPrigid.rotation = Quaternion.Euler(xRotation, yRotation, 0);   // 플레이어 팔 회전(x축 y축 같이)

            //플레이어 이동
            targetRotation = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + zoom.eulerAngles.y;  
            Vector3 currentMoveDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;
            rigid.velocity = speed * currentMoveDirection;
        }
        else
        {
            // 이동 속도 서서히 감소
            speed = Mathf.Lerp(speed, 0.0f, deltaTime * speedChangeRate);
            rigid.rotation = Quaternion.Euler(0, yRotation, 0);     // 1인칭에서 3인칭으로 돌아갔을 때 몸 회전을 자연스럽게 해주기
            FPrigid.rotation = Quaternion.Euler(xRotation, yRotation, 0);   // 플레이어 팔 회전(x축 y축 같이)
        }
    }

    /// <summary>
    /// 카메라 우선도 변경
    /// </summary>
    void CameraChange()
    {
        // 카메라 우선도를 서로 바꿔서 송출 카메라 바꾸기
        (cinemachineVirtualCameras[0].Priority, cinemachineVirtualCameras[1].Priority) = 
            (cinemachineVirtualCameras[1].Priority, cinemachineVirtualCameras[0].Priority);
    }

    /// <summary>
    /// 장전하는 함수
    /// </summary>
    void Reload()
    {
        armAnimator.SetTrigger(Reload_Hash);
        shootCoolTime = reloadTime;
        isReload = false;
        BulletNumber = 7;
    }

    /// <summary>
    /// 각도를 -360 ~ 360사이로 제한해주는 함수
    /// </summary>
    /// <param name="lfAngle">현재 각도</param>
    /// <param name="lfMin">최소 각도</param>
    /// <param name="lfMax">최대 각도</param>
    /// <returns></returns>
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;      
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    /// <summary>
    /// 총기 반동 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator Recoil()
    {
        noise.m_AmplitudeGain = amplitudeGain;  // 진동 강도 설정
        noise.m_FrequencyGain = frequencyGain;  // 진동 주기 설정

        yield return new WaitForSeconds(shootInterval);

        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
    }

    /// <summary>
    /// 일어났을 때 이동속도 돌려주는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator StandWaitTime()
    {
        yield return new WaitForSeconds(1.0f);
        moveSpeed = moveSpeedSetting;
    }
}
