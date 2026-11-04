using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 字符串 / 资源路径处理的扩展方法
/// </summary>
public static class StringExpand 
{
	/// <summary>
	/// 返回 Resources.Load 能用的路径（去掉 Resources 及其前面的目录，再去掉后缀名）
	/// </summary>
	/// <param name="_path">普通资源路径，例如 Assets/Resources/Effect/Fx_001.prefab</param>
	/// <returns>相对 Resources 的加载路径，例如 Effect/Fx_001</returns>
	public static string GetGameObjectResourcePath(this string _path)
	{
		string[] p = _path.Split('/');
		string path = "";
		if (p[p.Length - 2].Equals("Resources"))//不需要Resources
		{
			path = p[p.Length - 1];
		}
		else
		{
			path = p[p.Length - 2] + "/" + p[p.Length - 1];
		}
		///去掉后缀
		int indexD = path.LastIndexOf('.');
		path = path.Substring(0, indexD);
		return path;
	}
    #region 写编辑器常用
    /// <summary>
    /// 返回一条长横线分隔符
    /// </summary>
    /// <param name="str">调用者字符串，不参与拼接（仅为扩展方法的写法）</param>
    /// <returns>固定长度的长横线</returns>
    public static string Truncation(this string str) =>
		("----------------------------------------------------------------" +
		"-----------------------------------------------------------------" +
		"------------------------------------------------------------------");
	/// <summary>
	/// 返回把传入文字夹在中间的长横线分隔符
	/// </summary>
	/// <param name="str">要夹在横线中间的文字</param>
	/// <returns>横线 + 传入文字 + 横线</returns>
	public static string TextTruncation(this string str) => 
		("-------------------------------" + str + "--------------------------------------------------------------------------------");
    #endregion


}
