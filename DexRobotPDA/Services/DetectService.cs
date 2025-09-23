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
    
    public async Task<ApiResponse> UpdateDetect1(UpdateDetect1Dto detect1Dto)
    {
        var request = new RestRequest("api/Detect1/UpdateLatestDetect", Method.Put);

        // 使用JSON格式发送检测数据
        request.AddJsonBody(detect1Dto);
        
        _logger.LogInformation("尝试更新检测记录 - 电机ID: {MotorId}", detect1Dto.MotorId);
        var apiResponse = await ExecuteCommand(request);

        // 序列化并打印响应
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("API响应内容:");
        Console.WriteLine(responseJson);

        if (apiResponse.ResultCode == 1)
        {
            _logger.LogInformation("检测记录更新成功 - 电机ID: {MotorId}", detect1Dto.MotorId);
        }
        else
        {
            _logger.LogWarning("检测记录更新失败 - 电机ID: {MotorId}, 错误信息: {Msg}",
                detect1Dto.MotorId, apiResponse.Msg);
        }

        return apiResponse;
    }
    
    public async Task<MotorWormDetectDto?> GetMotorWormDetect(string motor_id)
    {
        var request = new RestRequest("api/Detect1/GetMotorWormDetect");
        request.AddParameter("motor_id", motor_id);
        return await ExecuteRequest<MotorWormDetectDto>(request);
    }
}