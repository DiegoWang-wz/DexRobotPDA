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
public class Detect2Controller : ControllerBase
{
    private readonly DailyDbContext db;
    private readonly IMapper mapper;
    private readonly ILogger<Detect2Controller> _logger;

    public Detect2Controller(DailyDbContext _db, IMapper _mapper, ILogger<Detect2Controller> logger)
    {
        db = _db;
        mapper = _mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSplitWormDetect(string split_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 1. 验证输入参数
            if (string.IsNullOrEmpty(split_id))
            {
                response.ResultCode = -1;
                response.Msg = "分指机构ID不能为空";
                _logger.LogWarning("获取检测记录失败：分指机构ID为空");
                return BadRequest(response);
            }

            // 2. 根据split_id查询，按id降序排序取第一条（最新记录）
            var latestDetect = await db.Detect2
                .Where(d => d.split_id == split_id)
                .OrderByDescending(d => d.id)
                .FirstOrDefaultAsync();

            // 3. 处理查询结果
            if (latestDetect == null)
            {
                response.ResultCode = 0;
                response.Msg = $"未找到分指机构ID为 '{split_id}' 的检测记录";
                _logger.LogInformation("未找到分指机构 {SplitId} 的检测记录", split_id);
            }
            else
            {
                // 映射为DTO返回
                var detectDto = mapper.Map<SplitWormDetectDto>(latestDetect);
                response.ResultCode = 1;
                response.Msg = "Success";
                response.ResultData = detectDto;
                _logger.LogInformation("成功获取分指机构 {SplitId} 的最新检测记录，检测ID: {DetectId}",
                    split_id, latestDetect.id);
            }
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "获取检测记录失败";
            _logger.LogError(e, "获取分指机构 {SplitId} 的检测记录时发生错误", split_id);
        }

        return Ok(response);
    }
    
    [HttpGet]
    public IActionResult GetSplitWormDetectList(string task_id)
    {
        ApiResponse response = new ApiResponse();
        try
        {
            // 根据task_id查询相关的SplitWormDetect记录
            var list = db.Detect2
                .Join(db.Splits,
                    detect => detect.split_id,
                    split => split.split_id,
                    (detect, split) => new { Detect = detect, Split = split })
                .Where(x => x.Split.task_id == task_id)
                .Select(x => x.Detect)
                .ToList();

            _logger.LogDebug("从数据库获取到{Count}条记录", list.Count);

            List<SplitWormDetectDto> Detects = mapper.Map<List<SplitWormDetectDto>>(list);
            response.ResultCode = 1;
            response.Msg = "Success";
            response.ResultData = Detects;

            // 记录成功信息
            _logger.LogInformation("成功获取，共{Count}条记录", Detects.Count);
        }
        catch (Exception e)
        {
            response.ResultCode = -1;
            response.Msg = "Error";

            // 记录错误信息，包括异常详情
            _logger.LogError(e, "获取列表时发生错误");
        }

        return Ok(response);
    }


    // [HttpPost]
    // public async Task<IActionResult> AddDetect2(AddDetect2Dto addDetectDto)
    // {
    //     ApiResponse response = new ApiResponse();
    //     try
    //     {
    //         // 1. 基础参数验证
    //         if (string.IsNullOrEmpty(addDetectDto.split_id))
    //         {
    //             response.ResultCode = -1;
    //             response.Msg = "分指机构ID不能为空";
    //             _logger.LogWarning("新增检测记录失败：分指机构ID为空");
    //             return BadRequest(response);
    //         }
    //
    //         // 2. 检查关联的分指机构是否存在（确保外键有效）
    //         bool splitExists = await db.Splits.AnyAsync(m => m.split_id == addDetectDto.split_id);
    //         if (!splitExists)
    //         {
    //             response.ResultCode = -1;
    //             response.Msg = $"分指机构ID '{addDetectDto.split_id}' 不存在，无法创建检测记录";
    //             _logger.LogWarning("新增检测记录失败：分指机构不存在 - {SplitId}", addDetectDto.split_id);
    //             return BadRequest(response);
    //         }
    //
    //         // 3. 使用AutoMapper将DTO转换为实体
    //         var detectModel = mapper.Map<SplitWormDetectModel>(addDetectDto);
    //
    //         // 5. 添加到数据库并保存
    //         await db.Detect2.AddAsync(detectModel);
    //         await db.SaveChangesAsync();
    //
    //         // 6. 记录成功日志
    //         _logger.LogInformation("成功新增检测记录，检测ID: {DetectId}, 分指机构ID: {SplitId}",
    //             detectModel.id, addDetectDto.split_id);
    //
    //         // 7. 构建成功响应
    //         response.ResultCode = 1;
    //         response.Msg = "新增检测记录成功";
    //         response.ResultData = new
    //         {
    //             detect_id = detectModel.id,
    //             split_id = detectModel.split_id,
    //             combine_time = detectModel.combine_time
    //         };
    //
    //         return Ok(response);
    //     }
    //     catch (DbUpdateException dbEx)
    //     {
    //         // 处理数据库相关异常（如外键约束错误）
    //         string errorMsg = "数据库操作失败";
    //         if (dbEx.InnerException is SqlException sqlEx)
    //         {
    //             // 外键约束错误（例如关联的分指机构不存在，虽然上面已做检查，但防止并发问题）
    //             if (sqlEx.Number == 547)
    //             {
    //                 errorMsg = $"关联数据不存在（分指机构ID: {addDetectDto.split_id}）";
    //             }
    //             // 唯一键约束错误（如果表中有唯一索引）
    //             else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
    //             {
    //                 errorMsg = "检测记录已存在，不能重复添加";
    //             }
    //         }
    //
    //         response.ResultCode = -1;
    //         response.Msg = errorMsg;
    //         _logger.LogError(dbEx, "新增检测记录时数据库操作失败，分指机构ID: {SplitId}", addDetectDto?.split_id);
    //
    //         return BadRequest(response);
    //     }
    //     catch (Exception ex)
    //     {
    //         // 处理其他未知异常
    //         response.ResultCode = -1;
    //         response.Msg = "新增检测记录失败";
    //         _logger.LogError(ex, "新增检测记录时发生错误，分指机构ID: {SplitId}", addDetectDto?.split_id);
    //
    //         return StatusCode(500, response);
    //     }
    // }
    //
    // [HttpPut]
    // public async Task<IActionResult> UpdateLatestDetect(SplitWormDetectDto dto)
    // {
    //     if (!ModelState.IsValid)
    //     {
    //         var errors = ModelState.Values
    //             .SelectMany(v => v.Errors)
    //             .Select(e => e.ErrorMessage)
    //             .ToList();
    //
    //         _logger.LogWarning("模型绑定失败，错误: {Errors}", string.Join(", ", errors));
    //
    //         ApiResponse response1 = new ApiResponse
    //         {
    //             ResultCode = -1,
    //             Msg = "请求数据格式错误: " + string.Join(", ", errors)
    //         };
    //         return BadRequest(response1);
    //     }
    //
    //     ApiResponse response = new ApiResponse();
    //     try
    //     {
    //         if (string.IsNullOrEmpty(dto.split_id))
    //         {
    //             response.ResultCode = -1;
    //             response.Msg = "分指机构ID不能为空";
    //             _logger.LogWarning("更新检测记录失败：分指机构ID为空");
    //             return BadRequest(response);
    //         }
    //
    //         var latestDetect = await db.Detect2
    //             .Where(d => d.split_id == dto.split_id)
    //             .OrderByDescending(d => d.id)
    //             .FirstOrDefaultAsync();
    //
    //         if (latestDetect == null)
    //         {
    //             response.ResultCode = -1;
    //             response.Msg = $"分指机构ID '{dto.split_id}' 不存在检测记录，无法更新";
    //             _logger.LogWarning("更新检测记录失败：未找到分指机构 {SplitId} 的检测记录", dto.split_id);
    //             return BadRequest(response);
    //         }
    //
    //         // 直接使用DTO字段更新实体，保持字段名一致
    //         if (dto.distance_before.HasValue)
    //             latestDetect.distance_before = dto.distance_before;
    //
    //         if (dto.force.HasValue)
    //             latestDetect.force = dto.force;
    //
    //         if (dto.distance_after.HasValue)
    //             latestDetect.distance_after = dto.distance_after;
    //
    //         if (dto.distance_result.HasValue)
    //             latestDetect.distance_result = dto.distance_result;
    //         else if (dto.distance_before.HasValue && dto.distance_after.HasValue)
    //             // 计算距离结果时添加四舍五入
    //             latestDetect.distance_result = Math.Round(
    //                 dto.distance_before.Value - dto.distance_after.Value,
    //                 2
    //             );
    //
    //         if (dto.using_time.HasValue)
    //             latestDetect.using_time = dto.using_time;
    //
    //         if (!string.IsNullOrEmpty(dto.inspector_id))
    //             latestDetect.inspector_id = dto.inspector_id;
    //
    //         if (!string.IsNullOrEmpty(dto.remarks))
    //             latestDetect.remarks = dto.remarks;
    //
    //         if (dto.if_qualified == true || dto.if_qualified == false)
    //         {
    //             latestDetect.if_qualified = dto.if_qualified;
    //         }
    //         else
    //         {
    //             // 自动判断合格状态的逻辑保持不变
    //             bool isQualified = false;
    //             if (latestDetect.distance_before.HasValue && latestDetect.distance_after.HasValue)
    //             {
    //                 double distanceDiff = latestDetect.distance_before.Value - latestDetect.distance_after.Value;
    //                 if (distanceDiff <= 0.02)
    //                 {
    //                     if (latestDetect.combine_time.HasValue && latestDetect.using_time.HasValue)
    //                     {
    //                         TimeSpan timeDiff = latestDetect.using_time.Value - latestDetect.combine_time.Value;
    //                         if (timeDiff.TotalHours > 72)
    //                         {
    //                             isQualified = true;
    //                         }
    //                     }
    //                 }
    //             }
    //
    //             latestDetect.if_qualified = isQualified;
    //         }
    //
    //         // 保存更新
    //         db.Detect2.Update(latestDetect);
    //         await db.SaveChangesAsync();
    //
    //         // 记录日志并返回结果
    //         _logger.LogInformation("成功更新分指机构 {SplitId} 的最新检测记录，检测ID: {DetectId}",
    //             dto.split_id, latestDetect.id);
    //
    //         response.ResultCode = 1;
    //         response.Msg = "检测记录更新成功";
    //         response.ResultData = new
    //         {
    //             detect_id = latestDetect.id,
    //             split_id = latestDetect.split_id,
    //             if_qualified = latestDetect.if_qualified
    //         };
    //
    //         return Ok(response);
    //     }
    //     catch (DbUpdateException dbEx)
    //     {
    //         string errorMsg = "数据库操作失败";
    //         if (dbEx.InnerException is SqlException sqlEx)
    //         {
    //             _logger.LogError(sqlEx, "更新检测记录时发生数据库错误，错误编号: {ErrorCode}", sqlEx.Number);
    //         }
    //
    //         response.ResultCode = -1;
    //         response.Msg = errorMsg;
    //         _logger.LogError(dbEx, "更新分指机构 {SplitId} 的检测记录时数据库操作失败", dto?.split_id);
    //
    //         return BadRequest(response);
    //     }
    //     catch (Exception ex)
    //     {
    //         response.ResultCode = -1;
    //         response.Msg = "更新检测记录失败";
    //         _logger.LogError(ex, "更新分指机构 {SplitId} 的检测记录时发生错误", dto?.split_id);
    //
    //         return StatusCode(500, response);
    //     }
    // }
}