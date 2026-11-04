using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class SequenceFrameNumber : MonoBehaviour
    {
        public int columnCount = 5;
        public int rowCount = 2;

        private int? currentShowNum = null;

        private Material m_Material;
        private Color m_Color = Color.white;

        public Color color
        {
            get
            {
                if (m_Material)
                    return m_Material.color;
                else
                    return m_Color;
            }
            set
            {
                m_Color = value;
                if (m_Material)
                    m_Material.color = value;
            }
        }

        private void Awake()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer)
                m_Material = renderer.material;

            if (m_Material && columnCount > 0 && rowCount > 0)
            {
                m_Material.mainTextureScale = new Vector2(1.0f / columnCount, 1.0f / rowCount);
            }
        }

        public void ShowNum(int num)
        {
            num = Mathf.Abs(num) % 10;

            if (currentShowNum.HasValue && currentShowNum.Value == num)
                return;

            currentShowNum = num;

            if (m_Material && columnCount > 0 && rowCount > 0)
            {
                int x = num % columnCount;
                int y = num / columnCount;

                m_Material.mainTextureOffset = new Vector2(1.0f / columnCount * x, 1.0f - 1.0f / rowCount * (y + 1));
            }
        }
    }
}
