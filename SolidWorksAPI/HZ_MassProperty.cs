using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
   public class HZ_MassProperty
    {

        // 质量属性
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 材料
        /// </summary>
        public string material { get; set; }
        /// <summary>
        /// 质量
        /// </summary>
        public string mass { get; set; }
        ///// <summary>
        ///// 密度
        ///// </summary>
        public string density { get; set; }
        /// <summary>
        /// 表面积
        /// </summary>
        public string surface { get; set; }
        /// <summary>
        /// 体积
        /// </summary>
        public string volume { get; set; }
        /// <summary>
        /// 长
        /// </summary>
        public string x { get; set; }

        /// <summary>
        /// 宽
        /// </summary>
        public string y { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        public string z { get; set; }

        // 外形尺寸
        /// <summary>
        /// 尺寸数据 X * Y * Z
        /// </summary>
        public string box { get; set; }

        /// <summary>
        /// 直径
        /// </summary>
        public string diameter { get; set; }

        /// <summary>
        /// Java端原始json
        /// </summary>
        public string originalJson { get; set; }
    }
}
