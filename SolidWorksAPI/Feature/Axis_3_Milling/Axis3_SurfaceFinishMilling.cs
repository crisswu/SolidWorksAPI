using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 3轴 -- 精铣
    /// </summary>
   public class Axis3_SurfaceFinishMilling:Axis3Milling
    {
        /// <summary>
        /// 棱角半径
        /// </summary>
        public double EdgRadius { get; set; }
        /// <summary>
        /// 表面积
        /// </summary>
        public double SurfaceArea { get; set; }
        /// <summary>
        /// 裁剪宽度
        /// </summary>
        public double CuttingWidth { get; set; }
        /// <summary>
        /// 表面粗糙度
        /// </summary>
        public double SurfaceRoughness { get; set; }

        /// <summary>
        /// 裁剪长度
        /// </summary>
        public double CuttingLength { get; set; }

        public Axis3_SurfaceFinishMilling(double Dia, double EdgRadius, double SurfaceArea, double SurfaceRoughness, double NoOfPlace, Materials _Materials)
        {
            this.Dia = Dia;
            this.No = 2;
            this.CuttingSpeed = GetCuttingSpeed();
            this.FeedPer = 0.1;
            this.ReserveLength = 5;
            this._Materials = _Materials;
            this.NoOfPlaces = NoOfPlaces;
            this.EdgRadius = EdgRadius;
            this.SurfaceArea = SurfaceArea;
            this.SurfaceRoughness = SurfaceRoughness;
            Calculate_CuttingWidth();
            Calculate_CuttingLength();
            Calculate_SpindleSpeed();
            Calculate_FeedRate();
            Calculate_CuttingTime();
            Calculate_TotalTime();
           
        }
        /// <summary>
        /// 裁剪长度
        /// </summary>
        public void Calculate_CuttingLength()
        {
            this.CuttingLength = this.SurfaceArea / CuttingWidth;
        }
        /// <summary>
        /// 裁剪宽度
        /// </summary>
        private void Calculate_CuttingWidth()
        {
            this.CuttingWidth = 2 * Math.Sqrt(Math.Pow(this.EdgRadius, 2) - Math.Pow((this.EdgRadius - this.SurfaceRoughness / 1000), 2));
        }
        /// <summary>
        /// 裁剪时间
        /// </summary>
        protected override void Calculate_CuttingTime()
        {
            this.CuttingTime = (this.CuttingLength + this.ReserveLength) * this.NoOfPlaces * 60 / this.FeedRate;
        }
        /// <summary>
        /// 计算 进给速率
        /// </summary>
        protected override void Calculate_FeedRate()
        {
            this.FeedRate = this.No * this.FeedPer * this.SpindleSpeed;
        }
        /// <summary>
        /// 计算 主轴转速
        /// </summary>
        protected override void Calculate_SpindleSpeed()
        {
            if ((this.CuttingSpeed * 1000 / (this.Dia * 3.14) - 6500) >= 0)
            {
                this.SpindleSpeed = 6500;
            }
            else
            {
                this.SpindleSpeed = this.CuttingSpeed * 1000 / (this.Dia * 3.14);
            }
        }
        /// <summary>
        /// 加工时间合计
        /// </summary>
        protected override void Calculate_TotalTime()
        {
            this.TotalTime = this.AtcTime + this.OtherTime + this.CuttingTime;
        }

        protected override int GetCuttingSpeed()
        {
            switch (this._Materials)
            {
                case Materials.Carbon:
                    return 160;
                case Materials.Alloy:
                    return 160;
                case Materials.Stainless:
                    return 120;
                case Materials.Aluminum:
                    return 250;
                default:
                    return 250;
            }
        }
    }
}
