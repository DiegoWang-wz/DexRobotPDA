using System.Text.Json;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using RestSharp;

namespace DexRobotPDA.Services;

public class ProcessTwoService : BaseService
{
    public ProcessTwoService(
        RestClient restClient,
        ILogger<AuthService> logger,ILocalStorageService localStorage) 
        : base(restClient, logger, localStorage)
    {
    }
    
    public async Task<List<FingerDto>?> GetFinishedList(string taskId)
    {
        var request = new RestRequest("api/Finger/GetFingerList");
        request.AddParameter("taskId", taskId);
        return await ExecuteRequest<List<FingerDto>>(request);
    }
    public async Task<ApiResponse> AddFinger(AddFingerDto fingerDto)
    {
        var request = new RestRequest("api/Finger/AddFinger", Method.Post);

        // 使用JSON格式发送手指数据
        request.AddJsonBody(fingerDto);

        _logger.LogInformation("尝试新增手指 - 手指ID: {FingerId}", fingerDto.finger_id);
        var apiResponse = await ExecuteCommand(request);

        // 序列化并打印响应
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("API响应内容:");
        Console.WriteLine(responseJson);

        // 新增成功时处理返回数据
        if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
        {
            var fingerData = JsonSerializer.Serialize(apiResponse.ResultData);
            // 解析响应数据中的finger对象
            using (var doc = JsonDocument.Parse(fingerData))
            {
                var fingerJson = doc.RootElement.GetProperty("finger").GetRawText();
                var createdFinger = JsonSerializer.Deserialize<FingerDto>(fingerJson);
                _logger.LogInformation("手指新增成功 - 手指ID: {FingerId}", createdFinger?.finger_id);
            }
        }
        else
        {
            _logger.LogWarning("手指新增失败 - 手指ID: {FingerId}, 错误信息: {Msg}", 
                fingerDto.finger_id, apiResponse.Msg);
        }

        return apiResponse;
    }
    public async Task<ApiResponse> MotorBindFinger(string motor_id, string finger_id)
    {
        var request = new RestRequest("api/Motor/MotorBindFinger", Method.Post);
        request.AddJsonBody(new { 
            motor_id = motor_id, 
            finger_id = finger_id
        });
        
        var apiResponse = await ExecuteCommand(request);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("更新任务流程状态API响应内容:");
        Console.WriteLine(responseJson);
        return apiResponse;
    }

}