using System.Text.Json;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using RestSharp;

namespace DexRobotPDA.Services;

public class ProcessOneService : BaseService
{
    public ProcessOneService(
        RestClient restClient,
        ILogger<AuthService> logger,ILocalStorageService localStorage) 
        : base(restClient, logger, localStorage)
    {
    }
    
    public async Task<List<MotorDto>?> GetFinishedList(string taskId)
    {
        var request = new RestRequest("api/Motor/GetFinishedList");
        request.AddParameter("taskId", taskId);
        return await ExecuteRequest<List<MotorDto>>(request);
    }
    public async Task<ApiResponse> AddMotor(AddMotorDto motorDto)
    {
        var request = new RestRequest("api/Motor/AddMotor", Method.Post);
    
        // 使用JSON格式发送电机数据
        request.AddJsonBody(motorDto);
    
        _logger.LogInformation("尝试新增电机 - 电机ID: {MotorId}", motorDto.motor_id);
        var apiResponse = await ExecuteCommand(request);
    
        // 序列化并打印响应
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("API响应内容:");
        Console.WriteLine(responseJson);

        // 新增成功时处理返回数据
        if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
        {
            var motorData = JsonSerializer.Serialize(apiResponse.ResultData);
            var createdMotor = JsonSerializer.Deserialize<MotorDto>(motorData);
            _logger.LogInformation("电机新增成功 - 电机ID: {MotorId}", createdMotor?.motor_id);
        }
        else
        {
            _logger.LogWarning("电机新增失败 - 电机ID: {MotorId}, 错误信息: {Msg}", 
                motorDto.motor_id, apiResponse.Msg);
        }

        return apiResponse;
    }
}