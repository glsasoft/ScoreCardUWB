using System;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// Nooploop pos[x,y,z] → 场馆米制（Y 朝上，水平 XZ）。
    /// 默认把官方水平 XY、高度 Z 映射为场馆 (x, z→y, y→z)。
    /// </summary>
    [Serializable]
    public class UwbCoordinateMapper
    {
        public enum Axis
        {
            PosX = 0,
            PosY = 1,
            PosZ = 2,
            NegX = 3,
            NegY = 4,
            NegZ = 5
        }

        [Tooltip("场馆 X 取自 UWB 哪一轴")]
        public Axis venueXFrom = Axis.PosX;

        [Tooltip("场馆 Y（高度）取自 UWB 哪一轴")]
        public Axis venueYFrom = Axis.PosZ;

        [Tooltip("场馆 Z 取自 UWB 哪一轴")]
        public Axis venueZFrom = Axis.PosY;

        [Tooltip("映射后再加的偏移（米）")]
        public Vector3 offsetMeters;

        [Tooltip("映射后再乘的缩放")]
        public Vector3 scale = Vector3.one;

        [Tooltip("是否用地板高度强制 y=0（贴地交给 VenuePlayerSync）")]
        public bool flattenHeight = true;

        public Vector3 ToVenueMeters(float rawX, float rawY, float rawZ)
        {
            float x = Pick(venueXFrom, rawX, rawY, rawZ) * scale.x + offsetMeters.x;
            float y = Pick(venueYFrom, rawX, rawY, rawZ) * scale.y + offsetMeters.y;
            float z = Pick(venueZFrom, rawX, rawY, rawZ) * scale.z + offsetMeters.z;
            if (flattenHeight)
                y = 0f;
            return new Vector3(x, y, z);
        }

        public float EstimateYawDegrees(float velX, float velY, float velZ)
        {
            Vector3 venueVel = new Vector3(
                Pick(venueXFrom, velX, velY, velZ) * scale.x,
                0f,
                Pick(venueZFrom, velX, velY, velZ) * scale.z);
            if (venueVel.sqrMagnitude < 0.01f)
                return float.NaN;
            return Mathf.Atan2(venueVel.x, venueVel.z) * Mathf.Rad2Deg;
        }

        static float Pick(Axis axis, float x, float y, float z)
        {
            switch (axis)
            {
                case Axis.PosX: return x;
                case Axis.PosY: return y;
                case Axis.PosZ: return z;
                case Axis.NegX: return -x;
                case Axis.NegY: return -y;
                case Axis.NegZ: return -z;
                default: return 0f;
            }
        }
    }
}
