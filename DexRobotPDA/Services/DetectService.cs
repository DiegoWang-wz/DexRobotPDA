using System.Text.Json;
using Blazored.LocalStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace DexRobotPDA.Services;

public class DetectService : BaseService
{
    private readonly ILogger<DetectService> _logger;

    public DetectService(
        RestClient restClient,
        ILogger<DetectService> logger,
        ILocalStorageService localStorage)
        : base(restClient, logger, localStorage)
    {
        _logger = logger;
    }


    public async Task<ApiResponse> AddDetect1(UpdateDetect1Dto detect1Dto)
    {
        try
        {
            // 发送创建检测记录的请求
            var detectRequest = new RestRequest("api/Detect1/AddDetect1", Method.Post);
            detectRequest.AddJsonBody(detect1Dto);

            _logger.LogInformation("开始为电机 {MotorId} 创建检测记录", detect1Dto.motor_id);
            var detectResponse = await ExecuteCommand(detectRequest);

            if (detectResponse.ResultCode == 1)
            {
                _logger.LogInformation("电机 {MotorId} 的检测记录创建成功", detect1Dto.motor_id);
            }
            else
            {
                _logger.LogWarning("电机 {MotorId} 的检测记录创建失败: {Message}", detect1Dto.motor_id, detectResponse.Msg);
            }
            return detectResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建电机 {MotorId} 的检测记录时发生异常", detect1Dto.motor_id);
            throw;
        }
    }

    public async Task<MotorDto> GetMotor(string motor_id)
    {
        var request = new RestRequest("api/Motor/GetMotor");
        request.AddParameter("motor_id", motor_id);
        return await ExecuteRequest<MotorDto>(request);
    }

    public async Task<ApiResponse> UpdateDetect1(MotorWormDetectDto detect1Dto)
    {
        var request = new RestRequest("api/Detect1/UpdateLatestDetect", Method.Put);

        // 先输出请求内容
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(detect1Dto, options);
        Console.WriteLine("=== 请求内容 ===");
        Console.WriteLine(requestJson);
        Console.WriteLine("==============");

        // 使用JSON格式发送检测数据
        request.AddJsonBody(detect1Dto);

        _logger.LogInformation("尝试更新检测记录 - 电机ID: {MotorId}", detect1Dto.motor_id);

        try
        {
            var apiResponse = await ExecuteCommand(request);

            // 序列化并打印响应
            string responseJson = JsonSerializer.Serialize(apiResponse, options);
            Console.WriteLine("=== API响应内容 ===");
            Console.WriteLine(responseJson);
            Console.WriteLine("=================");

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 电机ID: {MotorId}", detect1Dto.motor_id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 电机ID: {MotorId}, 错误信息: {Msg}",
                    detect1Dto.motor_id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求异常: {ex.Message}");
            Console.WriteLine($"异常堆栈: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<MotorWormDetectDto?> GetMotorWormDetect(string motor_id)
    {
        var request = new RestRequest("api/Detect1/GetMotorWormDetect");
        request.AddParameter("motor_id", motor_id);
        return await ExecuteRequest<MotorWormDetectDto>(request);
    }

    public async Task<List<MotorWormDetectDto>?> GetMotorWormDetectList(string task_id)
    {
        var request = new RestRequest("api/Detect1/GetMotorWormDetectList");
        request.AddParameter("task_id", task_id);
        return await ExecuteRequest<List<MotorWormDetectDto>>(request);
    }

    public async Task<List<SplitWormDetectDto>?> GetSplitWormDetectList(string task_id)
    {
        var request = new RestRequest("api/Detect2/GetSplitWormDetectList");
        request.AddParameter("task_id", task_id);
        return await ExecuteRequest<List<SplitWormDetectDto>>(request);
    }

    public async Task<ApiResponse> UpdateMotorQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Motor/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        Console.WriteLine("=== 请求内容 ===");
        Console.WriteLine(requestJson);
        Console.WriteLine("==============");
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);
            Console.WriteLine("=== API响应内容 ===");
            Console.WriteLine(responseJson);
            Console.WriteLine("=================");

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求异常: {ex.Message}");
            Console.WriteLine($"异常堆栈: {ex.StackTrace}");
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }

    public async Task<ApiResponse> UpdateFingerQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Finger/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        Console.WriteLine("=== 请求内容 ===");
        Console.WriteLine(requestJson);
        Console.WriteLine("==============");
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);
            Console.WriteLine("=== API响应内容 ===");
            Console.WriteLine(responseJson);
            Console.WriteLine("=================");

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求异常: {ex.Message}");
            Console.WriteLine($"异常堆栈: {ex.StackTrace}");
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }

    public async Task<ApiResponse> UpdatePalmQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Palm/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        Console.WriteLine("=== 请求内容 ===");
        Console.WriteLine(requestJson);
        Console.WriteLine("==============");
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);
            Console.WriteLine("=== API响应内容 ===");
            Console.WriteLine(responseJson);
            Console.WriteLine("=================");

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求异常: {ex.Message}");
            Console.WriteLine($"异常堆栈: {ex.StackTrace}");
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }

    public async Task<ApiResponse> UpdateSplitQualify(UpdateQualifyDto qualifyDto)
    {
        var request = new RestRequest("api/Split/UpdateQualify", Method.Put);
        var options = new JsonSerializerOptions { WriteIndented = true };
        string requestJson = JsonSerializer.Serialize(qualifyDto, options);
        Console.WriteLine("=== 请求内容 ===");
        Console.WriteLine(requestJson);
        Console.WriteLine("==============");
        request.AddJsonBody(qualifyDto);
        _logger.LogInformation("尝试更新检测记录 - 配件ID: {ID}", qualifyDto.id);
        try
        {
            var apiResponse = await ExecuteCommand(request);
            string responseJson = JsonSerializer.Serialize(apiResponse, options);
            Console.WriteLine("=== API响应内容 ===");
            Console.WriteLine(responseJson);
            Console.WriteLine("=================");

            if (apiResponse.ResultCode == 1)
            {
                _logger.LogInformation("检测记录更新成功 - 配件ID: {ID}", qualifyDto.id);
            }
            else
            {
                _logger.LogWarning("检测记录更新失败 - 配件ID: {ID}, 错误信息: {Msg}", qualifyDto.id, apiResponse.Msg);
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求异常: {ex.Message}");
            Console.WriteLine($"异常堆栈: {ex.StackTrace}");
            _logger.LogError(ex, "更新配件状态时发生异常 - 配件ID: {ID}", qualifyDto.id);
            throw;
        }
    }
}