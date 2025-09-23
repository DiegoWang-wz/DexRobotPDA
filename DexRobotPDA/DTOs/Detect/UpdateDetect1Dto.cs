using System.ComponentModel.DataAnnotations;

namespace DexRobotPDA.DTOs
{
    /// <summary>
    /// 更新检测记录的DTO
    /// </summary>
    public class UpdateDetect1Dto
    {
        /// <summary>
        /// 电机编号，用于匹配要更新的检测记录所属电机
        /// </summary>
        [Required(ErrorMessage = "电机编号不能为空")]
        public string MotorId { get; set; }

        /// <summary>
        /// 检测前距离
        /// </summary>
        public double? DistanceBefore { get; set; }

        /// <summary>
        /// 测试力大小
        /// </summary>
        public double? Force { get; set; }

        /// <summary>
        /// 检测后距离
        /// </summary>
        public double? DistanceAfter { get; set; }

        /// <summary>
        /// 距离判定结果（检测前 - 检测后）
        /// </summary>
        public double? DistanceResult { get; set; }

        /// <summary>
        /// 投入使用时间
        /// </summary>
        public DateTime? UsingTime { get; set; }

        /// <summary>
        /// 检验员ID
        /// </summary>
        public string InspectorId { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 是否合格
        /// </summary>
        public bool? IfQualified { get; set; }
    }
}