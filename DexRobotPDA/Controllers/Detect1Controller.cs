using DexRobotPDA.DataModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;
using Microsoft.Data.SqlClient;

namespace DexRobotPDA.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class Detect1Controller : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<Detect1Controller> _logger;

    public Detect1Controller(DailyDbContext _db, IMapper _mapper, ILogger<Detect1Controller> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMotorWormDetect(string motor_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 验证输入参数
            if (string.IsNullOrEmpty(motor_id))
            {
                response.ResultCode = -1;
                response.Msg = "电机ID不能为空";
                _logger.LogWarning("获取检测记录失败：电机ID为空");
                return BadRequest(response);
            }

            // 2. 根据motor_id查询，按id降序排序取第一条（最新记录）
            var latestDetect = await db.Detect1
                .Where(d => d.motor_id == motor_id)
                .OrderByDescending(d => d.id)
                .FirstOrDefaultAsync();

            // 3. 处理查询结果
            if (latestDetect == null)
            {
                response.ResultCode = 0;
                response.Msg = $"未找到电机ID为 '{motor_id}' 的检测记录";
                _logger.LogInformation("未找到电机 {MotorId} 的检测记录", motor_id);
            }
            else
            {
                // 映射为DTO返回
                var detectDto = mapper.Map<MotorWormDetectDto>(latestDetect);
                response.ResultCode = 1;
                response.Msg = "Success";
                response.ResultData = detectDto;
                _logger.LogInformation("成功获取电机 {MotorId} 的最新检测记录，检测ID: {DetectId}", 
                    motor_id, latestDetect.id);
            }
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取检测记录失败";
            _logger.LogError(e, "获取电机 {MotorId} 的检测记录时发生错误", motor_id);
        }

        return Ok(response);
    }


    [HttpPost]
    public async Task<IActionResult> AddDetect1(AddDetect1Dto addDetectDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 基础参数验证
            if (string.IsNullOrEmpty(addDetectDto.motor_id))
            {
                response.ResultCode = -1;
                response.Msg = "电机ID不能为空";
                _logger.LogWarning("新增检测记录失败：电机ID为空");
                return BadRequest(response);
            }

            // 2. 检查关联的电机是否存在（确保外键有效）
            bool motorExists = await db.Motors.AnyAsync(m => m.motor_id == addDetectDto.motor_id);
            if (!motorExists)
            {
                response.ResultCode = -1;
                response.Msg = $"电机ID '{addDetectDto.motor_id}' 不存在，无法创建检测记录";
                _logger.LogWarning("新增检测记录失败：电机不存在 - {MotorId}", addDetectDto.motor_id);
                return BadRequest(response);
            }

            // 3. 使用AutoMapper将DTO转换为实体
            var detectModel = mapper.Map<MotorWormDetectModel>(addDetectDto);

            // 5. 添加到数据库并保存
            await db.Detect1.AddAsync(detectModel);
            await db.SaveChangesAsync();

            // 6. 记录成功日志
            _logger.LogInformation("成功新增检测记录，检测ID: {DetectId}, 电机ID: {MotorId}",
                detectModel.id, addDetectDto.motor_id);

            // 7. 构建成功响应
            response.ResultCode = 1;
            response.Msg = "新增检测记录成功";
            response.ResultData = new
            {
                detect_id = detectModel.id,
                motor_id = detectModel.motor_id,
                combine_time = detectModel.combine_time
            };

            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            // 处理数据库相关异常（如外键约束错误）
            string errorMsg = "数据库操作失败";
            if (dbEx.InnerException is SqlException sqlEx)
            {
                // 外键约束错误（例如关联的电机不存在，虽然上面已做检查，但防止并发问题）
                if (sqlEx.Number == 547)
                {
                    errorMsg = $"关联数据不存在（电机ID: {addDetectDto.motor_id}）";
                }
                // 唯一键约束错误（如果表中有唯一索引）
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    errorMsg = "检测记录已存在，不能重复添加";
                }
            }

            response.ResultCode = -1;
            response.Msg = errorMsg;
            _logger.LogError(dbEx, "新增检测记录时数据库操作失败，电机ID: {MotorId}", addDetectDto?.motor_id);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            // 处理其他未知异常
            response.ResultCode = -1;
            response.Msg = "新增检测记录失败";
            _logger.LogError(ex, "新增检测记录时发生错误，电机ID: {MotorId}", addDetectDto?.motor_id);

            return StatusCode(500, response);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateLatestDetect(UpdateDetect1Dto updateDto)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            if (string.IsNullOrEmpty(updateDto.MotorId))
            {
                response.ResultCode = -1;
                response.Msg = "电机ID不能为空";
                _logger.LogWarning("更新检测记录失败：电机ID为空");
                return BadRequest(response);
            }

            var latestDetect = await db.Detect1
                .Where(d => d.motor_id == updateDto.MotorId)
                .OrderByDescending(d => d.id)
                .FirstOrDefaultAsync();

            if (latestDetect == null)
            {
                response.ResultCode = -1;
                response.Msg = $"电机ID '{updateDto.MotorId}' 不存在检测记录，无法更新";
                _logger.LogWarning("更新检测记录失败：未找到电机 {MotorId} 的检测记录", updateDto.MotorId);
                return BadRequest(response);
            }

            if (updateDto.DistanceBefore.HasValue)
                latestDetect.distance_before = updateDto.DistanceBefore;

            if (updateDto.Force.HasValue)
                latestDetect.force = updateDto.Force;

            if (updateDto.DistanceAfter.HasValue)
                latestDetect.distance_after = updateDto.DistanceAfter;

            if (updateDto.DistanceResult.HasValue)
                latestDetect.distance_result = updateDto.DistanceResult;
            else if (updateDto.DistanceBefore.HasValue && updateDto.DistanceAfter.HasValue)
                // 计算距离结果时添加四舍五入
                latestDetect.distance_result = Math.Round(
                    updateDto.DistanceBefore.Value - updateDto.DistanceAfter.Value,
                    2
                );

            if (updateDto.UsingTime.HasValue)
                latestDetect.using_time = updateDto.UsingTime;

            if (!string.IsNullOrEmpty(updateDto.InspectorId))
            {
                latestDetect.inspector_id = updateDto.InspectorId;
            }

            if (!string.IsNullOrEmpty(updateDto.Remarks))
                latestDetect.remarks = updateDto.Remarks;

            if (updateDto.IfQualified.HasValue)
            {
                latestDetect.if_qualified = updateDto.IfQualified;
            }
            else
            {
                bool isQualified = false;
                if (latestDetect.distance_before.HasValue && latestDetect.distance_after.HasValue)
                {
                    double distanceDiff = latestDetect.distance_before.Value - latestDetect.distance_after.Value;
                    if (distanceDiff <= 0.02)
                    {
                        // 条件2：投入使用时间距离蜗杆粘接时间大于三天（72小时）
                        if (latestDetect.combine_time.HasValue && latestDetect.using_time.HasValue)
                        {
                            TimeSpan timeDiff = latestDetect.using_time.Value - latestDetect.combine_time.Value;
                            if (timeDiff.TotalHours > 72)
                            {
                                isQualified = true;
                            }
                        }
                    }
                }

                latestDetect.if_qualified = isQualified;
            }

            // 5. 保存更新
            db.Detect1.Update(latestDetect);
            await db.SaveChangesAsync();

            // 6. 记录日志并返回结果
            _logger.LogInformation("成功更新电机 {MotorId} 的最新检测记录，检测ID: {DetectId}",
                updateDto.MotorId, latestDetect.id);

            response.ResultCode = 1;
            response.Msg = "检测记录更新成功";
            response.ResultData = new
            {
                detect_id = latestDetect.id,
                motor_id = latestDetect.motor_id,
                updated_fields = GetUpdatedFields(updateDto),
                if_qualified = latestDetect.if_qualified
            };

            return Ok(response);
        }
        catch (DbUpdateException dbEx)
        {
            string errorMsg = "数据库操作失败";
            if (dbEx.InnerException is SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "更新检测记录时发生数据库错误，错误编号: {ErrorCode}", sqlEx.Number);
            }

            response.ResultCode = -1;
            response.Msg = errorMsg;
            _logger.LogError(dbEx, "更新电机 {MotorId} 的检测记录时数据库操作失败", updateDto?.MotorId);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            response.ResultCode = -1;
            response.Msg = "更新检测记录失败";
            _logger.LogError(ex, "更新电机 {MotorId} 的检测记录时发生错误", updateDto?.MotorId);

            return StatusCode(500, response);
        }
    }

// 辅助方法：获取已更新的字段列表，用于响应信息
    private List<string> GetUpdatedFields(UpdateDetect1Dto dto)
    {
        var updatedFields = new List<string>();

        if (dto.DistanceBefore.HasValue) updatedFields.Add("distance_before");
        if (dto.Force.HasValue) updatedFields.Add("force");
        if (dto.DistanceAfter.HasValue) updatedFields.Add("distance_after");
        if (dto.DistanceResult.HasValue) updatedFields.Add("distance_result");
        if (dto.UsingTime.HasValue) updatedFields.Add("using_time");
        if (!string.IsNullOrEmpty(dto.InspectorId)) updatedFields.Add("inspector");
        if (!string.IsNullOrEmpty(dto.Remarks)) updatedFields.Add("remarks");
        if (dto.IfQualified.HasValue) updatedFields.Add("if_qualified");

        return updatedFields;
    }
}