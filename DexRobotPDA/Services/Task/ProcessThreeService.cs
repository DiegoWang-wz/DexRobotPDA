using System.Text.Json;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using DexRobotPDA.ApiResponses;
using DexRobotPDA.DTOs;
using RestSharp;

namespace DexRobotPDA.Services;

public class ProcessThreeService : BaseService
{
    public ProcessThreeService(
        RestClient restClient,
        ILogger<AuthService> logger,ILocalStorageService localStorage) 
        : base(restClient, logger, localStorage)
    {
    }
    
    public async Task<List<PalmDto>?> GetPalmList(string taskId)
    {
        var request = new RestRequest("api/Palm/GetPalmList");
        request.AddParameter("taskId", taskId);
        return await ExecuteRequest<List<PalmDto>>(request);
    }

    public async Task<ApiResponse> AddPalm(AddPalmDto palmDto)
    {
        var request = new RestRequest("api/Palm/AddPalm", Method.Post);

        // 使用JSON格式发送手掌数据
        request.AddJsonBody(palmDto);

        _logger.LogInformation("尝试新增手掌 - 手掌ID: {PalmId}", palmDto.palm_id);
        var apiResponse = await ExecuteCommand(request);

        // 序列化并打印响应
        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("API响应内容:");
        Console.WriteLine(responseJson);

        // 新增成功时处理返回数据
        if (apiResponse.ResultCode == 1 && apiResponse.ResultData != null)
        {
            var palmData = JsonSerializer.Serialize(apiResponse.ResultData);
            // 解析响应数据中的palm对象
            using (var doc = JsonDocument.Parse(palmData))
            {
                var palmJson = doc.RootElement.GetProperty("palm").GetRawText();
                var createdPalm = JsonSerializer.Deserialize<PalmDto>(palmJson);
                _logger.LogInformation("手掌新增成功 - 手掌ID: {PalmId}", createdPalm?.palm_id);
            }
        }
        else
        {
            _logger.LogWarning("手掌新增失败 - 手掌ID: {PalmId}, 错误信息: {Msg}", 
                palmDto.palm_id, apiResponse.Msg);
        }

        return apiResponse;
    }

    public async Task<ApiResponse> FingerBindPalm(string finger_id, string palm_id)
    {
        var request = new RestRequest("api/Finger/FingerBindPalm", Method.Post);
        request.AddJsonBody(new { 
            finger_id = finger_id, 
            palm_id = palm_id
        });
        
        var apiResponse = await ExecuteCommand(request);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("更新任务流程状态API响应内容:");
        Console.WriteLine(responseJson);
        return apiResponse;
    }
    
    public async Task<ApiResponse> SplitBindPalm(string split_id, string palm_id)
    {
        var request = new RestRequest("api/Split/SplitBindPalm", Method.Post);
        request.AddJsonBody(new { 
            split_id = split_id, 
            palm_id = palm_id
        });
        
        var apiResponse = await ExecuteCommand(request);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string responseJson = JsonSerializer.Serialize(apiResponse, options);
        Console.WriteLine("更新任务流程状态API响应内容:");
        Console.WriteLine(responseJson);
        return apiResponse;
    }
}