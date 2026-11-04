using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define;
using LetsPiu.Base.Localization;
using UnityEngine;
using YooAsset;

namespace LetsPiu.Base
{
    public class TableManager
    {
        public static TableManager S = new TableManager();

        public float process => m_LoadProcess;

        float m_LoadProcess = 0;

        ResourcePackage m_DefaultPackage;

        public async UniTask LoadTable()
        {
            m_DefaultPackage = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            // 集体预加载
            for (int i = 0; i < TableConfig.loadTableArray.Length; ++i)
            {
                var data = TableConfig.loadTableArray[i];
                data.onPreParse();
                if (i % 6 == 0)
                {
                    await UniTask.NextFrame();
                }
            }

            for (int i = 0; i < TableConfig.loadTableArray.Length; ++i)
            {
                var data = TableConfig.loadTableArray[i];
                var txtPath = $"Assets/TableConfigRes/{data.tableName}";
                if (!m_DefaultPackage.CheckLocationValid(txtPath))
                {
                    Debug.Log($"无效配置表{data.tableName}");
                    continue;
                }
                var textAsset = m_DefaultPackage.LoadAssetAsync<TextAsset>(txtPath);
                await textAsset.ToUniTask();
                var bytesData = (textAsset.AssetObject as TextAsset).bytes;
                textAsset.Dispose();
                // 加载数据
                data.onParse(bytesData, "");
                if (i % 3 == 0)
                {
                    await UniTask.NextFrame();
                }
            }

            for (int i = 0; i < TableConfig.loadTableArray.Length; ++i)
            {
                var data = TableConfig.loadTableArray[i];
                data.onPostParse();
                if (i % 6 == 0)
                {
                    await UniTask.NextFrame();
                }
            }

            // 多国语言 CSV 加载（替代老的 LanguageCHN/USA/GAT.txt 三表分支）。
            await LocalizationManager.S.LoadAsync();
        }
    }
}
