using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : RecycleObject
{
    /// <summary>
    /// 탄피가 튀어나가는 최소 힘
    /// </summary>
    public float minForce = 10.0f;

    /// <summary>
    /// 탄피가 튀어나가는 최대 힘
    /// </summary>
    public float maxForce = 20.0f;

    /// <summary>
    /// 탄피가 사라질때까지의 시간
    /// </summary>
    public float lifeTime = 4.0f;

    /// <summary>
    /// 탄피 리지드바디
    /// </summary>
    Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    protected override void OnReset()
    {
        float force = Random.Range(minForce, maxForce);     // 랜덤으로 힘 부여

        float x = GetRandomSignedTwo();
        float y = Random.Range(0, 3);

        Vector3 shellForcePosition = new Vector3(x, y, 0);

        rigid.AddForce(transform.position + (shellForcePosition.normalized * force));            // 튀어나가는 힘 주기
        rigid.AddTorque(Random.insideUnitSphere * force * 0.5f);   // 돌아가는 힘 주기

        DisableTimer(4.0f);                                 // 탄피 수명 주기
    }

    /// <summary>
    /// 랜덤으로 2 나 -2를 반환하는 함수
    /// </summary>
    /// <returns></returns>
    int GetRandomSignedTwo()
    {
        int n = Random.Range(1, 11);
        if(n % 2 == 0)
        {
            return 2;
        }
        else
        {
            return -2;
        }
    }
}
