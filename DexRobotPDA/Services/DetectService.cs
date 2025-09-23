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
}