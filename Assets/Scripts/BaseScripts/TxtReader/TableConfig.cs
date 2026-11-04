using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public static class TableConfig
    {
        /// <summary>
        /// 所有配表必须在这里注册才可以加载
        /// </summary>
        public static TDTableMetaData[] loadTableArray =
        {
            TDLoadingTipTable.metaData,
            TDEffectResourceTable.metaData,
            TDTrophyExpTable.metaData,
            TDCharacterProficiencyTable.metaData,
            TDPlayerLevelExpTable.metaData,
            TDOnlineDefaultPosTable.metaData,
            TDAudioResourceTable.metaData,
            TDSkillIconTable.metaData,
            TDTargetShootingModeTable.metaData,
            TDTargetShootingTargetTable.metaData,
        };
    }
}
