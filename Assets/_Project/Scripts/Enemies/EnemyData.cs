using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "The Immortal Merchant/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyId;
    public string displayName;
    public int maxHealth = 30;
    public float moveSpeed = 2f;
    public int contactDamage = 10;
    public float damageInterval = 1f;
    public float attackRange = 1.25f;
    public float attackWindup = 0.45f;
    public float attackRecovery = 0.65f;
    public Vector3 scale = Vector3.one;
    public Color prototypeColor = Color.red;
    public DropTableData dropTable;
}
