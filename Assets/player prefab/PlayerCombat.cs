using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Stats")]
    public int health = 100;
    public bool isKnockedDown = false;

    [Header("Spells (MVP)")]
    public GameObject mainSpellPrefab; // 拖入你的火球预制体
    public Transform firePoint;        // 在玩家子物体建一个空物体作为发射点

    private PlayerController controller;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    // Input System 自动调用的主法术方法 (对应 CastMain Action)
    void OnCastMain(InputValue value)
    {
        if (value.isPressed && !isKnockedDown && mainSpellPrefab != null)
        {
            CastSpell(mainSpellPrefab);
        }
    }

    void CastSpell(GameObject spellPrefab)
    {
        // 实例化法术，方向为当前玩家的面朝向
        Instantiate(spellPrefab, firePoint.position, firePoint.rotation);
        // TODO: 这里可以加一个简单的 float cooldownTimer 限制连续发射
    }

    // 其他物体（如法术）打中你时调用这个公开方法
    public void TakeDamage(int damage)
    {
        if (isKnockedDown) return; // 倒地时无敌

        health -= damage;
        Debug.Log(gameObject.name + " 受到伤害，剩余血量: " + health);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(KnockdownRoutine());
        }
    }

    // 极简版“击倒”逻辑：禁用移动1.5秒，完全不需要做动画
    IEnumerator KnockdownRoutine()
    {
        isKnockedDown = true;
        controller.canMove = false;
        
        // 可选：把Sprite变红或变灰，示意正在受击状态
        GetComponent<SpriteRenderer>().color = Color.gray; 

        yield return new WaitForSeconds(1.5f); // 硬直时间

        GetComponent<SpriteRenderer>().color = Color.white;
        controller.canMove = true;
        isKnockedDown = false;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " 被淘汰了！");
        // Tech Demo 先直接销毁或隐藏，后续交给 GameManager 判定胜负
        gameObject.SetActive(false); 
    }
}