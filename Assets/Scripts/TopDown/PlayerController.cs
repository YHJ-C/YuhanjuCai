using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public ParticleSystem clickEffect;

    private Mouse mouse;
    private TDCharater tDCharater;

    void Start()
    {
        mouse = Mouse.current;
        tDCharater = GetComponent<TDCharater>();
        tDCharater.OnDeath += Death; // 订阅死亡事件
    }


    void Update()
    {
        if(BattleService.Instance.State == BattleService.BattleState.Battle)
        {           
            HandleInput();
        }

    }

    private void HandleInput()
    {
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // 是否点击在ui上
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // 如果点击在UI上，直接返回不处理
            }

            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                {
                    PlayClickEffect(navHit.position);
                    tDCharater.MoveTo(navHit.position);
                }
            }

        }
    }

    public void Death()
    {
        BattleService.Instance.PlayerDeath();
    }

    public TDCharater GetPlayer()
    {
        return tDCharater;
    }

    public void PlayClickEffect(Vector3 position)
    {
        clickEffect.transform.position = position;
        clickEffect.Clear(true);
        clickEffect.Play();
    }
}
