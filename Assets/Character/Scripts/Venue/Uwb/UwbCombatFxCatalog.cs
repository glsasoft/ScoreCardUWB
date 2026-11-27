using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 战斗特效目录：直接引用美术原目录 prefab（GLSA_UWB_2.5DView/Effect）。
    /// 离线包通过引用关系打进包，无需再复制一份到 Resources。
    /// </summary>
    [CreateAssetMenu(menuName = "UWB/Combat FX Catalog", fileName = "UwbCombatFxCatalog")]
    public class UwbCombatFxCatalog : ScriptableObject
    {
        [Header("Attack / Man")]
        public GameObject manAttackRed;
        public GameObject manAttackBlue;
        public GameObject attackBehit01;
        public GameObject attackBehit02;
        public GameObject attackBehit02Red;
        public GameObject attackDead;
        public GameObject attackReborn;

        [Header("Attack / Tower")]
        public GameObject townBulletRed;
        public GameObject townBulletBlue;

        [Header("Buff")]
        public GameObject buffRed;
        public GameObject buffBlue;
        public GameObject buffHeling;

        public GameObject Resolve(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            switch (relativePath)
            {
                case "Attack/Man/VFX_UWB_2.5DView_Man_Attack_Red":
                    return manAttackRed;
                case "Attack/Man/VFX_UWB_2.5DView_Man_Attack_Blue":
                    return manAttackBlue;
                case "Attack/Man/VFX_UWB_2.5DView_Attack_Behit01":
                    return attackBehit01;
                case "Attack/Man/VFX_UWB_2.5DView_Attack_Behit02":
                    return attackBehit02;
                case "Attack/Man/VFX_UWB_2.5DView_Attack_Behit02_Red":
                    return attackBehit02Red;
                case "Attack/Man/VFX_UWB_2.5DView_Attack_Dead":
                    return attackDead;
                case "Attack/Man/VFX_UWB_2.5DView_Attack_Reborn":
                    return attackReborn;
                case "Attack/Tower/VFX_UWB_2.5DView_Town_Bullet_Red":
                    return townBulletRed;
                case "Attack/Tower/VFX_UWB_2.5DView_Town_Bullet_Blue":
                    return townBulletBlue;
                case "Buff/VFX_UWB_2.5DView_BUFF_Redbuff":
                    return buffRed;
                case "Buff/VFX_UWB_2.5DView_BUFF_Bluebuff":
                    return buffBlue;
                case "Buff/VFX_UWB_2.5DView_BUFF_Heling":
                    return buffHeling;
                default:
                    return null;
            }
        }
    }
}
