using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    public class HZ_FeatCost
    {
        /// <summary>
        /// 特征名称
        /// </summary>
        public string feature { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 特征类型
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 综合成本
        /// </summary>
        public double combinedCost { get; set; }
        /// <summary>
        /// 组合时间
        /// </summary>
        public string combinedTime { get; set; }
        /// <summary>
        /// 子特征链表
        /// </summary>
        public List<HZ_SubFeatCost> subFeatCostList = new List<HZ_SubFeatCost>();
    }
    /// <summary>
    /// 零件子特征类
    /// </summary>
    public class HZ_SubFeatCost
    {
        /// <summary>
        /// 父特征名称
        /// </summary>
        public string Feature { get; set; }
        /// <summary>
        /// 子特征名称
        /// </summary>
        public string SubFeature { get; set; }
        /// <summary>
        /// 描述        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 特征类型
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 综合成本
        /// </summary>
        public double combinedCost { get; set; }
        /// <summary>
        /// 组合时间
        /// </summary>
        public string combinedTime { get; set; }
    }

    /// <summary>
    /// 工具类
    /// </summary>
    public class HZ_Tools
    {
        /// <summary>
        /// 工具类对象
        /// </summary>
        private static HZ_Tools m_Tools = null;
        /// <summary>
        /// 静态引用
        /// </summary>
        /// <returns>工具类对象</returns>
        public static HZ_Tools initInstance()
        {
            if (m_Tools == null)
            {
                m_Tools = new HZ_Tools();
            }
            return m_Tools;
        }
        /// <summary>
        /// 时间成本
        /// </summary>
        /// <param name="timevalue">时间值（浮点）</param>
        /// <returns>时间值（格式化）</returns>
        public string getTimeCost(double timevalue)
        {
            string smin;
            string ssec;
            if ((timevalue < 1e-10))
            {
                smin = "0";
                ssec = "0";
            }
            else
            {
                smin = ((int)timevalue).ToString();
                var min = Convert.ToDouble((int)timevalue);
                var sec = (timevalue - min) * 60;
                ssec = ((int)sec).ToString();
            }

            var combinedtime = smin + "分" + ssec + "秒";
            return combinedtime;
        }
        /// <summary>
        /// 得到尺寸信息
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="hzuint">单位类型</param>
        /// <returns>外形</returns>
        public string getLong(double x, double y, double z, HZ_Unit hzuint)
        {
            string longstr = "";
            int factor = 1000;
            if (hzuint == HZ_Unit.hz_mm)
            {
                longstr = (Math.Round(Math.Abs(x * factor), 2)).ToString() + "×" + (Math.Round(Math.Abs(y * factor), 2)).ToString() + "×" +
                          (Math.Round(Math.Abs(z * factor), 2)).ToString();
            }
            else
            {
                longstr = (Math.Round(Math.Abs(x), 3)).ToString() + "×" + (Math.Round(Math.Abs(y), 3)).ToString() + "×" +
                          (Math.Round(Math.Abs(z), 3)).ToString();
            }
            return longstr;
        }

        public string getLong(double value, HZ_Unit hzuint)
        {
            string longstr = "";
            int factor = 1000;
            if (hzuint == HZ_Unit.hz_mm)
            {
                longstr = (Math.Round(Math.Abs(value * factor), 5)).ToString();
            }
            else
            {
                longstr = (Math.Round(Math.Abs(value), 5)).ToString();
            }
            return longstr;
        }

        /// <summary>
        /// 得到表面积信息
        /// </summary>
        /// <param name="area">面积</param>
        /// <param name="hzunit">单位类型</param>
        /// <returns>表面积</returns>
        public string getArea(string area, HZ_Unit hzunit)
        {
            //平方分米
            string areastr = "";
            int factor = 1000000;
            if (hzunit == HZ_Unit.hz_mm)
            {
                areastr = Math.Round(double.Parse(area) * factor, 3).ToString();
            }
            else
            {
                areastr = Math.Round(double.Parse(area), 3).ToString();
            }

            return areastr;
        }
        /// <summary>
        /// 得到体积信息
        /// </summary>
        /// <param name="volume">体积</param>
        /// <param name="hzunit">单位类型</param>
        /// <returns>体积</returns>
        public string getVolume(string volume, HZ_Unit hzunit)
        {
            //立方米
            string volumestr = "";
            int factor = 1000000000;
            if (hzunit == HZ_Unit.hz_mm)
            {
                volumestr = Math.Round(double.Parse(volume) * factor, 3).ToString();
            }
            else
            {
                volumestr = Math.Round(double.Parse(volume), 6).ToString();
            }
            return volumestr;
        }
        /// <summary>
        /// 保留两位小数
        /// </summary>
        /// <param name="num">数</param>
        /// <returns>规范后的数</returns>
        public double specNum(double num)
        {
            return Math.Round(num, 3);
        }
        /// <summary>
        /// 得到质量
        /// </summary>
        /// <param name="mass">质量</param>
        /// <returns>质量</returns>
        public string getWeight(double mass)
        {
            return Math.Round(mass, 3).ToString();
        }

         
    }
}
